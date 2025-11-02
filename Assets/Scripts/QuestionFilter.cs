using Firebase.Firestore;
using UnityEngine;
using System.Collections.Generic;

public class QuestionFilter : MonoBehaviour
{
    // Lista estática para armazenar os IDs das questões sorteadas
    public static List<string> questoesSorteadas = new List<string>();

    public static void randomQuestion(string faseID)
    {
        // Limpa a lista a cada nova chamada
        questoesSorteadas.Clear();

        if (string.IsNullOrEmpty(faseID))
        {
            Debug.LogWarning("Nenhuma Fase Selecionada | faseID está vazio ou nulo");
            return;
        }

        int allQuestions = GameManager.Instance.faseQuestoes; // total de questões disponíveis
        int maxQuestions = GameManager.Instance.maxQuestoes;  // quantas sortear

        if (maxQuestions > allQuestions)
        {
            Debug.LogWarning("maxQuestions é maior que allQuestions. Ajustando...");
            maxQuestions = allQuestions;
        }

        // Criar lista com todos os números de questão
        List<int> pool = new List<int>();
        for (int i = 1; i <= allQuestions; i++)
        {
            pool.Add(i);
        }

        // Lista final com os números sorteados
        List<int> sorteados = new List<int>();
        System.Random rnd = new System.Random();

        for (int i = 0; i < maxQuestions; i++)
        {
            int index = rnd.Next(pool.Count);   // pegar índice aleatório
            sorteados.Add(pool[index]);         // adicionar número sorteado
            pool.RemoveAt(index);               // remover para evitar repetição
        }

        // Armazenar cada questão sorteada na lista estática e logar individualmente
        foreach (int numero in sorteados)
        {
            string questaoID = $"{faseID}-Q{numero:D3}";
            questoesSorteadas.Add(questaoID);
            // Debug.Log("Fase ID: " + questaoID);
        }

        // Logar a lista completa no final
        // Debug.Log("Lista completa de questões sorteadas: " + string.Join(", ", questoesSorteadas));
    }
}
