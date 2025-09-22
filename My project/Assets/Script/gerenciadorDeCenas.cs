using UnityEngine;
using UnityEngine.SceneManagement;

public class GerenciadorDeCenas : MonoBehaviour
{
    // metodo público para carregar uma cena pelo nome
    public void MudarCena(string nomeDaCena)
    {
        SceneManager.LoadScene(nomeDaCena);
    }
}