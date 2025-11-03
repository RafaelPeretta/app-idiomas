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

        string path = Path.Combine(Application.streamingAssetsPath, "diagnostica unificada revisada.json");
        if (!File.Exists(path))
        {
            Debug.LogError("❌ Arquivo JSON não encontrado em StreamingAssets!");
            return;
        }

        string json = File.ReadAllText(path);
        DiagnosticaWrapper diagnostica = JsonUtility.FromJson<DiagnosticaWrapper>(json);

        await EnviarDiagnostica(diagnostica);
    }

    private async Task EnviarDiagnostica(DiagnosticaWrapper d)
    {
        // Prepara lista de questões
        var questoesList = new List<Dictionary<string, object>>();

        for (int i = 0; i < d.diagnostica_6ano.Count; i++)
        {
            var q = d.diagnostica_6ano[i];

            var questaoDict = new Dictionary<string, object>
            {
                {"Tipo", q.Tipo},
                {"Midia", q.Midia},
                {"Texto", q.Texto},
                {"Questao", q.Questão},
                {"Alternativas", q.Alternativas},
                {"Explicacoes", q.Explicacoes},
                {"Habilidades", q.Habilidades},
                {"RespostaCorreta", q.RespostaCorreta}
            };

            questoesList.Add(questaoDict);
        }

        // Monta o documento final
        var dados = new Dictionary<string, object>
        {
            {"avaliacaoID", "diagnostica_6ano"},
            {"titulo", "Avaliação Diagnóstica - 6º Ano"},
            {"descricao", "Banco de questões de diagnóstico de inglês - 6º ano"},
            {"questoes", questoesList}
        };

        // Envia para Firestore
        await db.Collection("avaliacoes").Document("diagnostica_6ano").SetAsync(dados);
        Debug.Log("✅ Avaliação diagnóstica enviada com sucesso!");
    }
}

// ---------- CLASSES DE DADOS ---------- //

[System.Serializable]
public class DiagnosticaWrapper
{
    public List<Questao> diagnostica_6ano;
}

[System.Serializable]
public class Questao
{
    public string Tipo;
    public string Midia;
    public string Texto;
    public string Questão; // usa "Questão" (com til) porque é o nome original do JSON
    public List<string> Alternativas;
    public List<string> Explicacoes;
    public string Habilidades;
    public object RespostaCorreta; // pode ser string ou lista
}
