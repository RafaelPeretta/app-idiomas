using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;

public class SaveToFirestore : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField inputField;
    public Button saveButton;

    FirebaseFirestore db;

    void Start()
    {
        // Inicializa Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                db = FirebaseFirestore.DefaultInstance;
                Debug.Log("Firestore inicializado com sucesso!");

                // Listener do botão
                saveButton.onClick.AddListener(SaveData);
            }
            else
            {
                Debug.LogError("Não foi possível inicializar Firebase: " + task.Result);
            }
        });
    }

    void SaveData()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            Debug.LogWarning("Campo de input vazio!");
            return;
        }

        // Cria objeto para salvar
        var data = new
        {
            text = inputField.text,
            timestamp = Timestamp.GetCurrentTimestamp()
        };

        // Salva em uma coleção chamada "Inputs"
        db.Collection("Inputs").AddAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Dado salvo no Firestore com sucesso!");
                inputField.text = ""; // limpa campo
            }
            else
            {
                Debug.LogError("Erro ao salvar no Firestore: " + task.Exception);
            }
        });
    }
}
