using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AuthLogout : MonoBehaviour
{
    public void Logout()
    {
        // Chama o auth central do AuthManager
        FirebaseAuth.DefaultInstance.SignOut();
        Debug.Log("Usuário desconectado.");

        // Redireciona para a cena de login
        SceneManager.LoadScene("Auth Beta");
    }
}
