using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class NavigationManager : MonoBehaviour
{
    [Header("Painéis de Conteúdo")]
    public GameObject painelHome;
    public GameObject painelPerfil;

    [Header("UI do Perfil - Conecte no Inspetor")]
    public TMP_Text nomeUsuarioText;
    public TMP_Text emailUsuarioText;
    public TMP_Text membroDesdeText;

    void Start()
    {
        AbrirPainelHome();
    }

    private void FecharTodosOsPaineis()
    {
        painelHome.SetActive(false);
        painelPerfil.SetActive(false);
    }

    public void AbrirPainelHome()
    {
        FecharTodosOsPaineis();
        painelHome.SetActive(true);
    }

    public void AbrirPainelPerfil()
    {
        FecharTodosOsPaineis();
        painelPerfil.SetActive(true);
        PreencherInformacoesDoPerfil();
    }

    private void PreencherInformacoesDoPerfil()
    {
        // --- CORREÇÃO FINAL APLICADA AQUI ---
        // Em vez de usar 'FirebaseAuthenticator.Instance.user',
        // usamos 'FirebaseAuthenticator.Instance.auth.CurrentUser'.
        // Esta é a fonte oficial e sempre atualizada de quem está logado.
        var user = FirebaseAuthenticator.Instance.auth.CurrentUser;

        if (user != null)
        {
            string nome = user.DisplayName;
            string email = user.Email;
            long timestamp = (long)user.Metadata.CreationTimestamp;
            DateTime dataDeCriacao = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
            string dataFormatada = dataDeCriacao.ToString("dd/MM/yyyy");

            nomeUsuarioText.text = nome;
            emailUsuarioText.text = email;
            membroDesdeText.text = "Membro desde: " + dataFormatada;
        }
        else
        {
            nomeUsuarioText.text = "Usuário não encontrado";
            emailUsuarioText.text = "";
            membroDesdeText.text = "";
        }
    }

    public void LogoutButton()
    {
        FirebaseAuthenticator.Instance.auth.SignOut();
        SceneManager.LoadScene("telaLogin");
    }
}