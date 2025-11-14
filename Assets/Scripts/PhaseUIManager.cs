using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Video;
using UnityEngine.Networking;
using Firebase.Firestore;
using System.Threading.Tasks;

[System.Serializable]
public class TrilhaLoad
{
    public string id;
    public List<string> habilidades;
}

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

    [Header("Gráfico")]
    public BarGraphGenerator barGraphGenerator;

    [Header("Trilhas")]
    public List<TrilhaLoad> todasTrilhas;

    public int currentID = 0;
    private List<QuestionData> questoes;

    private void Awake()
    {
        if (FirestorePhaseLoader.Instance != null)
            FirestorePhaseLoader.Instance.OnPhaseLoaded += OnPhaseLoaded;
    }

    private async void Start()
    {
        answerScript = GetComponent<answerQuestion>();

        if (answerScript != null)
            answerScript.onRespostaRegistrada += OnRespostaRegistrada;

        // Carrega as trilhas do Firebase
        await CarregarTrilhasDoFirebase();

        // Exibe todos os IDs de trilhas carregadas
        MostrarTodosIDsTrilhas();

        if (PhaseManager.Instance != null && PhaseManager.Instance.currentPhase != null)
        {
            questoes = PhaseManager.Instance.currentPhase.diagnostica_6ano;
            ShowItemByID(currentID);
        }
    }

    private async Task CarregarTrilhasDoFirebase()
    {
        todasTrilhas = new List<TrilhaLoad>();
        var db = FirebaseFirestore.DefaultInstance;
        var trilhasDocRef = db.Collection("referencias").Document("Trilhas");

        try
        {
            var snapshot = await trilhasDocRef.GetSnapshotAsync();
            if (snapshot.Exists && snapshot.TryGetValue("Lista", out List<object> listaTrilhas))
            {
                foreach (var item in listaTrilhas)
                {
                    // Cada item deve ser um Dictionary<string, object>
                    var dict = item as Dictionary<string, object>;
                    if (dict != null)
                    {
                        TrilhaLoad trilha = new TrilhaLoad();
                        if (dict.TryGetValue("ID", out object idObj))
                            trilha.id = idObj.ToString();
                        else
                            continue;

                        if (dict.TryGetValue("Habilidades", out object habObj))
                        {
                            var habList = habObj as IEnumerable<object>;
                            trilha.habilidades = habList != null ? habList.Select(o => o.ToString()).ToList() : new List<string>();
                        }
                        else
                        {
                            trilha.habilidades = new List<string>();
                        }

                        todasTrilhas.Add(trilha);
                    }
                }
            }

            Debug.Log($"Carregadas {todasTrilhas.Count} trilhas do Firebase.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao carregar trilhas do Firebase: {e.Message}");
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

            var porcentagens = CalcularPorcentagemPorHabilidade();

            foreach (var p in porcentagens)
                Debug.Log($"Habilidade {p.Key}: {p.Value}%");

            if (barGraphGenerator != null)
                barGraphGenerator.GerarGrafico(porcentagens);

            // Identifica habilidade mais fraca (abaixo de 80%)
            string habilidadeMaisFraca = ObterHabilidadeMaisFraca();
            Debug.Log($"Habilidade mais fraca do aluno (abaixo de 80%): {habilidadeMaisFraca}");

            List<string> trilhasParaSalvar = new List<string>();

            if (!string.IsNullOrEmpty(habilidadeMaisFraca))
            {
                string trilhaId = ObterTrilhaParaHabilidade(habilidadeMaisFraca);
                if (!string.IsNullOrEmpty(trilhaId))
                {
                    int trilhaAtualNum = ExtrairNumeroTrilha(trilhaId);

                    // Pega apenas todas as trilhas com número menor que a trilha da habilidade fraca
                    trilhasParaSalvar = todasTrilhas
                        .Where(t => ExtrairNumeroTrilha(t.id) < trilhaAtualNum)
                        .Select(t => t.id)
                        .ToList();
                }
            }
            else
            {
                // Nenhuma habilidade fraca -> salva todas as trilhas
                trilhasParaSalvar = todasTrilhas.Select(t => t.id).ToList();
            }


            if (trilhasParaSalvar.Count > 0)
            {
                Debug.Log($"Trilhas que serão adicionadas ao documento do usuário: {string.Join(", ", trilhasParaSalvar)}");
                string userId = UserDataManager.userInstance.GetUserId();
                AtualizarTrilhasUsuario(userId, trilhasParaSalvar);
            }

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
            {
                Debug.LogError($"[PhaseUIManager] Erro ao carregar imagem: {request.error}");
            }
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
        var lista = item.RespostaCorreta;
        string correta = lista != null && lista.Count > 0 ? lista[0] : "";

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

            var lista = r.respostaCorreta;
            string corretaStr = lista != null && lista.Count > 0 ? lista[0] : "";
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

    public Dictionary<string, float> CalcularPorcentagemPorHabilidade()
    {
        if (answerScript == null || answerScript.Respostas == null)
            return new Dictionary<string, float>();

        Dictionary<string, int> totalPorHabilidade = new Dictionary<string, int>();
        Dictionary<string, int> acertosPorHabilidade = new Dictionary<string, int>();

        foreach (var r in answerScript.Respostas)
        {
            string correta = r.respostaCorreta != null && r.respostaCorreta.Count > 0
                             ? r.respostaCorreta[0]
                             : "";

            bool acertou = r.respostaUsuario == correta;

            foreach (string hab in r.habilidades)
            {
                if (!totalPorHabilidade.ContainsKey(hab))
                    totalPorHabilidade[hab] = 0;
                if (!acertosPorHabilidade.ContainsKey(hab))
                    acertosPorHabilidade[hab] = 0;

                totalPorHabilidade[hab]++;
                if (acertou)
                    acertosPorHabilidade[hab]++;
            }
        }

        Dictionary<string, float> porcentagens = new Dictionary<string, float>();
        foreach (var kvp in totalPorHabilidade)
        {
            string habilidade = kvp.Key;
            int total = kvp.Value;
            int acertos = acertosPorHabilidade[habilidade];

            porcentagens[habilidade] = (float)acertos / total * 100f;
        }

        return porcentagens;
    }

    public string ObterHabilidadeMaisFraca()
    {
        var porcentagens = CalcularPorcentagemPorHabilidade();
        if (porcentagens == null || porcentagens.Count == 0)
            return null;

        var abaixo80 = porcentagens.Where(kvp => kvp.Value < 80f).ToList();
        if (abaixo80.Count == 0) return null;

        float menorValor = abaixo80.Min(kvp => kvp.Value);
        var maisFracas = abaixo80.Where(kvp => kvp.Value == menorValor)
                                 .Select(kvp => kvp.Key)
                                 .ToList();

        if (maisFracas.Contains("EF06LI08"))
            return "EF06LI08";

        return maisFracas[0];
    }

    public string ObterTrilhaParaHabilidade(string habilidadeMaisFraca)
    {
        if (string.IsNullOrEmpty(habilidadeMaisFraca) || todasTrilhas == null || todasTrilhas.Count == 0)
            return null;

        var trilhasOrdenadas = todasTrilhas
            .OrderBy(t => ExtrairNumeroTrilha(t.id))
            .ToList();

        foreach (var trilha in trilhasOrdenadas)
        {
            if (trilha.habilidades.Contains(habilidadeMaisFraca))
                return trilha.id;
        }

        return null;
    }

    private int ExtrairNumeroTrilha(string trilhaId)
    {
        if (string.IsNullOrEmpty(trilhaId)) return 0;
        string numeroStr = new string(trilhaId.Reverse().TakeWhile(char.IsDigit).Reverse().ToArray());
        int numero;
        int.TryParse(numeroStr, out numero);
        return numero;
    }

    private async void AtualizarTrilhasUsuario(string userId, List<string> trilhasParaSalvar)
    {
        if (string.IsNullOrEmpty(userId) || trilhasParaSalvar == null || trilhasParaSalvar.Count == 0) return;

        var db = FirebaseFirestore.DefaultInstance;
        var usuarioRef = db.Collection("Users").Document(userId);

        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { "trilhas", trilhasParaSalvar }
        };

        await usuarioRef.SetAsync(updates, SetOptions.MergeAll);
        Debug.Log($"Documento do usuário {userId} atualizado com sucesso.");
    }

    private void MostrarTodosIDsTrilhas()
    {
        if (todasTrilhas == null || todasTrilhas.Count == 0)
        {
            Debug.Log("Nenhuma trilha carregada.");
            return;
        }

        string ids = string.Join(", ", todasTrilhas.Select(t => t.id));
        Debug.Log("IDs de todas as trilhas carregadas: " + ids);
    }

}
