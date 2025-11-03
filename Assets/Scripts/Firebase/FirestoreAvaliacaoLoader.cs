using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;

public class FirestoreAvaliacaoLoader : MonoBehaviour
{
    public static FirestoreAvaliacaoLoader Instance;
    private FirebaseFirestore db;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[FirestoreAvaliacaoLoader] Instância criada e persistente.");
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("[FirestoreAvaliacaoLoader] Instância duplicada destruída.");
        }
    }

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    /// <summary>
    /// Chamado ao clicar no botão e passando o ID da avaliação.
    /// </summary>
    public void GetTestByButton(string avaliacaoID)
    {
        Debug.Log($"[FirestoreAvaliacaoLoader] Botão clicado. Carregando avaliação: {avaliacaoID}");
        LoadPhaseByID(avaliacaoID, OnPhaseLoaded);
    }

    /// <summary>
    /// Carrega um documento de avaliação do Firestore e converte em PhaseDataLoad.
    /// </summary>
    public async void LoadPhaseByID(string avaliacaoID, Action<PhaseDataLoad> onComplete)
    {
        try
        {
            DocumentReference docRef = db.Collection("avaliacoes").Document(avaliacaoID);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                Debug.LogWarning($"[FirestoreAvaliacaoLoader] Documento '{avaliacaoID}' não encontrado.");
                onComplete?.Invoke(null);
                return;
            }

            if (!snapshot.ContainsField("questoes"))
            {
                Debug.LogWarning($"[FirestoreAvaliacaoLoader] Documento '{avaliacaoID}' não contém o campo 'questoes'.");
                onComplete?.Invoke(null);
                return;
            }

            // Extrai a lista de questões
            var questoesList = snapshot.GetValue<List<object>>("questoes");

            if (questoesList == null || questoesList.Count == 0)
            {
                Debug.LogWarning($"[FirestoreAvaliacaoLoader] O campo 'questoes' está vazio no documento {avaliacaoID}.");
                onComplete?.Invoke(null);
                return;
            }

            // Converte a lista bruta em uma lista tipada de QuestionData
            List<QuestionData> parsedQuestions = new List<QuestionData>();

            foreach (var q in questoesList)
            {
                if (q is Dictionary<string, object> questaoDict)
                {
                    QuestionData question = new QuestionData();

                    question.Tipo = questaoDict.ContainsKey("Tipo") ? questaoDict["Tipo"].ToString() : "";
                    question.Midia = questaoDict.ContainsKey("Midia") ? questaoDict["Midia"].ToString() : "";
                    question.Texto = questaoDict.ContainsKey("Texto") ? questaoDict["Texto"].ToString() : "";
                    question.Questao = questaoDict.ContainsKey("Questao") ? questaoDict["Questao"]?.ToString() : "";

                    if (questaoDict.ContainsKey("Alternativas"))
                        question.Alternativas = ConvertToStringList(questaoDict["Alternativas"]);

                    if (questaoDict.ContainsKey("Explicacoes"))
                        question.Explicacoes = ConvertToStringList(questaoDict["Explicacoes"]);

                    question.Habilidades = questaoDict.ContainsKey("Habilidades") ? questaoDict["Habilidades"].ToString() : "";

                    if (questaoDict.ContainsKey("RespostaCorreta"))
                        question.RespostaCorreta = questaoDict["RespostaCorreta"];

                    parsedQuestions.Add(question);
                }
            }

            // Monta o objeto PhaseDataLoad
            PhaseDataLoad loadedPhase = new PhaseDataLoad
            {
                diagnostica_6ano = parsedQuestions
            };

            // Salva no PhaseManager
            if (PhaseManager.Instance != null)
            {
                PhaseManager.Instance.currentPhase = loadedPhase;
                Debug.Log($"[FirestoreAvaliacaoLoader] Avaliação armazenada no PhaseManager ({parsedQuestions.Count} questões).");
            }
            else
            {
                Debug.LogWarning("[FirestoreAvaliacaoLoader] PhaseManager.Instance não encontrado!");
            }

            onComplete?.Invoke(loadedPhase);
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirestoreAvaliacaoLoader] Erro ao carregar documento '{avaliacaoID}': {e.Message}");
            onComplete?.Invoke(null);
        }
    }

    /// <summary>
    /// Converte listas genéricas do Firestore em listas de strings.
    /// </summary>
    private List<string> ConvertToStringList(object firestoreList)
    {
        List<string> result = new List<string>();
        if (firestoreList is IEnumerable<object> list)
        {
            foreach (var item in list)
            {
                if (item != null)
                    result.Add(item.ToString());
            }
        }
        return result;
    }

    /// <summary>
    /// Callback de sucesso.
    /// </summary>
    private void OnPhaseLoaded(PhaseDataLoad loadedPhase)
    {
        if (loadedPhase == null)
        {
            Debug.LogError("[FirestoreAvaliacaoLoader] Falha ao carregar a avaliação (objeto nulo).");
            return;
        }

        Debug.Log($"[FirestoreAvaliacaoLoader] Avaliação carregada com sucesso! Total de questões: {loadedPhase.diagnostica_6ano.Count}");
    }
}
