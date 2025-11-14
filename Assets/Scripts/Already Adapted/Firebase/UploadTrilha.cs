using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using System.IO;
using System.Threading.Tasks;

public class TrilhaUploader : MonoBehaviour
{
    private FirebaseFirestore db;

    private async void Start()
    {
        db = FirebaseFirestore.DefaultInstance;

        // Lista das trilhas a enviar (nomes dos arquivos JSON)
        string[] trilhas = { "TRILHA001", "TRILHA002", "TRILHA003", "TRILHA004", "TRILHA005" };

        foreach (string trilhaID in trilhas)
        {
            await ProcessarTrilha(trilhaID);
        }

        Debug.Log("🔥 Todas as trilhas foram enviadas!");
    }

    private async Task ProcessarTrilha(string trilhaID)
    {
        string path = Path.Combine(Application.streamingAssetsPath, trilhaID + ".json");

        if (!File.Exists(path))
        {
            Debug.LogError($"❌ Arquivo {trilhaID}.json não encontrado!");
            return;
        }

        string json = File.ReadAllText(path);
        TrilhaData trilha = null;

        // Desserializa para o wrapper correto de cada trilha
        switch (trilhaID)
        {
            case "TRILHA001": trilha = JsonUtility.FromJson<TrilhaWrapper001>(json).TRILHA001; break;
            case "TRILHA002": trilha = JsonUtility.FromJson<TrilhaWrapper002>(json).TRILHA002; break;
            case "TRILHA003": trilha = JsonUtility.FromJson<TrilhaWrapper003>(json).TRILHA003; break;
            case "TRILHA004": trilha = JsonUtility.FromJson<TrilhaWrapper004>(json).TRILHA004; break;
            case "TRILHA005": trilha = JsonUtility.FromJson<TrilhaWrapper005>(json).TRILHA005; break;
        }

        if (trilha == null)
        {
            Debug.LogError($"❌ Chave '{trilhaID}' não encontrada no JSON!");
            return;
        }

        await EnviarTrilha(trilhaID, trilha);
    }

    private async Task EnviarTrilha(string trilhaID, TrilhaData trilha)
    {
        var questoesList = new List<Dictionary<string, object>>();

        foreach (var q in trilha.Questoes)
        {
            questoesList.Add(new Dictionary<string, object>
            {
                {"Tipo", q.Tipo},
                {"Midia", q.Midia},
                {"Texto", q.Texto},
                {"Questao", q.Questao},
                {"Alternativas", q.Alternativas},
                {"Explicacao", q.Explicacao},
                {"Habilidades", q.Habilidades},
                {"RespostaCorreta", q.RespostaCorreta}
            });
        }

        var trilhaDict = new Dictionary<string, object>
        {
            {"ID", trilha.ID},
            {"Nome", trilha.Nome},
            {"Descricao", trilha.Descricao},
            {"Habilidades", trilha.Habilidades},
            {"Questoes", questoesList}
        };

        await db.Collection("trilhas").Document(trilhaID).SetAsync(trilhaDict);
        Debug.Log($"✅ {trilhaID} enviada!");

        await AtualizarReferencia(trilhaID, trilha);
    }

    private async Task AtualizarReferencia(string trilhaID, TrilhaData trilha)
    {
        var referencia = new Dictionary<string, object>()
        {
            {"ID", trilha.ID},
            {"Nome", trilha.Nome},
            {"Descricao", trilha.Descricao},
            {"Habilidades", trilha.Habilidades}
        };

        var updateData = new Dictionary<string, object>()
        {
            {"Lista", FieldValue.ArrayUnion(referencia)}
        };

        await db.Collection("referencias")
                .Document("Trilhas")
                .SetAsync(updateData, SetOptions.MergeAll);

        Debug.Log($"📌 Referencia atualizada: {trilhaID}");
    }
}

// ---------------------------
// WRAPPERS INDIVIDUAIS PARA CADA TRILHA
// ---------------------------

[System.Serializable] public class TrilhaWrapper001 { public TrilhaData TRILHA001; }
[System.Serializable] public class TrilhaWrapper002 { public TrilhaData TRILHA002; }
[System.Serializable] public class TrilhaWrapper003 { public TrilhaData TRILHA003; }
[System.Serializable] public class TrilhaWrapper004 { public TrilhaData TRILHA004; }
[System.Serializable] public class TrilhaWrapper005 { public TrilhaData TRILHA005; }

[System.Serializable]
public class TrilhaData
{
    public string ID;
    public string Nome;
    public string Descricao;
    public List<string> Habilidades;
    public List<TrilhaQuestao> Questoes;
}

[System.Serializable]
public class TrilhaQuestao
{
    public string Tipo;
    public string Midia;
    public string Texto;
    public string Questao;
    public List<string> Alternativas;
    public string Explicacao;
    public List<string> Habilidades;
    public string RespostaCorreta;
}
