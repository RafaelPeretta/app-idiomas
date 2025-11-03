using UnityEngine;
using TMPro;
using System.IO;

public class MostrarTermos : MonoBehaviour
{
    public TextMeshProUGUI textoTermos; // arraste o TMP no inspetor
    private string caminho;

    public GameObject termoScreen;

    private bool ativo = false;

    void Start()
    {
        caminho = Path.Combine(Application.streamingAssetsPath, "termos_chigo.txt");
        CarregarTermos();

        termoScreen.gameObject.SetActive(ativo);
    }

    void CarregarTermos()
    {
        if (File.Exists(caminho))
        {
            textoTermos.text = File.ReadAllText(caminho);
        }
        else
        {
            textoTermos.text = "Arquivo de termos não encontrado.";
        }
    }

    public void termos()
    {
        ativo = !ativo;
        termoScreen.gameObject.SetActive(ativo);
    }
}
