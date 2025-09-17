using UnityEngine;
using Firebase; 
using Firebase.Auth; 
using TMPro;         
using System.Collections;
using UnityEngine.SceneManagement; 

public class LoginAuth : MonoBehaviour
{
    [Header("UI de Login")]
    public TMP_InputField emailInput;    
    public TMP_InputField passwordInput; 

    [Header("Feedback para o Usuário")]
    public TMP_Text warningLoginText; 
    public TMP_Text confirmLoginText; 


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

            Debug.LogWarning($"Falha ao fazer login com {loginTask.Exception}");
            FirebaseException firebaseEx = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
            warningLoginText.text = GetLoginErrorMessage(errorCode);
        }
        else
        {

            AuthResult result = loginTask.Result;
            FirebaseUser user = result.User;


            warningLoginText.text = "";
            confirmLoginText.text = "Login bem-sucedido!";
            Debug.LogFormat("Usuário logado com sucesso: {0} ({1})", user.DisplayName, user.Email);


            yield return new WaitForSeconds(2);
            SceneManager.LoadScene("TelaPrincipal");
        }
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