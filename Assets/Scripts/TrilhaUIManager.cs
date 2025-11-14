using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

[System.Serializable]
public class TrilhaLoad_
{
    public string id;
    public List<string> habilidades;
}

public class TrilhaUIManager : MonoBehaviour
{
    private answerTrilha answerScript;
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
    public TMP_Text videoAlt1, videoAlt2, videoAlt3;
    public Button videoBtn1, videoBtn2, videoBtn3;

    [Header("Imagem Alternativa")]
    public Image imagemQuestao;
    public TMP_Text imagemPerguntaText;
    public TMP_Text imgAlt1, imgAlt2, imgAlt3;
    public Button imgBtn1, imgBtn2, imgBtn3;

    [Header("Simples Alternativa")]
    public TMP_Text simplesPerguntaText;
    public TMP_Text simAlt1, simAlt2, simAlt3;
    public Button simBtn1, simBtn2, simBtn3;

    [Header("Texto Alternativa")]
    public TMP_Text textoPerguntaText;
    public TMP_Text textoConteudoText;
    public TMP_Text textoAlt1, textoAlt2, textoAlt3;
    public Button textoBtn1, textoBtn2, textoBtn3;

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

    [Header("Trilhas")]
    public List<TrilhaLoad_> todasTrilhas;

    public int currentID = 0;
    private List<QuestionData> questoes;

    private void Awake()
    {
        if (TrilhaLoader.Instance != null)
            TrilhaLoader.Instance.OnTrilhaLoaded += OnTrilhaLoaded;
    }

    private void Start()
    {
        answerScript = GetComponent<answerTrilha>();
        if (answerScript != null)
            answerScript.onRespostaRegistrada += OnRespostaRegistrada;

        if (TrilhaManager.Instance != null && TrilhaManager.Instance.currentTrilha != null)
        {
            questoes = TrilhaManager.Instance.currentTrilha.questoes;
            ShowItemByID(currentID);
        }
    }

    private void OnTrilhaLoaded()
    {
        if (TrilhaManager.Instance != null && TrilhaManager.Instance.currentTrilha != null)
        {
            questoes = TrilhaManager.Instance.currentTrilha.questoes;
            currentID = 0;
            ShowItemByID(currentID);
        }
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
            GerarResultadoComFeedback();
            return;
        }

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
                PreencherAlternativas(videoAlt1, videoAlt2, videoAlt3, item.Alternativas);
                ConfigurarBotoes(videoBtn1, videoBtn2, videoBtn3, videoAlt1, videoAlt2, videoAlt3, item);
                break;

            case "imagemAlternativa":
                imagemAlternativaLayout.SetActive(true);
                imagemPerguntaText.text = item.Questao;
                if (!string.IsNullOrEmpty(item.Midia))
                    StartCoroutine(CarregarImagemDeURL(item.Midia, imagemQuestao));
                else
                    imagemQuestao.sprite = null;
                PreencherAlternativas(imgAlt1, imgAlt2, imgAlt3, item.Alternativas);
                ConfigurarBotoes(imgBtn1, imgBtn2, imgBtn3, imgAlt1, imgAlt2, imgAlt3, item);
                break;

            case "simplesAlternativa":
                simplesAlternativaLayout.SetActive(true);
                simplesPerguntaText.text = item.Questao;
                PreencherAlternativas(simAlt1, simAlt2, simAlt3, item.Alternativas);
                ConfigurarBotoes(simBtn1, simBtn2, simBtn3, simAlt1, simAlt2, simAlt3, item);
                break;

            case "textoAlternativa":
                textoAlternativaLayout.SetActive(true);
                textoPerguntaText.text = item.Questao;
                textoConteudoText.text = item.Texto;
                PreencherAlternativas(textoAlt1, textoAlt2, textoAlt3, item.Alternativas);
                ConfigurarBotoes(textoBtn1, textoBtn2, textoBtn3, textoAlt1, textoAlt2, textoAlt3, item);
                break;

