using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using System.Threading.Tasks;

public class QuizUploader : MonoBehaviour
{
    private FirebaseFirestore db;

    private async void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        await GerarEEnviarQuizzes();
    }

    private async Task GerarEEnviarQuizzes()
    {
        List<Dictionary<string, object>> listaQuizzes = new List<Dictionary<string, object>>();

        for (int i = 1; i <= 5; i++)
        {
            string id = $"QUIZ{i:D3}";
            string trilhaID = $"TRILHA{i:D3}";

            listaQuizzes.Add(new Dictionary<string, object>
            {
                {"ID", id},
                {"TrilhaID", trilhaID}
            });
        }

        var quizDict = new Dictionary<string, object>
        {
            {"Lista", listaQuizzes}
        };

        try
        {
            await db.Collection("referencias").Document("Quiz").SetAsync(quizDict);
            Debug.Log("✅ 5 quizzes enviados com sucesso para Firestore!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Erro ao enviar quizzes: {ex}");
        }
    }
}
