using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using System.IO;
using System.Threading.Tasks;

// Script principal (herda MonoBehaviour)
public class FasesUploader : MonoBehaviour
{
    private FirebaseFirestore db;

    private async void Start()
    {
        db = FirebaseFirestore.DefaultInstance;

        string path = Path.Combine(Application.streamingAssetsPath, "hiragana1.json");
        if (!File.Exists(path)) { Debug.LogError("JSON não encontrado"); return; }

        Fase fase = JsonUtility.FromJson<Fase>(File.ReadAllText(path));
        await EnviarFase(fase);
    }

    private async Task EnviarFase(Fase f)
    {
        var itensList = new List<Dictionary<string, object>>();
        foreach (var item in f.itens)
            itensList.Add(new Dictionary<string, object>
            {
                {"id", item.id},
                {"tipo", item.tipo},
                {"conteudo", item.conteudo},
                {"pergunta", item.pergunta},
                {"opcoes", item.opcoes},
                {"resposta", item.resposta}
            });

        var dados = new Dictionary<string, object>
        {
            {"faseID", f.faseID},
            {"titulo", f.titulo},
            {"topico", f.topico},
            {"descricao", f.descricao},
            {"itens", itensList}
        };

        await db.Collection("fases").Document(f.faseID).SetAsync(dados);
        Debug.Log($"✅ Fase enviada: {f.faseID}");
    }
}

// Classes de dados (não herdam MonoBehaviour)
[System.Serializable]
public class Fase
{
    public string faseID;
    public string titulo;
    public string topico;
    public string descricao;
    public List<FaseItem> itens;
}

[System.Serializable]
public class FaseItem
{
    public int id;
    public string tipo;
    public string conteudo;
    public string pergunta;
    public List<string> opcoes;
    public string resposta;
}
