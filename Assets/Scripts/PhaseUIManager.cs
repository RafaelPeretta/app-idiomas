using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Video;
using UnityEngine.Networking;

public class PhaseUIManager : MonoBehaviour
{
    private answerQuestion answerScript;
    public ProgressBarManager progressBarManager;
    public GameObject nextBTN;
    public bool questaoRespondida = false;

    [Header("Layouts")]
    public GameObject videoAlternativaLayout;
    public GameObject imagemAlternativaLayout;
    public GameObject simplesAlternativaLayout;
    public GameObject textoAlternativaLayout;
    public GameObject simplesEscritaLayout;

    [Header("Video Alternativa")]
    public VideoPlayer videoPlayer;
    public TMP_Text videoPerguntaText;
    public TMP_Text videoAlt1, videoAlt2, videoAlt3, videoAlt4;
    public Button videoBtn1, videoBtn2, videoBtn3, videoBtn4;

    [Header("Imagem Alternativa")]
    public Image imagemQuestao;
    public TMP_Text imagemPerguntaText;
    public TMP_Text imgAlt1, imgAlt2, imgAlt3, imgAlt4;
    public Button imgBtn1, imgBtn2, imgBtn3, imgBtn4;

    [Header("Simples Alternativa")]
    public TMP_Text simplesPerguntaText;
    public TMP_Text simAlt1, simAlt2, simAlt3, simAlt4;
    public Button simBtn1, simBtn2, simBtn3, simBtn4;

    [Header("Texto Alternativa")]
    public TMP_Text textoPerguntaText;
    public TMP_Text textoConteudoText;
    public TMP_Text textoAlt1, textoAlt2, textoAlt3, textoAlt4;
    public Button textoBtn1, textoBtn2, textoBtn3, textoBtn4;

    [Header("Simples Escrita")]
    public TMP_Text escritaPerguntaText;
    public TMP_Text escritaAlt1, escritaAlt2, escritaAlt3;
    public TMP_InputField escritaInput1, escritaInput2, escritaInput3;
    public Button escritaNextBtn;

    [Header("Cores")]
    public Color corAlternativaNormal = Color.white;
    public Color corAlternativaAcerto = Color.green;
    public Color corAlternativaErro = Color.red;

    [Header("Feedback")]
    public GameObject feedbackLayout;
    public TMP_Text feedbackTexto;

    [Header("Resultados")]
    public GameObject resultButtonPrefab;
    public Transform resultButtonsParent;

    public int currentID = 0;
    private List<QuestionData> questoes;

    private void Awake()
    {
        if (FirestorePhaseLoader.Instance != null)
            FirestorePhaseLoader.Instance.OnPhaseLoaded += OnPhaseLoaded;
    }

    private void Start()
    {
        answerScript = GetComponent<answerQuestion>();
        if (answerScript != null)
            answerScript.onRespostaRegistrada += OnRespostaRegistrada;

        if (PhaseManager.Instance != null && PhaseManager.Instance.currentPhase != null)
        {
            questoes = PhaseManager.Instance.currentPhase.diagnostica_6ano;
            ShowItemByID(currentID);
        }
    }

    private void OnPhaseLoaded()
    {
        questoes = PhaseManager.Instance.currentPhase.diagnostica_6ano;
        currentID = 0;
        ShowItemByID(currentID);
    }

    private void ResetLayouts()
    {
        videoAlternativaLayout.SetActive(false);
        imagemAlternativaLayout.SetActive(false);
        simplesAlternativaLayout.SetActive(false);
        textoAlternativaLayout.SetActive(false);
        simplesEscritaLayout.SetActive(false);
        nextBTN.SetActive(false);
    }

    public void ShowItemByID(int id)
    {
        questaoRespondida = false;
        ResetLayouts();

        if (questoes == null || questoes.Count == 0 || id >= questoes.Count)
        {
            if (feedbackLayout != null)
                feedbackLayout.SetActive(true);

            if (feedbackTexto != null)
                feedbackTexto.text = "Você concluiu todas as questões!";

            GerarResultadoBotoes();
            return;
        }

        if (feedbackLayout != null)
            feedbackLayout.SetActive(false);

        var item = questoes[id];

        switch (item.Tipo)
        {
            case "videoAlternativa":
                videoAlternativaLayout.SetActive(true);
                if (videoPlayer != null && !string.IsNullOrEmpty(item.Midia))
                {
                    videoPlayer.url = item.Midia;
                    videoPlayer.Play();
                }
                videoPerguntaText.text = item.Questao;
                PreencherAlternativas(videoAlt1, videoAlt2, videoAlt3, videoAlt4, item.Alternativas);
                ConfigurarBotoes(videoBtn1, videoBtn2, videoBtn3, videoBtn4, videoAlt1, videoAlt2, videoAlt3, videoAlt4, item);
                break;

            case "imagemAlternativa":
                imagemAlternativaLayout.SetActive(true);
                imagemPerguntaText.text = item.Questao;

                if (!string.IsNullOrEmpty(item.Midia))
                    StartCoroutine(CarregarImagemDeURL(item.Midia, imagemQuestao));
                else
                    imagemQuestao.sprite = null;

                PreencherAlternativas(imgAlt1, imgAlt2, imgAlt3, imgAlt4, item.Alternativas);
                ConfigurarBotoes(imgBtn1, imgBtn2, imgBtn3, imgBtn4, imgAlt1, imgAlt2, imgAlt3, imgAlt4, item);
                break;

            case "simplesAlternativa":
                simplesAlternativaLayout.SetActive(true);
                simplesPerguntaText.text = item.Questao;
                PreencherAlternativas(simAlt1, simAlt2, simAlt3, simAlt4, item.Alternativas);
                ConfigurarBotoes(simBtn1, simBtn2, simBtn3, simBtn4, simAlt1, simAlt2, simAlt3, simAlt4, item);
                break;

            case "textoAlternativa":
                textoAlternativaLayout.SetActive(true);
                textoPerguntaText.text = item.Questao;
                textoConteudoText.text = item.Texto;
                PreencherAlternativas(textoAlt1, textoAlt2, textoAlt3, textoAlt4, item.Alternativas);
                ConfigurarBotoes(textoBtn1, textoBtn2, textoBtn3, textoBtn4, textoAlt1, textoAlt2, textoAlt3, textoAlt4, item);
                break;

            case "simplesEscrita":
                simplesEscritaLayout.SetActive(true);
                escritaPerguntaText.text = item.Questao;

                if (item.Alternativas != null && item.Alternativas.Count >= 3)
                {
                    escritaAlt1.text = item.Alternativas[0];
                    escritaAlt2.text = item.Alternativas[1];
                    escritaAlt3.text = item.Alternativas[2];
                }

                escritaInput1.text = "";
                escritaInput2.text = "";
                escritaInput3.text = "";

                escritaNextBtn.onClick.RemoveAllListeners();
                escritaNextBtn.onClick.AddListener(() =>
                {
                    if (!string.IsNullOrEmpty(escritaInput1.text) &&
                        !string.IsNullOrEmpty(escritaInput2.text) &&
                        !string.IsNullOrEmpty(escritaInput3.text))
                    {
                        string respostaComposta = $"{escritaInput1.text}|{escritaInput2.text}|{escritaInput3.text}";
                        answerScript.RegistrarResposta(respostaComposta);

                        questaoRespondida = true;
                        nextBTN.SetActive(true);
                        progressBarManager?.AtualizarProgress();
                    }
                });
                break;
        }

        progressBarManager?.AtualizarProgress();
    }

