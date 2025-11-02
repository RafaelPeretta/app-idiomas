using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions;

public class FaseCardUI : MonoBehaviour
{
    private string nomeText;
    private string descricaoText;
    private string faseID;
    
    private FirebaseFirestore db;
    private Button botao;

    // 🔹 Cores para estados
    private Color azul = new Color(0.2f, 0.4f, 0.9f);  // azul original
    private Color cinza = new Color(0.2f, 0.2f, 0.2f); // cinza escuro

    void Awake()
    {
        botao = GetComponent<Button>(); // pega o próprio botão do card
    }

    public void Setup(string id, string nome, string descricao)
    {
        faseID = id;
        nomeText = nome;
        descricaoText = descricao;

        db = FirebaseFirestore.DefaultInstance;

        VerificarProgresso();
    }

    private void VerificarProgresso()
    {
        string userID = UserDataManager.userInstance.GetUserId();

        DocumentReference docRef = db
            .Collection("FASES")
            .Document(faseID)
            .Collection("users")
            .Document(userID);

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                DocumentSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {
                    // ✅ Usuário já fez essa fase → card cinza
                    botao.image.color = cinza;
                }
                else
                {
                    // ❌ Usuário nunca fez → card azul
                    botao.image.color = azul;
                }
            }
            else
            {
                Debug.LogError("Erro ao verificar progresso do usuário: " + task.Exception);
                // fallback azul
                botao.image.color = azul;
            }
        });
    }
}
