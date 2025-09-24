using UnityEngine;
using Firebase.Auth;
using Firebase;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 

public class LoginAuth : MonoBehaviour
{
    [Header("UI de Login")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput; 
    public Button loginButton;         

    [Header("Feedback para o Usuário")]
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;

    private int loginAttempts = 0;
    private const int maxLoginAttempts = 5;

    public void MostrarSenha()
    {
        passwordInput.contentType = TMP_InputField.ContentType.Standard;
        passwordInput.ForceLabelUpdate();
    }

    public void EsconderSenha()
    {
        passwordInput.contentType = TMP_InputField.ContentType.Password;
        passwordInput.ForceLabelUpdate();
    }

    public void LoginButton()
    {
        StartCoroutine(Login(emailInput.text, passwordInput.text));
    }

    private IEnumerator Login(string email, string password)
    {
        var auth = FirebaseAuthenticator.Instance.auth;
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        
        yield return new WaitUntil(predicate: () => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            loginAttempts++;
            Debug.LogWarning($"Falha ao fazer login. Tentativa nº {loginAttempts}");
            FirebaseException firebaseEx = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
            warningLoginText.text = GetLoginErrorMessage(errorCode);

            if (loginAttempts >= maxLoginAttempts)
            {
                StartCoroutine(DisableLoginForDuration(15f));
            }
        }
        else
        {
            loginAttempts = 0;
            AuthResult result = loginTask.Result;
            FirebaseUser user = result.User;

            warningLoginText.text = "";
            confirmLoginText.text = "Login bem-sucedido!";
            Debug.LogFormat("Usuário logado com sucesso: {0} ({1})", user.DisplayName, user.Email);

            yield return new WaitForSeconds(2);
            SceneManager.LoadScene("TelaPrincipal"); 
        }
    }

    private IEnumerator DisableLoginForDuration(float duration)
    {
        loginButton.interactable = false;
        warningLoginText.text = $"Muitas tentativas falhadas. Tente novamente em {duration} segundos.";
        
        yield return new WaitForSeconds(duration);
        
        loginAttempts = 0;
        loginButton.interactable = true;
        warningLoginText.text = "";
    }

    private string GetLoginErrorMessage(AuthError errorCode)
    {
        switch (errorCode)
        {
            case AuthError.MissingEmail:
                return "Por favor, insira seu e-mail.";
            case AuthError.MissingPassword:
                return "Por favor, insira sua senha.";
            case AuthError.WrongPassword:
                return "Senha incorreta.";
            case AuthError.InvalidEmail:
                return "O e-mail inserido é inválido.";
            case AuthError.UserNotFound:
                return "Não há nenhuma conta com este e-mail.";
            default:
                return "Ocorreu um erro no login.";
        }
    }
}