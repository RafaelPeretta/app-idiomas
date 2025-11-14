using UnityEngine;

public class TelaSecundariaController : MonoBehaviour
{
    [Header("Telas secundárias controladas por este script")]
    public GameObject telaSecundaria1;
    public GameObject telaSecundaria2;

    private void Start()
    {
        if (telaSecundaria1 != null)
            telaSecundaria1.SetActive(false);
        if (telaSecundaria2 != null)
            telaSecundaria2.SetActive(false);
    }

    public void AbrirTela1()
    {
        if (telaSecundaria1 != null)
            telaSecundaria1.SetActive(true);
    }

    public void FecharTela1()
    {
        if (telaSecundaria1 != null)
            telaSecundaria1.SetActive(false);
    }

    public void AbrirTela2()
    {
        if (telaSecundaria2 != null)
            telaSecundaria2.SetActive(true);
    }

    public void FecharTela2()
    {
        if (telaSecundaria2 != null)
            telaSecundaria2.SetActive(false);
    }
}
