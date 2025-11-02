using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AuthLogin : MonoBehaviour
{
    public TMP_InputField input_email;
    public TMP_InputField input_password;
    FirebaseAuth auth;

    public FirebaseUser user;

    public TextMeshProUGUI outputMessage;

    public string afterLoginScene;

    void Start()
    {
        if (AuthManager.AuthInstance == null)
        {
            Debug.LogError("AuthManager não encontrado!");
            return;
        }

        auth = AuthManager.AuthInstance.Auth;

        outputMessage.gameObject.SetActive(false);

    }


    public void Login()
    {
        StartCoroutine(LoginUser());
    }

    private IEnumerator LoginUser()
    {
        if (string.IsNullOrEmpty(input_email.text) || string.IsNullOrEmpty(input_password.text))
        {
            outputMessage.gameObject.SetActive(true);
            outputMessage.text = "Preencha todos os campos antes de continuar.";
            yield break;
        }

        var loginTask = auth.SignInWithEmailAndPasswordAsync(input_email.text, input_password.text);

        // Espera a task terminar
        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            outputMessage.gameObject.SetActive(true);
            outputMessage.text = "E-mail ou senha incorretos, tente novamente.";
        }
        else
        {
            user = loginTask.Result.User;
            Debug.LogFormat("Login realizado com sucesso! Usuário: {0}", user.Email + "|  ID: " + user.UserId);

            UserDataManager.userInstance.LoadUser();
            GameManager.Instance.irParaCena(afterLoginScene);
        }
    }

}
