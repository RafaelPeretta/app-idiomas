using UnityEngine;
using Firebase.Auth;
using UnityEngine.SceneManagement;

public class LogoutManager : MonoBehaviour
{
    private FirebaseAuth auth;

    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
    }

    /// <summary>
    /// Executa logout do usuário, limpa dados locais e retorna à tela de login.
    /// </summary>
    public void Logout()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogWarning("[LogoutManager] Nenhum usuário logado para fazer logout.");
            return;
        }

        Debug.Log("[LogoutManager] Fazendo logout do usuário: " + auth.CurrentUser.UserId);

        // 1. Logout no Firebase
        auth.SignOut();

        // 2. Limpa dados locais
        if (UserDataManager.userInstance != null)
        {
            UserDataManager.userInstance.currentUserData = null;
        }

        // 3. (Opcional) Voltar para a cena de login
        // Coloque aqui o nome da sua cena de login
        SceneManager.LoadScene("Auth Beta");

        Debug.Log("[LogoutManager] Logout concluído.");
    }
}
