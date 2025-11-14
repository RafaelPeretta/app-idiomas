using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BarGraphGenerator : MonoBehaviour
{
    [Header("Prefab da Barra")]
    public GameObject barPrefab;

    [Header("Onde as barras serão criadas")]
    public Transform barContainer;

    // Mapeamento dos códigos para nomes curtos
    private Dictionary<string, string> habilidadeNomes = new Dictionary<string, string>
    {
        { "EF06LI08", "Presente Verbal" },
        { "EF06LI05", "Inglês na Sociedade" },
        { "EF06LI19", "Rotina Diária" }
    };

    public void GerarGrafico(Dictionary<string, float> valores)
    {
        // Limpa barras anteriores
        foreach (Transform child in barContainer)
            Destroy(child.gameObject);

        RectTransform containerRect = barContainer.GetComponent<RectTransform>();
        float maxAltura = containerRect.rect.height;

        foreach (var kvp in valores)
        {
            string habilidade = kvp.Key;
            float porcentagem = kvp.Value;

            GameObject barObj = Instantiate(barPrefab, barContainer);

            RectTransform barFill = barObj.transform.Find("Fill").GetComponent<RectTransform>();
            TMP_Text txtHabilidade = barObj.transform.Find("Nome").GetComponent<TMP_Text>();
            TMP_Text txtPercent = barObj.transform.Find("Percent").GetComponent<TMP_Text>();

            // Substitui pelo nome curto, se existir
            if (habilidadeNomes.ContainsKey(habilidade))
                txtHabilidade.text = habilidadeNomes[habilidade];
            else
                txtHabilidade.text = habilidade; // se não tiver mapeamento, usa o código mesmo

            txtPercent.text = Mathf.RoundToInt(porcentagem) + "%";

            float altura = (porcentagem / 100f) * maxAltura;
            barFill.sizeDelta = new Vector2(barFill.sizeDelta.x, altura);

            // Define cor com base no desempenho
            Image fillImg = barFill.GetComponent<Image>();
            if (fillImg != null)
            {
                if (porcentagem >= 80f)
                    fillImg.color = Color.green;
                else if (porcentagem >= 50f)
                    fillImg.color = Color.yellow;
                else
                    fillImg.color = Color.red;
            }
        }
    }
}