            case "simplesEscrita":
                simplesEscritaLayout.SetActive(true);
                escritaPerguntaText.text = item.Questao;
                escritaAlt1.text = item.Alternativas[0];
                escritaAlt2.text = item.Alternativas[1];
                escritaAlt3.text = item.Alternativas[2];
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
                        string respostaComposta =
                            $"{escritaInput1.text}|{escritaInput2.text}|{escritaInput3.text}";
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
                Debug.LogError($"Erro ao carregar imagem: {request.error}");
            else
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                Sprite sprite = Sprite.Create(texture,
                    new Rect(0, 0, texture.width, texture.height),
                    Vector2.one * 0.5f);
                destino.sprite = sprite;
            }
        }
    }

    private void PreencherAlternativas(TMP_Text alt1, TMP_Text alt2, TMP_Text alt3, List<string> opcoes)
    {
        if (opcoes == null || opcoes.Count < 3) return;
        alt1.text = opcoes[0];
        alt2.text = opcoes[1];
        alt3.text = opcoes[2];
    }

    private void ConfigurarBotoes(Button btn1, Button btn2, Button btn3,
                                  TMP_Text alt1, TMP_Text alt2, TMP_Text alt3,
                                  QuestionData item)
    {
        Button[] botoes = { btn1, btn2, btn3 };
        TMP_Text[] textos = { alt1, alt2, alt3 };

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

    private void AtualizarCoresBotoes(Button[] botoes, TMP_Text[] textos,
                                      QuestionData item, string respostaUsuario)
    {
        string correta = item.RespostaCorreta != null && item.RespostaCorreta.Count > 0
                         ? item.RespostaCorreta[0] : "";

        for (int i = 0; i < botoes.Length; i++)
        {
            botoes[i].interactable = false;
            Image img = botoes[i].GetComponent<Image>();
            if (img == null) continue;

            if (textos[i].text == respostaUsuario)
                img.color = respostaUsuario == correta ? corAlternativaAcerto : corAlternativaErro;
            else if (textos[i].text == correta)
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

            string corretaStr = r.respostaCorreta != null && r.respostaCorreta.Count > 0
                                ? r.respostaCorreta[0] : "";
            bool estaCorreta = r.respostaUsuario == corretaStr;

            btnScript.Configurar(
                r.idQuestao,
                r.respostaUsuario,
                corretaStr,
                string.Join(", ", r.habilidades),
                estaCorreta
            );
        }
    }

    // ================= Função nova: gerar resultado com feedback =================
    private void GerarResultadoComFeedback()
    {
        if (answerScript == null || answerScript.Respostas == null || answerScript.Respostas.Count == 0)
        {
            feedbackLayout.SetActive(true);
            feedbackTexto.text = "Nenhuma resposta registrada.";
            return;
        }

        int totalQuestoes = answerScript.Respostas.Count;
        int totalAcertos = 0;

        foreach (var r in answerScript.Respostas)
        {
            string corretaStr = (r.respostaCorreta != null && r.respostaCorreta.Count > 0) ? r.respostaCorreta[0] : "";
            if (r.respostaUsuario == corretaStr)
                totalAcertos++;
        }

        float porcentagem = (float)totalAcertos / totalQuestoes * 100f;

        feedbackLayout.SetActive(true);
        nextBTN.SetActive(false); // desativa botão de avançar ao final

        if (porcentagem >= 80f)
        {
            feedbackTexto.text = $"Parabéns! Você foi aprovado com {porcentagem:0}% de acertos.";
            if (TrilhaManager.Instance?.currentTrilha != null)
                SalvarTrilhaAprovada(TrilhaManager.Instance.currentTrilha.id);
        }
        else
        {
            feedbackTexto.text = $"Você foi reprovado com {porcentagem:0}% de acertos. Tente novamente!";
        }

        GerarResultadoBotoes();
    }

    // ================= Função nova: salvar trilha aprovada =================
    private void SalvarTrilhaAprovada(string trilhaID)
    {
        if (string.IsNullOrEmpty(trilhaID))
            return;

        var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("[TrilhaUIManager] Usuário não logado.");
            return;
        }

        DocumentReference userDoc = FirebaseFirestore.DefaultInstance
            .Collection("Users").Document(user.UserId);

        userDoc.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || !task.IsCompleted || task.Result == null)
            {
                Debug.LogError("[TrilhaUIManager] Erro ao acessar documento do usuário.");
                return;
            }

            List<object> trilhasAtuais = new List<object>();
            if (task.Result.Exists && task.Result.TryGetValue("trilhas", out object trilhasObj) && trilhasObj is IEnumerable<object> listObj)
                trilhasAtuais = new List<object>(listObj);

            if (!trilhasAtuais.Contains(trilhaID))
                trilhasAtuais.Add(trilhaID);

            userDoc.SetAsync(new Dictionary<string, object> { { "trilhas", trilhasAtuais } }, SetOptions.MergeAll)
                .ContinueWithOnMainThread(setTask =>
                {
                    if (setTask.IsCompleted)
                        Debug.Log($"[TrilhaUIManager] Trilha '{trilhaID}' salva com sucesso.");
                    else
                        Debug.LogError($"[TrilhaUIManager] Erro ao salvar trilha: {setTask.Exception}");
                });
        });
    }

}
