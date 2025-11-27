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

        foreach (var kvp in valores)
        {
            string habilidade = kvp.Key;
            float porcentagem = kvp.Value;

            GameObject barObj = Instantiate(barPrefab, barContainer);

            Slider slider = barObj.transform.Find("barra").GetComponent<Slider>();
            Image fillImg = barObj.transform.Find("barra/Fill").GetComponent<Image>();
            TMP_Text txtHabilidade = barObj.transform.Find("Nome").GetComponent<TMP_Text>();
            TMP_Text txtPercent = barObj.transform.Find("Percent").GetComponent<TMP_Text>();

            // Nome da habilidade
            if (habilidadeNomes.ContainsKey(habilidade))
                txtHabilidade.text = habilidadeNomes[habilidade];
            else
                txtHabilidade.text = habilidade;

            // Texto do percentual
            txtPercent.text = Mathf.RoundToInt(porcentagem) + "%";

            // Valor da barra
            slider.value = porcentagem;

            // Cor baseada no desempenho
            if (porcentagem >= 80f)
                fillImg.color = Color.green;
            else if (porcentagem >= 50f)
                fillImg.color = Color.yellow;
            else
                fillImg.color = Color.red;
        }
    }
}
