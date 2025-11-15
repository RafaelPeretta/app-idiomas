using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;

public class TrilhaHabilidadesLoader : MonoBehaviour
{
    [Header("Referências de UI")]
    public GameObject botaoPrefab;
    public Transform containerBotoes;

    [Header("Referência do contador")]
    public TMP_Text contadorTMP;

    [Header("Tela secundária 2")]
    public TelaSecundaria2Controller telaSecundaria2Controller;

    private FirebaseFirestore db;

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        BuscarTrilhasDoFirestore();
    }

    public void BuscarTrilhasDoFirestore()
    {
        if (botaoPrefab == null || containerBotoes == null)
        {
            Debug.LogError("[TrilhaHabilidadesLoader] Prefab ou container não atribuídos!");
            return;
        }

        DocumentReference trilhasRef = db.Collection("referencias").Document("Trilhas");
        trilhasRef.GetSnapshotAsync().ContinueWithOnMainThread(trilhaTask =>
        {
            if (trilhaTask.IsFaulted)
            {
                Debug.LogError($"Erro ao buscar trilhas: {trilhaTask.Exception}");
                return;
            }

            DocumentSnapshot trilhaSnap = trilhaTask.Result;
            if (!trilhaSnap.Exists)
            {
                Debug.LogWarning("[TrilhaHabilidadesLoader] Documento 'Trilhas' não encontrado.");
                return;
            }

            List<object> listaTrilhas = trilhaSnap.GetValue<List<object>>("Lista");
            if (listaTrilhas == null || listaTrilhas.Count == 0)
            {
                Debug.LogWarning("[TrilhaHabilidadesLoader] Nenhuma trilha encontrada.");
                return;
            }

            List<Dictionary<string, object>> trilhas = new List<Dictionary<string, object>>();
            foreach (var item in listaTrilhas)
            {
                if (item is Dictionary<string, object> dict)
                    trilhas.Add(dict);
            }

            CriarBotoes(trilhas);
        });
    }

    private void CriarBotoes(List<Dictionary<string, object>> trilhas)
    {
        // limpa container
        foreach (Transform filho in containerBotoes)
            Destroy(filho.gameObject);

        // Ordena por número de ID (TRILHA001, TRILHA002...)
        trilhas.Sort((a, b) =>
        {
            int numA = 0, numB = 0;
            if (a.ContainsKey("ID")) int.TryParse(a["ID"].ToString().Substring(6), out numA);
            if (b.ContainsKey("ID")) int.TryParse(b["ID"].ToString().Substring(6), out numB);
            return numA.CompareTo(numB);
        });

        string userId = UserDataManager.userInstance.GetUserId();
        List<string> trilhasUsuario = new List<string>();
        List<string> quizsUsuario = new List<string>();

        if (!string.IsNullOrEmpty(userId))
        {
            DocumentReference userRef = db.Collection("Users").Document(userId);
            userRef.GetSnapshotAsync().ContinueWithOnMainThread(userTask =>
            {
                if (!userTask.IsFaulted && userTask.Result.Exists)
                {
                    // carrega campo "trilhas" se existir
                    if (userTask.Result.ContainsField("trilhas"))
                    {
                        var lista = userTask.Result.GetValue<List<object>>("trilhas");
                        foreach (var t in lista)
                            trilhasUsuario.Add(t.ToString());
                    }

                    // carrega campo "quizs" se existir
                    if (userTask.Result.ContainsField("quizs"))
                    {
                        var listaQuiz = userTask.Result.GetValue<List<object>>("quizs");
                        foreach (var q in listaQuiz)
                            quizsUsuario.Add(q.ToString());
                    }

                    // IMPORTANTE: alguns quizzes podem estar salvos dentro do array "trilhas"
                    // então mesclamos os valores de 'trilhas' dentro de quizsUsuario para garantir detecção
                    foreach (var v in trilhasUsuario)
                    {
                        if (v.StartsWith("QUIZ") && !quizsUsuario.Contains(v))
                            quizsUsuario.Add(v);
                    }
                }

                bool proximaEncontrada = false;
                int concluidas = 0;

                for (int index = 0; index < trilhas.Count; index++)
                {
                    var trilha = trilhas[index];
                    string id = trilha.ContainsKey("ID") ? trilha["ID"].ToString() : $"TRILHA{(index + 1).ToString("D3")}";
                    string nome = trilha.ContainsKey("Nome") ? trilha["Nome"].ToString() : id;

                    GameObject botao = Instantiate(botaoPrefab, containerBotoes);
                    botao.SetActive(true);

                    TMP_Text tmp = botao.GetComponentInChildren<TMP_Text>(true);
                    if (tmp != null) tmp.text = nome;

                    Transform checkTransform = botao.transform.Find("Check");
                    Image checkImage = checkTransform != null ? checkTransform.GetComponent<Image>() : null;

                    // concluiu trilha (trilhaId presente)
                    bool concluiuTrilha = trilhasUsuario.Contains(id);

                    // concluiu quiz correspondente? checamos QUIZn onde n = index+1 (formatado D3)
                    string quizIdAtual = $"QUIZ{(index + 1).ToString("D3")}";
                    bool concluiuQuiz = quizsUsuario.Contains(quizIdAtual) || trilhasUsuario.Contains(quizIdAtual);

                    // === SISTEMA DE DESBLOQUEIO ===
                    bool desbloqueado = false;

                    if (index == 0)
                    {
                        // PRIMEIRA TRILHA SEMPRE LIBERADA
                        desbloqueado = true;
                    }
                    else
                    {
                        int numeroAtual = index + 1;
                        int anterior = numeroAtual - 1;

                        string trilhaAnterior = $"TRILHA{anterior.ToString("D3")}";
                        string quizAnterior = $"QUIZ{anterior.ToString("D3")}";

                        bool temTrilhaAnterior = trilhasUsuario.Contains(trilhaAnterior);
                        bool temQuizAnterior = quizsUsuario.Contains(quizAnterior) || trilhasUsuario.Contains(quizAnterior);

                        // precisa de ambos para liberar
                        desbloqueado = temTrilhaAnterior && temQuizAnterior;
                    }

                    // === APLICAR ESTADOS VISUAIS ===
                    if (checkImage != null)
                    {
                        // verde apenas se trilha N e quiz N estiverem presentes
                        if (concluiuTrilha && concluiuQuiz)
                        {
                            checkImage.color = Color.green;
                            concluidas++;
                        }
                        else if (!desbloqueado)
                        {
                            // bloqueado (não pode acessar)
                            checkImage.color = Color.grey;
                        }
                        else if (!proximaEncontrada)
                        {
                            // próxima disponível (desbloqueado, não concluído)
                            checkImage.color = new Color(1f, 0.65f, 0f); // laranja
                            proximaEncontrada = true;
                        }
                        else
                        {
                            // desbloqueado, mas não a próxima
                            checkImage.color = Color.red;
                        }
                    }

                    // === INTERATIVIDADE ===
                    Button btn = botao.GetComponent<Button>();
                    if (btn != null)
                    {
                        btn.interactable = desbloqueado;

                        if (desbloqueado && telaSecundaria2Controller != null)
                        {
                            // captura id local para o listener (evita closure errada)
                            string capturedId = id;
                            btn.onClick.AddListener(() =>
                            {
                                telaSecundaria2Controller.AbrirComTrilha(capturedId);
                            });
                        }
                    }
                }

                if (contadorTMP != null)
                    contadorTMP.text = $"{concluidas} / {trilhas.Count}";

                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(containerBotoes as RectTransform);
            });
        }
    }
}
