using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestionResultButton : MonoBehaviour
{
    public TMP_Text buttonText;
    public Button button;
    public Image buttonImage;

    private int questionID;
    private string respostaUsuario;
    private string respostaCorreta;
    private string habilidade;
    private bool estaCorreta;

    public void Configurar(int id, string respostaUsu, string respostaCor, string hab, bool correta)
    {
        questionID = id;
        respostaUsuario = respostaUsu;
        respostaCorreta = respostaCor;
        habilidade = hab;
        estaCorreta = correta;

        if (buttonText != null)
        {
            // Apenas o número da questão
            buttonText.text = (questionID + 1).ToString();
            buttonText.color = Color.white;
        }

        if (buttonImage != null)
        {
            // Define cor do botão de acordo com acerto/erro
            buttonImage.color = estaCorreta ? Color.green : Color.red;
        }
    }
}
