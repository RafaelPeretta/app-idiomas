using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class ResultadosLoader : MonoBehaviour
{
    [Header("Configurações")]
    public string userId;
    public Transform parent;
    public GameObject resultadoPrefab;

    private FirebaseFirestore db;

    private async void Start()
    {
        db = FirebaseFirestore.DefaultInstance;

        if (string.IsNullOrEmpty(userId))
            userId = UserDataManager.userInstance.GetUserId();

        await CarregarResultados();
    }

    private async Task CarregarResultados()
    {
        Query query = db.Collection("realizados").WhereEqualTo("userId", userId);
        QuerySnapshot snap = await query.GetSnapshotAsync();

        foreach (DocumentSnapshot doc in snap.Documents)
        {
            CriarResultadoPrefab(doc);
        }
    }

    private void CriarResultadoPrefab(DocumentSnapshot doc)
    {
        GameObject obj = Instantiate(resultadoPrefab, parent);

        // --- CAMPOS DO PREFAB ---
        TMP_Text aprovadoText = obj.transform.Find("AprovadoText").GetComponent<TMP_Text>();
        TMP_Text notaFinalText = obj.transform.Find("NotaFinalText").GetComponent<TMP_Text>();
        BarGraphGenerator grafico = obj.GetComponentInChildren<BarGraphGenerator>();

        // --- DADOS DO DOCUMENTO ---
        Dictionary<string, object> dados = doc.ToDictionary();

        float notaFinal = float.Parse(dados["notaFinal"].ToString());
        var lista = dados["pontuacaoPorHabilidade"] as List<object>;

        // Converter pontuação por habilidade
        Dictionary<string, float> porcentagens = new Dictionary<string, float>();

        foreach (var item in lista)
        {
            var d = item as Dictionary<string, object>;
            string hab = d["habilidade"].ToString();
            float nota = float.Parse(d["nota"].ToString());
            porcentagens[hab] = nota;
        }

        // --- APROVADO / REPROVADO ---
        if (notaFinal >= 80)
        {
            aprovadoText.text = "APROVADO";
            aprovadoText.color = Color.green;
        }
        else
        {
            aprovadoText.text = "REPROVADO";
            aprovadoText.color = Color.red;
        }

        // --- NOTA FINAL ---
        notaFinalText.text = $"Nota Final: {notaFinal:F1}%";

        // --- GERAR GRÁFICO ---
        if (grafico != null)
            grafico.GerarGrafico(porcentagens);
    }
}