    private IEnumerator CarregarImagemDeURL(string url, Image destino)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[PhaseUIManager] Erro ao carregar imagem: {request.error}");
            }
            else
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                destino.sprite = sprite;
                Debug.Log("[PhaseUIManager] Imagem carregada com sucesso da URL.");
            }
        }
    }

    private void PreencherAlternativas(TMP_Text alt1, TMP_Text alt2, TMP_Text alt3, TMP_Text alt4, List<string> opcoes)
    {
        if (opcoes == null || opcoes.Count < 4) return;
        alt1.text = opcoes[0];
        alt2.text = opcoes[1];
        alt3.text = opcoes[2];
        alt4.text = opcoes[3];
    }

    private void ConfigurarBotoes(Button btn1, Button btn2, Button btn3, Button btn4,
                                  TMP_Text alt1, TMP_Text alt2, TMP_Text alt3, TMP_Text alt4,
                                  QuestionData item)
    {
        Button[] botoes = { btn1, btn2, btn3, btn4 };
        TMP_Text[] textos = { alt1, alt2, alt3, alt4 };

        for (int i = 0; i < botoes.Length; i++)
        {
            botoes[i].interactable = true;
            Image img = botoes[i].GetComponent<Image>();
            if (img != null) img.color = corAlternativaNormal;

            int index = i;
            botoes[i].onClick.RemoveAllListeners();
            botoes[i].onClick.AddListener(() =>
            {
                answerScript.RegistrarResposta(textos[index].text);
                AtualizarCoresBotoes(botoes, textos, item, textos[index].text);
                questaoRespondida = true;
                nextBTN.SetActive(true);
                progressBarManager?.AtualizarProgress();
            });
        }
    }

    private void AtualizarCoresBotoes(Button[] botoes, TMP_Text[] textos, QuestionData item, string respostaUsuario)
    {
        string correta = "";

        if (item.Tipo != "simplesEscrita" && item.RespostaCorreta is List<object> lista && lista.Count > 0)
        {
            correta = lista[0]?.ToString();
        }
        else
        {
            correta = item.RespostaCorreta?.ToString();
        }

        for (int i = 0; i < botoes.Length; i++)
        {
            botoes[i].interactable = false;
            Image img = botoes[i].GetComponent<Image>();
            if (img == null) continue;

            if (textos[i].text == respostaUsuario)
                img.color = respostaUsuario == correta ? corAlternativaAcerto : corAlternativaErro;
            else if (textos[i].text == correta && respostaUsuario != correta)
                img.color = corAlternativaAcerto;
        }
    }

    public void NextQuestion()
    {
        currentID++;
        ShowItemByID(currentID);
    }

    private void OnRespostaRegistrada()
    {
        progressBarManager?.AtualizarProgress();
    }

    private void GerarResultadoBotoes()
    {
        if (resultButtonsParent == null || resultButtonPrefab == null || answerScript == null)
            return;

        foreach (Transform child in resultButtonsParent)
            Destroy(child.gameObject);

        foreach (var r in answerScript.Respostas)
        {
            GameObject btnObj = Instantiate(resultButtonPrefab, resultButtonsParent);
            var btnScript = btnObj.GetComponent<QuestionResultButton>();

            string corretaStr = "";
            bool estaCorreta = false;

            if (r.respostaCorreta is List<object> lista && lista.Count > 0)
            {
                corretaStr = lista[0]?.ToString();
                estaCorreta = r.respostaUsuario == corretaStr;
            }
            else
            {
                corretaStr = r.respostaCorreta?.ToString();
                estaCorreta = r.respostaUsuario == corretaStr;
            }

            btnScript.Configurar(r.idQuestao, r.respostaUsuario, corretaStr, r.habilidade, estaCorreta);
        }
    }
}
