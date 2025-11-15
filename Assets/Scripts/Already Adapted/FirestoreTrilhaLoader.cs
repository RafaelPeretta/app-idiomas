using UnityEngine;
using System;
using System.Collections.Generic;
using Firebase;
using Firebase.Firestore;
using System.Linq;
using System.Threading.Tasks;

public class TrilhaLoader : MonoBehaviour
{
    public static TrilhaLoader Instance { get; private set; }
    private FirebaseFirestore db;

    public event Action OnTrilhaLoaded;

    private async void Awake()
    {
        // Se já existe uma instância do TrilhaLoader, destrua este objeto.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Inicializando o Firebase e aguardando sua inicialização
        await InitializeFirebase();

        // Se o Firebase não foi inicializado corretamente, não continue com o carregamento.
        if (db == null)
        {
            Debug.LogError("[TrilhaLoader] FirebaseFirestore não inicializado corretamente.");
            return;
        }

        // Agora, o Firebase está pronto, e podemos continuar o carregamento das trilhas.
        Debug.Log("[TrilhaLoader] Firebase inicializado com sucesso.");
    }

    private async Task InitializeFirebase()
    {
        // Aguardar a conclusão da inicialização do Firebase
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (dependencyStatus != DependencyStatus.Available)
        {
            Debug.LogError($"[TrilhaLoader] Firebase não foi inicializado corretamente. Erro: {dependencyStatus}");
            return;
        }

        // Agora o Firebase foi inicializado com sucesso.
        FirebaseApp app = FirebaseApp.DefaultInstance;
        db = FirebaseFirestore.GetInstance(app);
        Debug.Log("[TrilhaLoader] Firestore inicializado com sucesso.");
    }

    public void LoadTrilha(string id)
    {
        Debug.LogWarning("ID RECEBIDO: " + id);

        if (db == null)
        {
            Debug.LogError("[TrilhaLoader] Firestore não inicializado.");
            return;
        }

        // Detecta se é um quiz (ex: QUIZ001)
        string trilhaID = id;
        bool isQuiz = false;

        if (id.StartsWith("QUIZ"))
        {
            isQuiz = true;
            Debug.Log($"[TrilhaLoader] Quiz detectado. ID: {id}. Substituindo por ID de trilha.");
            // Substitui QUIZ001 por TRILHA001
            trilhaID = "TRILHA" + id.Substring(4);
        }

        Debug.Log($"[TrilhaLoader] Carregando trilha com ID: {trilhaID} (original: {id}).");

        LoadTrilhaFromFirestore(trilhaID, id, isQuiz);
    }

    private async void LoadTrilhaFromFirestore(string trilhaID, string originalID, bool isQuiz)
    {
        try
        {
            // Acessando diretamente o Firestore
            DocumentReference docRef = db.Collection("trilhas").Document(trilhaID);
            Debug.Log($"[TrilhaLoader] Acessando documento Firestore para a trilha com ID: {trilhaID}");

            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                Debug.LogWarning($"[TrilhaLoader] Documento '{trilhaID}' não encontrado.");
                return;
            }

            if (!snapshot.TryGetValue("Questoes", out object questoesObj) || questoesObj == null)
            {
                Debug.LogWarning("[TrilhaLoader] Campo 'Questoes' não encontrado.");
                return;
            }

            List<object> questoesList = questoesObj as List<object>;
            if (questoesList == null || questoesList.Count == 0)
            {
                Debug.LogWarning("[TrilhaLoader] Questões vazias.");
                return;
            }

            Debug.Log($"[TrilhaLoader] {questoesList.Count} questões encontradas.");

            List<QuestionData> loadedQuestions = new List<QuestionData>();

            foreach (var q in questoesList)
            {
                if (q is Dictionary<string, object> questaoDict)
                {
                    QuestionData question = new QuestionData
                    {
                        Tipo = questaoDict.TryGetValue("Tipo", out var tipo) ? tipo?.ToString() : "",
                        Midia = questaoDict.TryGetValue("Midia", out var midia) ? midia?.ToString() : "",
                        Texto = questaoDict.TryGetValue("Texto", out var texto) ? texto?.ToString() : "",
                        Questao = questaoDict.TryGetValue("Questao", out var questao) ? questao?.ToString() : "",
                        Objetivo = questaoDict.TryGetValue("Objetivo", out var objetivo) ? objetivo?.ToString() : "",
                        Explicacoes = questaoDict.TryGetValue("Explicacoes", out var exp) ? exp?.ToString() : "",
                        Habilidades = questaoDict.TryGetValue("Habilidades", out var hab) ? ConvertToStringList(hab) : new List<string>(),
                        Alternativas = questaoDict.TryGetValue("Alternativas", out var alt) ? ConvertToStringList(alt) : new List<string>(),
                        RespostaCorreta = questaoDict.TryGetValue("RespostaCorreta", out var resp) ? ConvertToStringList(resp) : new List<string>()
                    };

                    loadedQuestions.Add(question);
                }
            }

            // Se for quiz, seleciona aleatoriamente 5 questões
            if (isQuiz)
            {
                if (loadedQuestions.Count > 5)
                {
                    Debug.Log("[TrilhaLoader] Quiz detectado, selecionando aleatoriamente 5 questões.");
                    // Seleciona aleatoriamente 5 questões
                    loadedQuestions = loadedQuestions.OrderBy(x => UnityEngine.Random.value).Take(5).ToList();
                    Debug.Log("[TrilhaLoader] 5 questões selecionadas para o quiz.");
                }
                else
                {
                    Debug.Log("[TrilhaLoader] Questões insuficientes para quiz, todas as questões serão carregadas.");
                }
            }

            // Exibindo o número de questões carregadas
            Debug.Log($"[TrilhaLoader] Carregamento finalizado com {loadedQuestions.Count} questões para o {(isQuiz ? "quiz" : "trilha")}.");

            TrilhaDataLoad loadedTrilha = new TrilhaDataLoad
            {
                id = originalID, // Mantém ID do quiz se for quiz
                questoes = loadedQuestions
            };

            if (TrilhaManager.Instance != null)
            {
                TrilhaManager.Instance.currentTrilha = loadedTrilha;
                Debug.Log($"[TrilhaLoader] {(isQuiz ? "Quiz" : "Trilha")} '{originalID}' carregado com sucesso.");
                OnTrilhaLoaded?.Invoke();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[TrilhaLoader] Erro ao carregar trilha '{trilhaID}': {e}");
        }
    }

    private List<string> ConvertToStringList(object firestoreList)
    {
        List<string> result = new List<string>();
        if (firestoreList is IEnumerable<object> list)
        {
            foreach (var item in list)
                if (item != null) result.Add(item.ToString());
        }
        else if (firestoreList != null)
        {
            result.Add(firestoreList.ToString());
        }
        return result;
    }
}
