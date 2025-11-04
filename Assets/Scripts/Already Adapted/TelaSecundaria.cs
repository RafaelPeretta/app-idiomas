using UnityEngine;

public class TelaSecundariaController : MonoBehaviour
{
    [Header("Tela secundária controlada por este script")]
    [Tooltip("Arraste aqui o GameObject da tela secundária que será exibida por cima.")]
    public GameObject telaSecundaria;

    private void Start()
    {
        // Garante que a tela comece desativada
        if (telaSecundaria != null)
            telaSecundaria.SetActive(false);
        else
            Debug.LogWarning("[TelaSecundariaController] Nenhuma tela secundária definida!");
    }

    /// <summary>
    /// Ativa a tela secundária.
    /// </summary>
    public void AbrirTela()
    {
        if (telaSecundaria != null)
            telaSecundaria.SetActive(true);
    }

    /// <summary>
    /// Desativa a tela secundária.
    /// </summary>
    public void FecharTela()
    {
        if (telaSecundaria != null)
            telaSecundaria.SetActive(false);
    }
}
