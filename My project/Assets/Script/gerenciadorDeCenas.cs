using UnityEngine;
using UnityEngine.SceneManagement;

public class GerenciadorDeCenas : MonoBehaviour
{
    // metodo público para carregar uma cena pelo nome
    public void MudarCena(string nomeDaCena)
    {
        SceneManager.LoadScene(nomeDaCena);
    }

    // Método para navegar para o quiz diário
    public void IrParaQuizDiario()
    {
        MudarCena("quizDiario"); 
    }

    // Método para voltar para a tela principal
    public void VoltarParaTelaPrincipal()
    {
        MudarCena("telaPrincipal");
    }
}