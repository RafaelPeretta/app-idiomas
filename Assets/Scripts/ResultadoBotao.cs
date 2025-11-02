using UnityEngine;

public class ResultadoBotao : MonoBehaviour
{
    public string faseID;
    public bool acerto;
    public string respostaUsuario;

    public void SetData(string faseID, bool acerto, string respostaUsuario)
    {
        this.faseID = faseID;
        this.acerto = acerto;
        this.respostaUsuario = respostaUsuario;
    }
}
