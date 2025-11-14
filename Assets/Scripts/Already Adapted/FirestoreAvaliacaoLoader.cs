using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;

public class FirestorePhaseLoader : MonoBehaviour
{
    public static FirestorePhaseLoader Instance { get; private set; }

    private FirebaseFirestore db;

    public event Action OnPhaseLoaded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (UserDataManager.userInstance != null)
        {
            if (UserDataManager.userInstance.DatabaseFirestore != null)
            {
                db = UserDataManager.userInstance.DatabaseFirestore;
            }
            else
            {
                StartCoroutine(WaitForFirestore());
            }
        }
        else
        {
            Debug.LogError("[FirestorePhaseLoader] UserDataManager não encontrado no GameObject.");
        }
    }

    private System.Collections.IEnumerator WaitForFirestore()
    {
        while (UserDataManager.userInstance.DatabaseFirestore == null)
            yield return null;

        db = UserDataManager.userInstance.DatabaseFirestore;
        Debug.Log("[FirestorePhaseLoader] Firestore agora está pronto!");
    }

    public void LoadPhase(string documentID)
    {
        if (db == null)
        {
            Debug.LogError("[FirestorePhaseLoader] Firestore ainda não inicializado. Não é possível carregar a fase.");
            return;
        }

        LoadPhaseFromFirestore(documentID);
    }

    private async void LoadPhaseFromFirestore(string documentID)
    {
        try
        {
            DocumentReference docRef = db.Collection("avaliacoes").Document(documentID);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                Debug.LogWarning($"[FirestorePhaseLoader] Documento '{documentID}' não encontrado.");
                return;
            }

            if (!snapshot.TryGetValue("questoes", out object questoesObj) || questoesObj == null)
            {
                Debug.LogWarning("[FirestorePhaseLoader] Campo 'questoes' não encontrado.");
                return;
            }

            List<object> questoesList = questoesObj as List<object>;
            if (questoesList == null || questoesList.Count == 0)
            {
                Debug.LogWarning("[FirestorePhaseLoader] Questões vazias.");
                return;
            }

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

                        // Agora Explicações é STRING
                        Explicacoes = questaoDict.TryGetValue("Explicacoes", out var exp) ? exp?.ToString() : "",

                        // Agora Habilidades é LISTA
                        Habilidades = questaoDict.TryGetValue("Habilidades", out var hab)
                            ? ConvertToStringList(hab)
                            : new List<string>(),

                        Alternativas = questaoDict.TryGetValue("Alternativas", out var alt)
                            ? ConvertToStringList(alt)
                            : new List<string>(),

                        RespostaCorreta = questaoDict.TryGetValue("RespostaCorreta", out var resp)
                            ? ConvertToStringList(resp)
                            : new List<string>()
                    };

                    loadedQuestions.Add(question);
                }
            }

            PhaseDataLoad loadedPhase = new PhaseDataLoad
            {
                diagnostica_6ano = loadedQuestions
            };

            if (PhaseManager.Instance != null)
            {
                PhaseManager.Instance.currentPhase = loadedPhase;
                Debug.Log($"[FirestorePhaseLoader] Fase carregada ({loadedQuestions.Count} questões).");
                OnPhaseLoaded?.Invoke();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirestorePhaseLoader] Erro ao carregar fase '{documentID}': {e}");
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
