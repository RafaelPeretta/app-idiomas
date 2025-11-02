using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class FaseData
{
    public string id;
    public string titulo;
    public string descricao;

    public FaseData(string id, string titulo, string descricao)
    {
        this.id = id;
        this.titulo = titulo;
        this.descricao = descricao;
    }
}

public class Categoria4FasesUI : MonoBehaviour
{
    [Header("Slots de Fase")]
    public Button[] faseButtons;      // 0 a 3
    public TMP_Text[] faseTexts;      // 0 a 3 (o texto do botão)
    public TMP_Text categoriaText;    // opcional, exibe N5-01, N5-02 etc.

    // Configura os slots com os dados das fases
    public void Setup(FaseData[] fases, string categoriaID, string categoriaTitulo, List<string> fasesCompletadas)
    {
        // Exibe o nome da categoria
        if (categoriaText != null)
            categoriaText.text = categoriaTitulo;

        for (int i = 0; i < faseButtons.Length; i++)
        {
            if (i < fases.Length)
            {
                faseTexts[i].text = fases[i].titulo;
                faseButtons[i].gameObject.SetActive(true);

                // Altera a cor do botão conforme se a fase já foi completada
                if (fasesCompletadas.Contains(fases[i].id))
                    faseButtons[i].GetComponent<Image>().color = Color.green; // concluída
                else
                    faseButtons[i].GetComponent<Image>().color = Color.gray; // não concluída

                int index = i; // captura índice para listener
                faseButtons[i].onClick.RemoveAllListeners();
                faseButtons[i].onClick.AddListener(() =>
                {
                    Vector3 posBotao = faseButtons[index].transform.position;
                    FasePanelController.FaseInstance.Show(
                        fases[index].id,
                        fases[index].titulo,
                        fases[index].descricao,
                        posBotao
                    );
                });
            }
            else
            {
                faseButtons[i].gameObject.SetActive(false);
            }
        }
    }
}

