using UnityEngine;
using Firebase.Auth; // essencial para Autenticação
using Firebase;      // essencial para o Firebase em geral
using TMPro;         // para usar os campos de texto TextMeshPro
using System.Collections;
using UnityEngine.SceneManagement; // para mudar de tela após o cadastro

public class RegisterAuth : MonoBehaviour
{
    [Header("UI de Cadastro")]
    public TMP_InputField usernameInput; // campo para o nome do usuário
    public TMP_InputField emailInput;    // campo para o e-mail
    public TMP_InputField passwordInput; // campo para a senha

    [Header("Feedback para o Usuário")]
    public TMP_Text warningRegisterText; // texto para exibir erros (ex: "Senha fraca")
    public TMP_Text confirmRegisterText; // texto para exibir sucesso

    // esta função será chamada pelo seu botão "Cadastre-se"
    public void RegisterButton()
    {
        // inicia o processo de registro em uma rotina separada para não travar o app
        StartCoroutine(Register(usernameInput.text, emailInput.text, passwordInput.text));
    }

    // a lógica principal de registro
    private IEnumerator Register(string username, string email, string password)
    {
        // acessa nosso gerente geral do Firebase
        var auth = FirebaseAuthenticator.Instance.auth;

        // 1. CRIA O USUÁRIO COM EMAIL E SENHA
        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        
        // Espera a tarefa de registro ser completada
        yield return new WaitUntil(predicate: () => registerTask.IsCompleted);

        // 2. VERIFICA SE HOUVE ERROS no registro inicial
        if (registerTask.Exception != null)
        {
            // Se houve erro, formata e exibe a mensagem apropriada
            Debug.LogWarning($"Falha ao registrar tarefa com {registerTask.Exception}");
            FirebaseException firebaseEx = registerTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
            warningRegisterText.text = GetRegisterErrorMessage(errorCode);
        }
        else
        {
            // 3. SE O CADASTRO FOI BEM-SUCEDIDO, pega o novo usuário
            AuthResult result = registerTask.Result;
            FirebaseUser newUser = result.User;

            if (newUser != null)
            {
                // Agora, atualizamos o perfil dele com o nome de usuário
                UserProfile profile = new UserProfile { DisplayName = username };
                var profileTask = newUser.UpdateUserProfileAsync(profile);

                // Espera a tarefa de atualização do perfil ser completada
                yield return new WaitUntil(predicate: () => profileTask.IsCompleted);

                if (profileTask.Exception != null)
                {
                    // Se deu erro ao salvar o nome, avisa o usuário
                    Debug.LogWarning($"Falha ao atualizar perfil com {profileTask.Exception}");
                    warningRegisterText.text = "Falha ao salvar o nome de usuário.";
                }
                else
                {
                    // 4. TUDO CERTO! Cadastro e perfil atualizado com sucesso!
                    warningRegisterText.text = ""; // Limpa qualquer aviso de erro antigo
                    confirmRegisterText.text = "Usuário registrado com sucesso!";
                    Debug.Log("Cadastro e atualização de perfil concluídos!");

                    // Opcional: Redirecionar para a tela de login após um tempo
                    yield return new WaitForSeconds(2); // Espera 2 segundos
                    SceneManager.LoadScene("telaLogin"); // Mude "telaLogin" para o nome EXATO da sua cena de login
                }
            }
        }
    }

    // Função para "traduzir" os códigos de erro do Firebase para mensagens amigáveis
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