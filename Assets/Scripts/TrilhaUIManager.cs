using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;
using Firebase;
using System.Threading.Tasks;
using Firebase.Auth;


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

    // Layouts
    [Header("Layouts")]
    public GameObject videoAlternativaLayout;
    public GameObject imagemAlternativaLayout;
    public GameObject simplesAlternativaLayout;
    public GameObject textoAlternativaLayout;

    // Variáveis de interface
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

    // Trilhas
    public List<TrilhaLoad_> todasTrilhas;
    public int currentID = 0;
    private List<QuestionData> questoes;

    private bool firebaseInicializado = false;

    private void Awake()
    {
        // Obtém a referência ao componente answerTrilha que está no mesmo GameObject
        answerScript = GetComponent<answerTrilha>();

        if (answerScript == null)
        {
            Debug.LogError("[TrilhaUIManager] O componente answerTrilha não foi encontrado no mesmo GameObject!");
        }

        // Inicia o Firebase
        StartCoroutine(InitializeFirebaseCoroutine());
    }

    private IEnumerator InitializeFirebaseCoroutine()
    {
        // Chama a tarefa para inicializar o Firebase
        Task firebaseInitTask = InitializeFirebase();

        // Aguardar a conclusão da tarefa
        yield return new WaitUntil(() => firebaseInitTask.IsCompleted);

        // Verifica se a inicialização do Firebase foi bem-sucedida
        if (firebaseInitTask.Exception != null)
        {
            Debug.LogError("[TrilhaUIManager] Erro ao inicializar o Firebase.");
        }
        else
        {
            Debug.Log("[TrilhaUIManager] Firebase inicializado com sucesso.");
            firebaseInicializado = true;

            // Agora que o Firebase foi inicializado, podemos carregar a trilha
            LoadTrilha();
        }
    }

    private async Task InitializeFirebase()
    {
        // Verificar as dependências do Firebase
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (dependencyStatus == DependencyStatus.Available)
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            FirebaseFirestore db = FirebaseFirestore.GetInstance(app);
            Debug.Log("[TrilhaUIManager] FirebaseFirestore inicializado com sucesso.");
            firebaseInicializado = true;
        }
        else
        {
            Debug.LogError($"[TrilhaUIManager] Erro ao inicializar Firebase: {dependencyStatus}");
        }
    }

    private async void SalvarStatusTrilha(string trilhaId)
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;

        if (user == null)
        {
            Debug.LogError("[TrilhaUIManager] Usuário não autenticado.");
            return;
        }

        string userId = user.UserId;

        DocumentReference userDocRef =
            FirebaseFirestore.DefaultInstance
            .Collection("Users")
            .Document(userId);

        // Agora salva SOMENTE o ID no array "trilhasAprovadas"
        Dictionary<string, object> updateData = new Dictionary<string, object>
    {
        { "trilhas", FieldValue.ArrayUnion(trilhaId) }
    };

        try
        {
            await userDocRef.SetAsync(updateData, SetOptions.MergeAll);
            Debug.Log("[TrilhaUIManager] ID da trilha salvo com sucesso.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TrilhaUIManager] Erro ao salvar trilha: {e.Message}");
        }
    }


    private void LoadTrilha()
    {
        if (!firebaseInicializado)
        {
            Debug.LogError("[TrilhaUIManager] Firebase não foi inicializado corretamente.");
            return;
        }

        if (TrilhaManager.Instance != null && TrilhaManager.Instance.currentTrilha != null)
        {
            questoes = TrilhaManager.Instance.currentTrilha.questoes;
            ShowItemByID(currentID);
        }
    }

    private void ResetLayouts()
    {
        videoAlternativaLayout.SetActive(false);
        imagemAlternativaLayout.SetActive(false);
        simplesAlternativaLayout.SetActive(false);
        textoAlternativaLayout.SetActive(false);
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

        if (item == null)
        {
            Debug.LogError("[TrilhaUIManager] Questão não encontrada.");
            return;
        }

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
            if (botoes[i] == null || textos[i] == null)
            {
                Debug.LogError($"[ConfigurarBotoes] Botão ou texto nulo na posição {i}. Atribua corretamente no Inspector.");
                continue;
            }

            ConfigurarBotao(botoes[i], textos[i], i, item);
        }
    }

    private void ConfigurarBotao(Button botao, TMP_Text texto, int index, QuestionData item)
    {
        botao.interactable = true; // Habilita o botão

        Image img = botao.GetComponent<Image>();
        if (img != null)
            img.color = corAlternativaNormal; // Define a cor inicial para o botão

        botao.onClick.RemoveAllListeners();
        botao.onClick.AddListener(() =>
        {
            // Registrar a resposta do usuário
            answerScript.RegistrarResposta(texto.text);

            // Atualizar a cor dos botões
            AtualizarCoresBotoes(new Button[] { botao }, new TMP_Text[] { texto }, item, texto.text);

            // Desabilitar os botões após a escolha
            foreach (Button b in new Button[] { botao, videoBtn1, videoBtn2, videoBtn3, imgBtn1, imgBtn2, imgBtn3, simBtn1, simBtn2, simBtn3, textoBtn1, textoBtn2, textoBtn3 })
            {
                b.interactable = false;
            }

            // Exibir o botão "Next"
            nextBTN.SetActive(true);
            questaoRespondida = true; // Marcar que a questão foi respondida
        });
    }

    private void AtualizarCoresBotoes(Button[] botoes, TMP_Text[] textos, QuestionData item, string respostaUsuario)
    {
        string correta = (item.RespostaCorreta != null && item.RespostaCorreta.Count > 0)
            ? item.RespostaCorreta[0]
            : "";

        for (int i = 0; i < botoes.Length; i++)
        {
            botoes[i].interactable = false;
            Image img = botoes[i].GetComponent<Image>();

            if (img == null) continue;

            string textoBotao = textos[i].text;

            // Botão clicado → certo ou errado
            if (textoBotao == respostaUsuario)
            {
                img.color = (respostaUsuario == correta)
                    ? corAlternativaAcerto        // verde
                    : corAlternativaErro;         // vermelho
            }
            // Botão correto → sempre verde
            else if (textoBotao == correta)
            {
                img.color = corAlternativaAcerto;
            }
        }

        // Marca que a questão foi respondida
        questaoRespondida = true;
        nextBTN.SetActive(true);
    }



    public void NextQuestion()
    {
        if (!questaoRespondida)
        {
            Debug.LogError("A questão não foi respondida corretamente.");
            return;
        }

        currentID++;
        ShowItemByID(currentID);
    }

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
        nextBTN.SetActive(false); // Desativa o botão de avançar ao final

        if (porcentagem >= 80f)
        {
            feedbackTexto.text = $"Parabéns! Você foi aprovado com {porcentagem:0}% de acertos.";

            // Salva apenas o ID da trilha aprovada
            if (TrilhaManager.Instance != null && TrilhaManager.Instance.currentTrilha != null)
            {
                string trilhaId = TrilhaManager.Instance.currentTrilha.id;
                SalvarStatusTrilha(trilhaId);
            }
        }
        else
        {
            feedbackTexto.text = $"Você foi reprovado com {porcentagem:0}% de acertos. Tente novamente!";
        }


    }
}
