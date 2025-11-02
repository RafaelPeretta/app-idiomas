using Firebase.Auth;
using UnityEngine;
using TMPro;
using System.Collections;

public class AuthForgotPassword : MonoBehaviour
{
    [Header("Inputs")]
    public TMP_InputField InputEmail;

    [Header("UI")]
    public TextMeshProUGUI OutputMessage;

    private FirebaseAuth auth;

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        OutputMessage.gameObject.SetActive(false);
    }

    public void PreencherEmailSeLogado()
    {
        if (auth.CurrentUser != null && !string.IsNullOrEmpty(auth.CurrentUser.Email))
        {
            InputEmail.text = auth.CurrentUser.Email;
            Debug.Log($"Email preenchido automaticamente: {auth.CurrentUser.Email}");
        }
    }

    public void EnviarEmailReset()
    {

        string email = InputEmail.text.Trim();

        if (string.IsNullOrEmpty(email))
        {
            MostrarMensagem("Informe seu email.");
            return;
        }

        auth.SendPasswordResetEmailAsync(email).ContinueWith(task =>
        {
            MostrarMensagem("Se o e-mail estiver cadastrado, você receberá um link para redefinição de senha.");

            if (task.Exception != null)
            {
                Debug.LogError($"Erro ao enviar email de redefinição: {task.Exception}");
            }
            else
            {
                Debug.Log($"Pedido de redefinição de senha enviado para {email}");

                // Se o usuário estiver logado, agenda logout após 10s
                if (auth.CurrentUser != null)
                {
                    StartCoroutine(DesconectarComContagemRegressiva(10f));
                    // Debug.LogWarning("DESCONECTANDO USUÁRIO...");
                }
            }
        });

        
    }

    private void MostrarMensagem(string mensagem)
    {
        OutputMessage.gameObject.SetActive(true);
        OutputMessage.text = mensagem;
    }

    private IEnumerator DesconectarComContagemRegressiva(float delay)
    {
        float tempoRestante = delay;

        while (tempoRestante > 0)
        {
            MostrarMensagem($"Se o e-mail existir, um link foi enviado.\nVocê será desconectado em {Mathf.CeilToInt(tempoRestante)} segundos...");
            yield return new WaitForSeconds(1f);
            tempoRestante--;
        }

        auth.SignOut();
        Debug.Log("Usuário desconectado após redefinição de senha.");
        GameManager.Instance.irParaCena("Auth Beta");
    }
}
