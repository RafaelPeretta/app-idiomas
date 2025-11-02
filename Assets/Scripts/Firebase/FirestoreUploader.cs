using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;

public class FirestoreUpload : MonoBehaviour
{
    private FirebaseFirestore db;

    private void Start()
    {
        // Inicializa o Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                db = FirebaseFirestore.DefaultInstance;
                Debug.Log("Firebase inicializado com sucesso!");
                UploadLevelN5();
            }
            else
            {
                Debug.LogError("Não foi possível inicializar o Firebase: " + task.Result);
            }
        });
    }

    private void UploadLevelN5()
    {
        // Cria os dados do documento
        Dictionary<string, object> levelData = new Dictionary<string, object>
        {
            { "VOCABULARIO", new List<Dictionary<string, object>>() },
            { "ESCUTA", new List<Dictionary<string, object>>() },
            { "KANJI", new List<Dictionary<string, object>>() },
            { "LEITURA", new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> { { "id", "N5L001" }, { "nome", "Hiragana I" } },
                    new Dictionary<string, object> { { "id", "N5L002" }, { "nome", "Hiragana II" } },
                    new Dictionary<string, object> { { "id", "N5L003" }, { "nome", "Hiragana III" } },
                    new Dictionary<string, object> { { "id", "N5L004" }, { "nome", "Katakana I" } },
                    new Dictionary<string, object> { { "id", "N5L005" }, { "nome", "Katakana II" } },
                    new Dictionary<string, object> { { "id", "N5L006" }, { "nome", "Katakana III" } }
                }
            },
            { "GRAMATICA", new List<Dictionary<string, object>>() },
            { "SIMULADO", new List<Dictionary<string, object>>() }
        };

        // Referência ao documento
        DocumentReference docRef = db.Collection("LEVELS").Document("N5");

        // Sobrescreve ou cria o documento
        docRef.SetAsync(levelData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log("Documento LEVELS/N5 criado com sucesso!");
            }
            else
            {
                Debug.LogError("Erro ao criar documento: " + task.Exception);
            }
        });
    }
}
