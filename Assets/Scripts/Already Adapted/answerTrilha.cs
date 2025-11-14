using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class answerTrilha : MonoBehaviour
{
    private TrilhaUIManager trilhaUI;
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
        trilhaUI = GetComponent<TrilhaUIManager>();

        if (trilhaUI == null)
            Debug.LogError("[answerTrilha] Não foi possível encontrar o TrilhaUIManager no mesmo objeto!");
    }

    public void RegistrarResposta(string respostaUsuario)
    {
        if (trilhaUI == null || trilhaUI.questaoRespondida)
            return;

        if (TrilhaManager.Instance == null || TrilhaManager.Instance.currentTrilha == null)
        {
            Debug.LogWarning("[answerTrilha] TrilhaManager não inicializado!");
            return;
        }

        var trilha = TrilhaManager.Instance.currentTrilha;

        if (trilha.questoes == null || trilhaUI.currentID >= trilha.questoes.Count)
        {
            Debug.LogWarning($"[answerTrilha] Questão ID {trilhaUI.currentID} não encontrada!");
            return;
        }

        var itemAtual = trilha.questoes[trilhaUI.currentID];

        List<string> respostasCorretas = itemAtual.RespostaCorreta;
        bool estaCorreta = false;

        if (itemAtual.Tipo != "simplesEscrita")
        {
            if (respostasCorretas.Count > 0)
                estaCorreta = respostaUsuario == respostasCorretas[0];
        }
        else
        {
            string[] respostasUsuario = respostaUsuario.Split('|');
            estaCorreta = respostasUsuario.SequenceEqual(respostasCorretas);
        }

        var novaResposta = new RespostaUsuario
        {
            idQuestao = trilhaUI.currentID,
            respostaCorreta = respostasCorretas,
            respostaUsuario = respostaUsuario,
            habilidades = itemAtual.Habilidades
        };

        respostas.Add(novaResposta);

        Debug.Log($"[RESPOSTA] Q{trilhaUI.currentID} | Correta: {estaCorreta} | Habs: {string.Join(",", itemAtual.Habilidades)}");

        trilhaUI.questaoRespondida = true;
        onRespostaRegistrada?.Invoke();
    }
}
