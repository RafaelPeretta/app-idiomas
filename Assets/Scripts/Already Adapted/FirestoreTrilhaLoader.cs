using UnityEngine;
using System;
using System.Collections.Generic;
using Firebase.Firestore;

public class TrilhaLoader : MonoBehaviour
{
    public static TrilhaLoader Instance { get; private set; }
    private FirebaseFirestore db;

    public event Action OnTrilhaLoaded;

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
            db = UserDataManager.userInstance.DatabaseFirestore;
        }
        else
        {
            Debug.LogError("[TrilhaLoader] UserDataManager não encontrado.");
        }
    }

    public void LoadTrilha(string trilhaID)
    {
        if (db == null)
        {
            Debug.LogError("[TrilhaLoader] Firestore não inicializado.");
            return;
        }

        LoadTrilhaFromFirestore(trilhaID);
    }

    private async void LoadTrilhaFromFirestore(string trilhaID)
    {
        try
        {
            DocumentReference docRef = db.Collection("trilhas").Document(trilhaID);
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

            TrilhaDataLoad loadedTrilha = new TrilhaDataLoad
            {
                id = trilhaID,  // Preenche o ID da trilha
                questoes = loadedQuestions
            };

            if (TrilhaManager.Instance != null)
            {
                TrilhaManager.Instance.currentTrilha = loadedTrilha;
                Debug.Log($"[TrilhaLoader] Trilha '{trilhaID}' carregada ({loadedQuestions.Count} questões).");
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
