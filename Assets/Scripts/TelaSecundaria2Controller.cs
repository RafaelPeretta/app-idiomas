using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TelaSecundaria2Controller : MonoBehaviour
{
    [Header("TMPs para exibir nomes da Trilha e do Quiz")]
    public TMP_Text trilhaTMP;
    public TMP_Text quizTMP;

    [Header("IDs selecionados")]
    public string trilhaIDSelecionada;
    public string quizIDSelecionado;

    [Header("Botões")]
    public GameObject botaoQuiz;

    private FirebaseFirestore db;
    private List<string> trilhasUsuario = new List<string>();

    private void Awake()
    {
        db = FirebaseFirestore.DefaultInstance;
        CarregarProgressoUsuario();
        gameObject.SetActive(false);
    }

    private void CarregarProgressoUsuario()
    {
        string userId = UserDataManager.userInstance.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return;

        DocumentReference userRef = db.Collection("Users").Document(userId);
        userRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && task.Result.Exists && task.Result.ContainsField("trilhas"))
            {
                var lista = task.Result.GetValue<List<object>>("trilhas");
                trilhasUsuario.Clear();
                foreach (var t in lista)
                    trilhasUsuario.Add(t.ToString());
            }
        });
    }

    public void AbrirComTrilha(string trilhaID)
    {
        trilhaIDSelecionada = trilhaID;
        quizIDSelecionado = "";
        gameObject.SetActive(true);

        if (trilhaTMP != null)
            trilhaTMP.text = "Carregando trilha...";
        if (quizTMP != null)
            quizTMP.text = "Carregando quiz...";

        if (botaoQuiz != null)
            botaoQuiz.GetComponent<UnityEngine.UI.Button>().interactable = false;

        BuscarNomeTrilha();
    }

    private void BuscarNomeTrilha()
    {
        DocumentReference trilhasRef = db.Collection("referencias").Document("Trilhas");
        trilhasRef.GetSnapshotAsync().ContinueWithOnMainThread(trilhaTask =>
        {
            if (trilhaTask.IsFaulted)
            {
                if (trilhaTMP != null) trilhaTMP.text = "Erro ao carregar trilha";
                return;
            }

            DocumentSnapshot trilhaSnap = trilhaTask.Result;

            List<object> listaTrilhas = trilhaSnap.GetValue<List<object>>("Lista");
            string nomeTrilha = trilhaIDSelecionada;

            foreach (var item in listaTrilhas)
            {
                if (item is Dictionary<string, object> dict &&
                    dict["ID"].ToString() == trilhaIDSelecionada)
                {
                    nomeTrilha = dict["Nome"].ToString();
                    break;
                }
            }

            if (trilhaTMP != null)
                trilhaTMP.text = $"Trilha: {nomeTrilha}";

            BuscarQuizCorrespondente(nomeTrilha);
        });
    }

    private void BuscarQuizCorrespondente(string nomeTrilha)
    {
        DocumentReference quizRef = db.Collection("referencias").Document("Quiz");
        quizRef.GetSnapshotAsync().ContinueWithOnMainThread(quizTask =>
        {
            if (quizTask.IsFaulted)
            {
                if (quizTMP != null) quizTMP.text = "Erro ao carregar quiz";
                return;
            }

            DocumentSnapshot quizSnap = quizTask.Result;
            List<object> listaQuiz = quizSnap.GetValue<List<object>>("Lista");

            bool encontrado = false;

            foreach (var item in listaQuiz)
            {
                if (item is Dictionary<string, object> dict &&
                    dict["TrilhaID"].ToString() == trilhaIDSelecionada)
                {
                    quizIDSelecionado = dict["ID"].ToString();
                    string nomeQuiz = nomeTrilha;

                    if (quizTMP != null)
                        quizTMP.text = $"Quiz: {nomeQuiz}";

                    // --- REGRA DE DESBLOQUEIO DO QUIZ ---
                    bool trilhaFeita = trilhasUsuario.Contains(trilhaIDSelecionada);

                    if (botaoQuiz != null)
                        botaoQuiz.GetComponent<UnityEngine.UI.Button>().interactable = trilhaFeita;

                    encontrado = true;
                    break;
                }
            }

            if (!encontrado && quizTMP != null)
                quizTMP.text = "Quiz não encontrado";
        });
    }

    public void loadTrilha()
    {
        TrilhaLoader.Instance.LoadTrilha(trilhaIDSelecionada);
    }

    public void loadQuiz()
    {
        TrilhaLoader.Instance.LoadTrilha(quizIDSelecionado);
    }

    public void FecharTela()
    {
        gameObject.SetActive(false);
    }
}
