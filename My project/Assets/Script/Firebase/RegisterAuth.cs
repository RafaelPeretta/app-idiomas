using UnityEngine;
using Firebase.Auth;
using Firebase;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class RegisterAuth : MonoBehaviour
{
    [Header("UI de Registo")]
    public TMP_InputField usernameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    [Header("Feedback para o Usuário")]
    public TMP_Text warningRegisterText;
    public TMP_Text confirmRegisterText;

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

    public void RegisterButton()
    {
        string username = usernameInput.text;
        string email = emailInput.text;
        string password = passwordInput.text;


        if (!IsEmailDomainAllowed(email))
        {
            warningRegisterText.text = "Domínio de e-mail não permitido.";
            return;
        }


        StartCoroutine(Register(username, email, password));
    }

    private IEnumerator Register(string username, string email, string password)
    {
        var auth = FirebaseAuthenticator.Instance.auth;
        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        
        yield return new WaitUntil(predicate: () => registerTask.IsCompleted);

        if (registerTask.Exception != null)
        {
            Debug.LogWarning($"Falha ao registar tarefa com {registerTask.Exception}");
            FirebaseException firebaseEx = registerTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
            warningRegisterText.text = GetRegisterErrorMessage(errorCode);
        }
        else
        {
            AuthResult result = registerTask.Result;
            FirebaseUser newUser = result.User;

            if (newUser != null)
            {
                UserProfile profile = new UserProfile { DisplayName = username };
                var profileTask = newUser.UpdateUserProfileAsync(profile);

                yield return new WaitUntil(predicate: () => profileTask.IsCompleted);

                if (profileTask.Exception != null)
                {
                    Debug.LogWarning($"Falha ao atualizar perfil com {profileTask.Exception}");
                    warningRegisterText.text = "Falha ao salvar o nome de usuário.";
                }
                else
                {
                    warningRegisterText.text = "";
                    confirmRegisterText.text = "Usuário registrado com sucesso!";
                    Debug.Log("Cadastro e atualização de perfil concluídos!");

                    yield return new WaitForSeconds(2);
                    SceneManager.LoadScene("telaLogin");
                }
            }
        }
    }


    private bool IsEmailDomainAllowed(string email)
    {
        if (string.IsNullOrEmpty(email)) return false;
        string[] allowedDomains = { "@gmail.com", "@hotmail.com", "@icloud.com" };
        foreach (var domain in allowedDomains)
        {
            if (email.EndsWith(domain))
            {
                return true;
            }
        }
        return false;
    }


    private string GetRegisterErrorMessage(AuthError errorCode)
    {
        switch (errorCode)
        {
            case AuthError.MissingEmail:
                return "Por favor, insira um e-mail.";
            case AuthError.MissingPassword:
                return "Por favor, insira uma senha.";
            case AuthError.WeakPassword:
                return "A senha precisa de no mínimo 6 caracteres.";
            case AuthError.InvalidEmail:
                return "O e-mail inserido é inválido.";
            case AuthError.EmailAlreadyInUse:
                return "Este e-mail já está em uso.";
            default:
                return "Ocorreu um erro no cadastro.";
        }
    }
}