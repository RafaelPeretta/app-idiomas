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

        // Lista das trilhas a enviar
        string[] trilhas = { "Trilha1", "Trilha2", "Trilha3", "Trilha4", "Trilha5" };

        foreach (string id in trilhas)
        {
            await ProcessarTrilha(id);
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

        // Wrapper genérico
        TrilhaWrapper wrapper = JsonUtility.FromJson<TrilhaWrapper>(json);

        // Reflexão manual: pegar o campo correspondente (Trilha1, Trilha2, etc)
        TrilhaData trilha = null;

        switch (trilhaID)
        {
            case "Trilha1": trilha = wrapper.Trilha1; break;
            case "Trilha2": trilha = wrapper.Trilha2; break;
            case "Trilha3": trilha = wrapper.Trilha3; break;
            case "Trilha4": trilha = wrapper.Trilha4; break;
            case "Trilha5": trilha = wrapper.Trilha5; break;
        }

        if (trilha == null)
        {
            Debug.LogError($"❌ JSON lido, mas chave '{trilhaID}' está nula!");
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
            {"ID", trilhaID},
            {"Nome", trilha.Nome},
            {"Descricao", trilha.Descricao},
            {"Habilidades", trilha.Habilidades}
        };

        var updateData = new Dictionary<string, object>()
        {
            {"Lista", FieldValue.ArrayUnion(referencia)}
        };

        await FirebaseFirestore.DefaultInstance
            .Collection("referencias")
            .Document("Trilhas")
            .SetAsync(updateData, SetOptions.MergeAll);

        Debug.Log($"📌 Referencia atualizada: {trilhaID}");
    }
}


// ------------------------------------------------------
// WRAPPER SUPORTA TODAS AS TRILHAS
// ------------------------------------------------------

[System.Serializable]
public class TrilhaWrapper
{
    public TrilhaData Trilha1;
    public TrilhaData Trilha2;
    public TrilhaData Trilha3;
    public TrilhaData Trilha4;
    public TrilhaData Trilha5;
}

[System.Serializable]
public class TrilhaData
{
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
