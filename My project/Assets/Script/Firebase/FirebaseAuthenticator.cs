using UnityEngine;
using Firebase;
using Firebase.Auth;
using System.Collections; // Adicionado para usar Coroutine

public class FirebaseAuthenticator : MonoBehaviour
{
    // o Padrão Singleton
    // a 'instância' estática é a chave para acessar este script de qualquer outro lugar
    public static FirebaseAuthenticator Instance { get; private set; }

    // variáveis do Firebase
    // referências para os serviços de Autenticação e para o Usuário logado
    public FirebaseAuth auth;
    public FirebaseUser user;

    private void Awake()
    {
        // configuração do Singleton: garante que exista apenas UMA instância deste script
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // faz este objeto persistir entre as cenas
        }
        else
        {
            Destroy(gameObject); // se uma instância já existe, destrói esta duplicata
        }
    }

    private void Start()
    {
        // inicia o processo de verificação e inicialização do Firebase
        StartCoroutine(CheckAndFixDependencies());
    }

    private IEnumerator CheckAndFixDependencies()
    {
        var checkAndFixTask = FirebaseApp.CheckAndFixDependenciesAsync();
        
        yield return new WaitUntil(() => checkAndFixTask.IsCompleted);

        var dependencyStatus = checkAndFixTask.Result;
        if (dependencyStatus == DependencyStatus.Available)
        {
            // se as dependências estiverem corretas, inicializa o Firebase Auth
            InitializeFirebase();
        }
        else
        {
            Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
        }
    }

    void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        Debug.Log("Firebase Auth inicializado com sucesso.");
    }
}