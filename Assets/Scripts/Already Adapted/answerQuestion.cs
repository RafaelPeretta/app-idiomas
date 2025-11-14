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
        public List<string> respostaCorreta;   // SEMPRE LISTA
        public string respostaUsuario;
        public List<string> habilidades;       // SEMPRE LISTA
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

        List<string> respostasCorretas = itemAtual.RespostaCorreta;
        bool estaCorreta = false;

        // 🔹 Questões de alternativa (apenas 1 correta)
        if (itemAtual.Tipo != "simplesEscrita")
        {
            if (respostasCorretas.Count > 0)
                estaCorreta = respostaUsuario == respostasCorretas[0];
        }
        else
        {
            // 🔹 Questões de escrita: "parte1|parte2|parte3"
            string[] respostasUsuario = respostaUsuario.Split('|');
            estaCorreta = respostasUsuario.SequenceEqual(respostasCorretas);
        }

        // 🔹 Salvar resposta
        var novaResposta = new RespostaUsuario
        {
            idQuestao = phaseUI.currentID,
            respostaCorreta = respostasCorretas,
            respostaUsuario = respostaUsuario,
            habilidades = itemAtual.Habilidades
        };

        respostas.Add(novaResposta);

        Debug.Log($"[RESPOSTA] Q{phaseUI.currentID} | Correta: {estaCorreta} | Habs: {string.Join(",", itemAtual.Habilidades)}");

        phaseUI.questaoRespondida = true;
        onRespostaRegistrada?.Invoke();

        // Última questão → finaliza
        if (phaseUI.currentID >= fase.diagnostica_6ano.Count - 1)
        {
            FinalizarFase();
        }
    }

    public void FinalizarFase()
    {
        Debug.Log("[answerQuestion] Todas as respostas foram registradas. Gerando feedback...");

        // UI visual de feedback final
        HabilidadeFeedbackUI feedbackUI = GetComponent<HabilidadeFeedbackUI>();
        if (feedbackUI != null)
            feedbackUI.GerarFeedbackVisual(respostas);

        // 🔹 Cada questão pode ter várias habilidades → expandimos
        var agrupadas =
            respostas.SelectMany(r => r.habilidades.Select(h => new { hab = h, resp = r }))
                     .GroupBy(x => x.hab);

        List<string> habilidadesAprovadas = new List<string>();

        foreach (var grupo in agrupadas)
        {
            int total = grupo.Count();
            int acertos = 0;

            foreach (var item in grupo)
            {
                var r = item.resp;
                bool estaCorreta = false;

                // Questão escrita
                if (r.respostaUsuario.Contains("|"))
                {
                    string[] userSplit = r.respostaUsuario.Split('|');
                    estaCorreta = userSplit.SequenceEqual(r.respostaCorreta);
                }
                else
                {
                    // Questão de alternativas
                    estaCorreta = r.respostaUsuario == r.respostaCorreta[0];
                }

                if (estaCorreta)
                    acertos++;
            }

            float taxa = (float)acertos / total;
            bool aprovado = taxa >= 0.66f;

            Debug.Log($"[Habilidade] {grupo.Key} = {acertos}/{total} → {(aprovado ? "APROVADO" : "REPROVADO")}");

            if (aprovado)
                habilidadesAprovadas.Add(grupo.Key);
        }

        // ------------------- SALVAR NO FIRESTORE -------------------
        if (habilidadesAprovadas.Count == 0)
        {
            Debug.Log("[answerQuestion] Nenhuma habilidade aprovada.");
            return;
        }

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        string userId = UserDataManager.userInstance.GetUserId();

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("[answerQuestion] Usuário não autenticado.");
            return;
        }

        DocumentReference userDoc = db.Collection("Users").Document(userId);

        userDoc.UpdateAsync("habilidades", FieldValue.ArrayUnion(habilidadesAprovadas.ToArray()))
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                    Debug.Log("[answerQuestion] ✅ Habilidades salvas.");
                else
                    Debug.LogError($"[answerQuestion] ❌ Erro ao salvar habilidades: {task.Exception}");
            });
    }
}
