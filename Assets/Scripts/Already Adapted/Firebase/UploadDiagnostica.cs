using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using System.IO;
using System.Threading.Tasks;

public class DiagnosticaUploader : MonoBehaviour
{
    private FirebaseFirestore db;

    private async void Start()
    {
        db = FirebaseFirestore.DefaultInstance;

        // Caminho do JSON na pasta StreamingAssets
        string path = Path.Combine(Application.streamingAssetsPath, "diagnostica unificada revisada.json");
        if (!File.Exists(path))
        {
            Debug.LogError("❌ Arquivo JSON não encontrado em StreamingAssets!");
            return;
        }

        string json = File.ReadAllText(path);

        // Lê o JSON diretamente como DiagnosticaWrapper
        DiagnosticaWrapper diagnostica = JsonUtility.FromJson<DiagnosticaWrapper>(json);

        await EnviarDiagnostica(diagnostica);
    }

    private async Task EnviarDiagnostica(DiagnosticaWrapper d)
    {
        var questoesList = new List<Dictionary<string, object>>();

        foreach (var q in d.diagnostica_6ano)
        {
            var questaoDict = new Dictionary<string, object>
            {
                {"Tipo", q.Tipo},
                {"Midia", q.Midia},
                {"Texto", q.Texto},
                {"Questao", q.Questao},
                {"Alternativas", q.Alternativas},
                {"Explicacoes", q.Explicacoes},      // agora é string
                {"Objetivo", q.Objetivo},            // novo campo (hipótese)
                {"Habilidades", q.Habilidades},      // agora array/list
                {"RespostaCorreta", q.RespostaCorreta}
            };

            questoesList.Add(questaoDict);
        }

        var dados = new Dictionary<string, object>
        {
            {"avaliacaoID", "diagnostica_6ano"},
            {"titulo", "Avaliação Diagnóstica - 6º Ano"},
            {"descricao", "Banco de questões de diagnóstico de inglês - 6º ano"},
            {"questoes", questoesList}
        };

        await db.Collection("avaliacoes").Document("diagnostica_6ano").SetAsync(dados);
        Debug.Log("✅ Avaliação diagnóstica enviada com sucesso!");
    }
}

// ---------- CLASSES DE DADOS ---------- //

[System.Serializable]
public class DiagnosticaWrapper
{
    public List<Questoes> diagnostica_6ano;
}

[System.Serializable]
public class Questoes
{
    public string Tipo;
    public string Midia;
    public string Texto;
    public string Questao;

    public List<string> Alternativas;

    public string Explicacoes;            // ALTERADO: agora é string
    public string Objetivo;               // NOVO CAMPO

    public List<string> Habilidades;      // ALTERADO: agora é lista/array

    public List<string> RespostaCorreta;  // permanece lista
}
