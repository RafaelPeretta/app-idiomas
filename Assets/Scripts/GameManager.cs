using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static string LoggedUserID;

    public int faseQuestoes = 46; // QTD de questões que cada fase tem no banco de dados
    public int maxQuestoes = 10; // QTD de questões a serem respondidas a cada fase

    private FirebaseAuth auth;

    private void Awake()
    {
        // Se já existe uma instância e não é essa, destrói para evitar duplicatas
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Define a instância atual
        Instance = this;

        // Faz o GameObject persistir entre cenas
        DontDestroyOnLoad(gameObject);
    }


    // Função para navegar entre cenas
    public void irParaCena(string cena)
    {
        SceneManager.LoadScene(cena);
    }

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += OnAuthStateChanged;
    }

    private void OnAuthStateChanged(object sender, System.EventArgs e)
    {
        if (auth.CurrentUser == null)
        {
            // Usuário não está mais logado
            Debug.Log("Usuário deslogado, retornando para login");
            irParaCena("Auth Beta"); // ou a cena de login
        }
    }

    private void OnDestroy()
    {
        if (auth != null)
            auth.StateChanged -= OnAuthStateChanged;
    }

}
