using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PhaseUIManager : MonoBehaviour
{
    private answerQuestion answerScript;
    public ProgressBarManager progressBarManager; // arraste pelo Inspector
    public GameObject nextBTN;
    public bool questaoRespondida = false;

    [Header("Layouts")]
    public GameObject alternativa5;
    public GameObject texto;
    public GameObject feedback;

    [Header("Layout alternativa5")]
    public TMP_Text pergunta;
    public TMP_Text alternativa1Text;
    public TMP_Text alternativa2Text;
    public TMP_Text alternativa3Text;
    public TMP_Text alternativa4Text;
    public TMP_Text alternativa5Text;

    [Header("Botões alternativas")]
    public Button alternativa1Btn;
    public Button alternativa2Btn;
    public Button alternativa3Btn;
    public Button alternativa4Btn;
    public Button alternativa5Btn;

    [Header("Cores alternativas")]
    public Color corAlternativaNormal = Color.white;
    public Color corAlternativaAcerto = Color.green;
    public Color corAlternativaErro = Color.red;

    [Header("Layout texto")]
    public TMP_Text textoConteudo;

    [Header("Layout feedback")]
    public TMP_Text tempoFinalText;
    public TMP_Text desempenhoPercentText; // TMP para desempenho final
    public GameObject feedbackButtonPrefab;
    public Transform feedbackContainer;
    public Color corAcerto = Color.green;
    public Color corErro = Color.red;

    public int currentID = 0;
    private float tempoFase = 0f;
    private bool contandoTempo = false;

    private void Start()
    {
        answerScript = GetComponent<answerQuestion>();
        if (answerScript != null)
            answerScript.onRespostaRegistrada += OnRespostaRegistrada;

        tempoFase = 0f;
        contandoTempo = true;

        ShowItemByID(currentID);
    }

    private void Update()
    {
        if (contandoTempo)
            tempoFase += Time.deltaTime;
    }

    private void ResetAlternativas()
    {
        Button[] botoes = { alternativa1Btn, alternativa2Btn, alternativa3Btn, alternativa4Btn, alternativa5Btn };

        foreach (var btn in botoes)
        {
            btn.interactable = true;
            Image img = btn.GetComponent<Image>();
            if (img != null) img.color = corAlternativaNormal;
        }
    }

    public void ShowItemByID(int id)
    {
        questaoRespondida = false;
        ResetAlternativas();
        nextBTN.SetActive(false);

        if (PhaseManager.Instance == null || PhaseManager.Instance.currentPhase == null)
        {
            Debug.LogWarning("PhaseManager ou CurrentPhase não disponível!");
            return;
        }

        var fase = PhaseManager.Instance.currentPhase;
        PhaseItem item = fase.itens.Find(x => x.id == id);

        if (item == null)
        {
            ShowFeedbackScreen();
            return;
        }

        alternativa5.SetActive(item.tipo == "alternativa5");
        texto.SetActive(item.tipo == "texto");
        feedback.SetActive(false);

        if (item.tipo == "alternativa5")
        {
            pergunta.text = item.pergunta;

            if (item.opcoes != null && item.opcoes.Count >= 5)
            {
                alternativa1Text.text = item.opcoes[0];
                alternativa2Text.text = item.opcoes[1];
                alternativa3Text.text = item.opcoes[2];
                alternativa4Text.text = item.opcoes[3];
                alternativa5Text.text = item.opcoes[4];
            }
        }
        else if (item.tipo == "texto")
        {
            textoConteudo.text = item.conteudo;
            nextBTN.SetActive(true);
        }

        progressBarManager?.AtualizarProgress();
    }

    public void NextQuestion()
    {
        currentID++;
        ShowItemByID(currentID);
    }

    private void ShowFeedbackScreen()
    {
        contandoTempo = false;

        alternativa5.SetActive(false);
        texto.SetActive(false);
        feedback.SetActive(true);
        nextBTN.SetActive(false);

        // Tempo total
        if (tempoFinalText != null)
        {
            int minutos = Mathf.FloorToInt(tempoFase / 60f);
            int segundos = Mathf.FloorToInt(tempoFase % 60f);
            tempoFinalText.text = $"Tempo total: {minutos:D2}:{segundos:D2}";
        }

        // Desempenho final
        if (desempenhoPercentText != null && answerScript.Respostas.Count > 0)
        {
            int corretas = 0;
            foreach (var resp in answerScript.Respostas)
            {
                if (resp.respostaUsuario == resp.respostaCorreta)
                    corretas++;
            }
            float percentual = (float)corretas / answerScript.Respostas.Count * 100f;
            desempenhoPercentText.text = $"Desempenho: {percentual:F0}%";
        }

        // Limpa botões antigos
        foreach (Transform child in feedbackContainer)
            Destroy(child.gameObject);

        // Cria botões de feedback apenas para questões respondidas (tipo alternativa5)
        for (int i = 0; i < answerScript.Respostas.Count; i++)
        {
            var resp = answerScript.Respostas[i];
            GameObject btnObj = Instantiate(feedbackButtonPrefab, feedbackContainer);

            TMP_Text txtBotao = btnObj.GetComponentInChildren<TMP_Text>();
            if (txtBotao != null)
                txtBotao.text = $"{i + 1}"; // posição na lista de respostas

            Image img = btnObj.GetComponent<Image>();
            if (img != null)
                img.color = resp.respostaUsuario == resp.respostaCorreta ? corAcerto : corErro;

            // Botões de feedback não fazem nada
        }
    }

    private void OnRespostaRegistrada()
    {
        if (alternativa5.activeSelf)
            nextBTN.SetActive(true);

        // Feedback visual nas alternativas
        Button[] botoes = { alternativa1Btn, alternativa2Btn, alternativa3Btn, alternativa4Btn, alternativa5Btn };
        TMP_Text[] textos = { alternativa1Text, alternativa2Text, alternativa3Text, alternativa4Text, alternativa5Text };

        var ultimaResposta = answerScript.Respostas[answerScript.Respostas.Count - 1];

        for (int i = 0; i < botoes.Length; i++)
        {
            botoes[i].interactable = false;
            Image img = botoes[i].GetComponent<Image>();
            if (img == null) continue;

            if (textos[i].text == ultimaResposta.respostaUsuario)
            {
                img.color = ultimaResposta.respostaUsuario == ultimaResposta.respostaCorreta
                    ? corAlternativaAcerto
                    : corAlternativaErro;
            }
            else if (textos[i].text == ultimaResposta.respostaCorreta && ultimaResposta.respostaUsuario != ultimaResposta.respostaCorreta)
            {
                img.color = corAlternativaAcerto;
            }
        }

        progressBarManager?.AtualizarProgress();
    }

    // Métodos de clique
    public void OnClickAlternativa1() => answerScript.RegistrarResposta(alternativa1Text.text);
    public void OnClickAlternativa2() => answerScript.RegistrarResposta(alternativa2Text.text);
    public void OnClickAlternativa3() => answerScript.RegistrarResposta(alternativa3Text.text);
    public void OnClickAlternativa4() => answerScript.RegistrarResposta(alternativa4Text.text);
    public void OnClickAlternativa5() => answerScript.RegistrarResposta(alternativa5Text.text);
}
