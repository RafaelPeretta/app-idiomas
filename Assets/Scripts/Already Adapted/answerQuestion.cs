using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;

public class answerQuestion : MonoBehaviour
{
    private PhaseUIManager phaseUI;
    public Action onRespostaRegistrada;

    [Serializable]
    public class RespostaUsuario
    {
        public int idQuestao;
        public object respostaCorreta;
        public string respostaUsuario;
        public string habilidade;
    }

    private List<RespostaUsuario> respostas = new List<RespostaUsuario>();
    public List<RespostaUsuario> Respostas => respostas;

    private void Start()
    {
        phaseUI = GetComponent<PhaseUIManager>();

        if (phaseUI == null)
            Debug.LogError("[answerQuestion] Não foi possível encontrar o PhaseUIManager no mesmo objeto!");
    }

    public void RegistrarResposta(string respostaUsuario)
    {
        if (phaseUI == null || phaseUI.questaoRespondida)
            return;

        if (PhaseManager.Instance == null || PhaseManager.Instance.currentPhase == null)
        {
            Debug.LogWarning("[answerQuestion] PhaseManager não inicializado!");
            return;
        }

        var fase = PhaseManager.Instance.currentPhase;
        if (fase.diagnostica_6ano == null || phaseUI.currentID >= fase.diagnostica_6ano.Count)
        {
            Debug.LogWarning($"[answerQuestion] Questão ID {phaseUI.currentID} não encontrada!");
            return;
        }

        var itemAtual = fase.diagnostica_6ano[phaseUI.currentID];
        string respostaUsuarioStr = respostaUsuario;
        bool estaCorreta = false;

        // Verificação de resposta correta
        if (itemAtual.Tipo != "simplesEscrita")
        {
            if (itemAtual.RespostaCorreta is List<object> lista && lista.Count > 0)
            {
                string respostaCorretaStr = lista[0]?.ToString();
                estaCorreta = respostaUsuarioStr == respostaCorretaStr;
            }
        }
        else
        {
            if (itemAtual.RespostaCorreta is List<object> lista && lista.Count >= 3)
            {
                string[] respostasCorretas = lista.Select(x => x?.ToString()).ToArray();
                string[] respostasUsuario = respostaUsuario.Split('|');
                estaCorreta = respostasUsuario.SequenceEqual(respostasCorretas);
            }
        }

        var novaResposta = new RespostaUsuario
        {
            idQuestao = phaseUI.currentID,
            respostaCorreta = itemAtual.RespostaCorreta,
            respostaUsuario = respostaUsuarioStr,
            habilidade = itemAtual.Habilidades
        };

        respostas.Add(novaResposta);

        Debug.Log($"[RESPOSTA] Questão {phaseUI.currentID} | Correta: {estaCorreta} | Habilidade: {itemAtual.Habilidades}");

        phaseUI.questaoRespondida = true;
        onRespostaRegistrada?.Invoke();

        // Finaliza se for a última questão
        if (phaseUI.currentID >= fase.diagnostica_6ano.Count - 1)
        {
            FinalizarFase();
        }
    }

    public void FinalizarFase()
    {
        Debug.Log("[answerQuestion] Todas as respostas foram registradas. Gerando feedback...");

        // Exibe feedback visual, se houver
        HabilidadeFeedbackUI feedbackUI = GetComponent<HabilidadeFeedbackUI>();
        if (feedbackUI != null)
            feedbackUI.GerarFeedbackVisual(respostas);

        // Agrupa respostas por habilidade
        var agrupadas = respostas.GroupBy(r => r.habilidade);

        // Lista final de habilidades aprovadas (≥ 66% de acertos)
        List<string> habilidadesAprovadas = new List<string>();

        foreach (var grupo in agrupadas)
        {
            int total = grupo.Count();
            int acertos = 0;

            foreach (var r in grupo)
            {
                bool estaCorreta = false;

                if (r.respostaCorreta is List<object> lista && lista.Count > 0)
                {
                    // Caso a resposta do usuário tenha múltiplos valores (separados por "|")
                    if (r.respostaUsuario.Contains("|"))
                    {
                        string[] respostasUsuario = r.respostaUsuario.Split('|');
                        string[] respostasCorretas = lista.Select(x => x?.ToString()).ToArray();
                        estaCorreta = respostasUsuario.SequenceEqual(respostasCorretas);
                    }
                    else
                    {
                        // Caso simples (única resposta)
                        estaCorreta = r.respostaUsuario == lista[0]?.ToString();
                    }
                }

                if (estaCorreta)
                    acertos++;
            }

            float taxa = (float)acertos / total;
            bool aprovado = taxa >= 0.66f;

            Debug.Log($"[answerQuestion] Habilidade {grupo.Key}: {acertos}/{total} acertos ({taxa:P0}) - {(aprovado ? "✅ APROVADA" : "❌ REPROVADA")}");

            if (aprovado)
                habilidadesAprovadas.Add(grupo.Key);
        }

        if (habilidadesAprovadas.Count == 0)
        {
            Debug.Log("[answerQuestion] Nenhuma habilidade aprovada para salvar no Firestore.");
            return;
        }

        // 🔹 Salva as habilidades aprovadas no Firestore
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        string userId = UserDataManager.userInstance.GetUserId();

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("[answerQuestion] Usuário não autenticado — não foi possível salvar habilidades.");
            return;
        }

        DocumentReference userDoc = db.Collection("Users").Document(userId);
        Debug.Log($"[answerQuestion] Salvando {habilidadesAprovadas.Count} habilidades aprovadas...");

        userDoc.UpdateAsync("habilidades", FieldValue.ArrayUnion(habilidadesAprovadas.ToArray()))
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                    Debug.Log("[answerQuestion] ✅ Habilidades aprovadas adicionadas com sucesso!");
                else
                    Debug.LogError($"[answerQuestion] ❌ Erro ao salvar habilidades: {task.Exception}");
            });
    }

}
