using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TelaSecundaria2Controller : MonoBehaviour
{
    [Header("TMPs para exibir nomes da Trilha e do Quiz")]
    public TMP_Text trilhaTMP; // TMP para exibir o nome da trilha
    public TMP_Text quizTMP;   // TMP para exibir o nome do quiz

    [Header("IDs selecionados")]
    public string trilhaIDSelecionada;
    public string quizIDSelecionado;

    private FirebaseFirestore db;

    private void Awake()
    {
        db = FirebaseFirestore.DefaultInstance;
        gameObject.SetActive(false); // garante que a tela comece desativada
    }

    public void AbrirComTrilha(string trilhaID)
    {
        trilhaIDSelecionada = trilhaID;
        quizIDSelecionado = ""; // limpa quiz anterior
        gameObject.SetActive(true);

        if (trilhaTMP != null)
            trilhaTMP.text = "Carregando trilha...";
        if (quizTMP != null)
            quizTMP.text = "Carregando quiz...";

        BuscarNomeTrilha();
    }

    private void BuscarNomeTrilha()
    {
        DocumentReference trilhasRef = db.Collection("referencias").Document("Trilhas");
        trilhasRef.GetSnapshotAsync().ContinueWithOnMainThread(trilhaTask =>
        {
            if (trilhaTask.IsFaulted)
            {
                Debug.LogError($"Erro ao buscar trilhas: {trilhaTask.Exception}");
                if (trilhaTMP != null) trilhaTMP.text = "Erro ao carregar trilha";
                return;
            }

            DocumentSnapshot trilhaSnap = trilhaTask.Result;
            if (!trilhaSnap.Exists)
            {
                Debug.LogWarning("Documento 'Trilhas' não encontrado.");
                if (trilhaTMP != null) trilhaTMP.text = "Trilha não encontrada";
                return;
            }

            List<object> listaTrilhas = trilhaSnap.GetValue<List<object>>("Lista");
            string nomeTrilha = trilhaIDSelecionada; // fallback para ID caso não encontre

            if (listaTrilhas != null)
            {
                foreach (var item in listaTrilhas)
                {
                    if (item is Dictionary<string, object> dict &&
                        dict.ContainsKey("ID") && dict["ID"].ToString() == trilhaIDSelecionada)
                    {
                        nomeTrilha = dict.ContainsKey("Nome") ? dict["Nome"].ToString() : trilhaIDSelecionada;
                        break;
                    }
                }
            }

            if (trilhaTMP != null)
                trilhaTMP.text = $"Trilha: {nomeTrilha}";

            // Depois que temos o nome da trilha, buscamos o quiz correspondente
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
                Debug.LogError($"Erro ao buscar quiz: {quizTask.Exception}");
                if (quizTMP != null) quizTMP.text = "Erro ao carregar quiz";
                return;
            }

            DocumentSnapshot quizSnap = quizTask.Result;
            if (!quizSnap.Exists)
            {
                Debug.LogWarning("Documento 'Quiz' não encontrado.");
                if (quizTMP != null) quizTMP.text = "Quiz não encontrado";
                return;
            }

            List<object> listaQuiz = quizSnap.GetValue<List<object>>("Lista");
            if (listaQuiz == null || listaQuiz.Count == 0)
            {
                if (quizTMP != null) quizTMP.text = "Quiz não encontrado";
                return;
            }

            bool encontrado = false;

            foreach (var item in listaQuiz)
            {
                if (item is Dictionary<string, object> dict &&
                    dict.ContainsKey("TrilhaID") &&
                    dict["TrilhaID"].ToString() == trilhaIDSelecionada)
                {
                    quizIDSelecionado = dict.ContainsKey("ID") ? dict["ID"].ToString() : "(null)";
                    string nomeQuiz = $"{nomeTrilha}";

                    if (quizTMP != null)
                        quizTMP.text = $"Quiz: {nomeQuiz}";

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

    public void FecharTela()
    {
        gameObject.SetActive(false);
    }

}
