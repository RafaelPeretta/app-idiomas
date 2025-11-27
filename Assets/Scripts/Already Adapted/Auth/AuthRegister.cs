using System.Collections;
using TMPro;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;

public class AuthRegister : MonoBehaviour
{
    [Header("Inputs")]
    public TMP_InputField InputName;
    public TMP_InputField InputEmail;
    public TMP_InputField InputConfirmEmail;
    public TMP_InputField InputPassword;
    public TMP_InputField InputConfirmPassword;

    [Header("UI")]
    public TextMeshProUGUI OutputMessage;
    public GameObject TermoCheck;

    private FirebaseAuth auth;
    private bool userAcceptedTerms = false;

    // -------------------------------
    // Inicialização
    // -------------------------------
    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                Debug.Log("Firebase Auth inicializado! (RegisterManager)");
            }
            else
            {
                Debug.LogError($"Não foi possível resolver todas as dependências do Firebase: {task.Result}");
            }
        });
        OutputMessage.gameObject.SetActive( false );
        TermoCheck.SetActive(userAcceptedTerms);
    }

    // -------------------------------
    // Toggle termos
    // -------------------------------
    public void CheckTermos()
    {
        userAcceptedTerms = !userAcceptedTerms;
        TermoCheck.SetActive(userAcceptedTerms);
    }

    // -------------------------------
    // Registrar usuário
    // -------------------------------
    public void Register()
    {
        if (auth == null)
        {
            Debug.LogError("FirebaseAuth não inicializado ainda!");
            OutputMessage.gameObject.SetActive(true);
            OutputMessage.text = "Erro: Firebase ainda não foi inicializado. Tente novamente em alguns segundos.";
            return;
        }

        if (!ValidateInputs())
            return;

        StartCoroutine(CreateUserCoroutine());
    }

    // -------------------------------
    // Validação de campos e termos
    // -------------------------------
    private bool ValidateInputs()
    {
        string email = InputEmail.text.Trim();
        string confirmEmail = InputConfirmEmail.text.Trim();
        string password = InputPassword.text.Trim();
        string confirmPassword = InputConfirmPassword.text.Trim();
        string name_ = InputName.text.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(name_))
        {
            OutputMessage.gameObject.SetActive(true);
            OutputMessage.text = "Preencha todos os campos corretamente.";
            return false;
        }

        if (email != confirmEmail)
        {
            OutputMessage.gameObject.SetActive(true);
            OutputMessage.text = "Emails distintos";
            return false;
        }

        if (password != confirmPassword)
        {
            OutputMessage.gameObject.SetActive(true);
            OutputMessage.text = "Senhas distintas";
            return false;
        }

        if (!IsValidPassword(password))
        {
            OutputMessage.gameObject.SetActive(true);
            OutputMessage.text = "Senha inválida! Deve conter:\n- Pelo menos 8 caracteres\n- Uma letra maiúscula\n- Uma letra minúscula\n- Um número\n- Um caractere especial";
            return false;
        }

        if (name_.Length < 6)
        {
            OutputMessage.gameObject.SetActive(true);
            OutputMessage.text = "O nome deve ter no mínimo 6 caracteres";
            return false;
        }

        if (!userAcceptedTerms)
        {
            OutputMessage.gameObject.SetActive(true);
            OutputMessage.text = "É necessário aceitar os termos para continuar";
            return false;
        }

        return true;
    }

    private bool IsValidPassword(string password)
    {
        if (password.Length < 8) return false;
        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Z]")) return false; // maiúscula
        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-z]")) return false; // minúscula
        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[0-9]")) return false; // número
        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[\W_]")) return false; // especial
        return true;
    }


    // -------------------------------
    // Coroutine para criar usuário no Firebase
    // -------------------------------
    private IEnumerator CreateUserCoroutine()
    {
        string email = InputEmail.text.Trim();
        string password = InputPassword.text.Trim();

        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => registerTask.IsCompleted);

        if (registerTask.Exception != null)
        {
            FirebaseException firebaseEx = registerTask.Exception.Flatten().InnerExceptions[0] as FirebaseException;
            if (firebaseEx != null)
            {
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                switch (errorCode)
                {
                    case AuthError.EmailAlreadyInUse:
                        OutputMessage.text = "Este email já está em uso!";
                        break;
                    case AuthError.InvalidEmail:
                        OutputMessage.text = "Email inválido!";
                        break;
                    case AuthError.WeakPassword:
                        OutputMessage.text = "Senha fraca! Use pelo menos 6 caracteres.";
                        break;
                    default:
                        OutputMessage.text = "Erro no registro: " + firebaseEx.Message;
                        break;
                }
            }
            Debug.LogError("Erro no registro: " + registerTask.Exception);
            yield break;
        }

        // -------------------------------
        // Registro bem-sucedido
        // -------------------------------
        FirebaseUser newUser = registerTask.Result.User;

        if (newUser == null)
        {
            OutputMessage.text = "Erro inesperado: usuário não foi criado.";
            yield break;
        }

        // Debug para garantir que está logado
        Debug.Log($"Usuário logado: {newUser.Email}, UID: {newUser.UserId}");

        // Login automático já ocorreu aqui, podemos criar o documento
        UserDataManager.userInstance.CreateDefaultUser(InputName.text);

        Debug.Log("Registro REALIZADO");
        OutputMessage.text = "Registro realizado com sucesso!";

    }

}
