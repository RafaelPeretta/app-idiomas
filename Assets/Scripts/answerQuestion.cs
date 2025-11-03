/*
using System;
using System.Collections.Generic;
using UnityEngine;

public class answerQuestion : MonoBehaviour
{
    private PhaseUIManager phaseUI;

    // Evento para avisar o UIManager que o jogador respondeu
    public Action onRespostaRegistrada;

    [System.Serializable]
    public class RespostaUsuario
    {
        public int idQuestao;
        public string respostaCorreta;
        public string respostaUsuario;
    }

    private List<RespostaUsuario> respostas = new List<RespostaUsuario>();

    // Getter público para feedback
    public List<RespostaUsuario> Respostas => respostas;

    private void Start()
    {
        phaseUI = GetComponent<PhaseUIManager>();

        if (phaseUI == null)
            Debug.LogError("[answerQuestion] Não foi possível encontrar o PhaseUIManager no mesmo objeto!");
    }

    public void RegistrarResposta(string respostaUsuario)
    {
        if (phaseUI == null)
            return;

        // Bloqueia se a questão já tiver sido respondida
        if (phaseUI.questaoRespondida)
            return;

        if (PhaseManager.Instance == null || PhaseManager.Instance.currentPhase == null)
        {
            Debug.LogWarning("[answerQuestion] PhaseManager não inicializado!");
            return;
        }

        var fase = PhaseManager.Instance.currentPhase;
        var itemAtual = fase.itens.Find(x => x.id == phaseUI.currentID);

        if (itemAtual == null)
        {
            Debug.LogWarning($"[answerQuestion] Questão ID {phaseUI.currentID} não encontrada!");
            return;
        }

        // Cria o registro da resposta
        var novaResposta = new RespostaUsuario
        {
            idQuestao = itemAtual.id,
            respostaCorreta = itemAtual.resposta,
            respostaUsuario = respostaUsuario
        };

        respostas.Add(novaResposta);
        Debug.Log($"[RESPOSTA] Questão {itemAtual.id} | Correta: {itemAtual.resposta} | Usuário: {respostaUsuario}");

        // Marca que a questão foi respondida
        phaseUI.questaoRespondida = true;

        // Notifica o PhaseUIManager que a resposta foi registrada
        onRespostaRegistrada?.Invoke();
    }
}
*/