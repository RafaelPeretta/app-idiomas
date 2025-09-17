using UnityEngine;
using Firebase;
using Firebase.Auth;
using System.Collections;

public class FirebaseAuthenticator : MonoBehaviour
{
    public static FirebaseAuthenticator Instance { get; private set; }
    public FirebaseAuth auth; // A única variável que ele precisa de expor

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(CheckAndFixDependencies());
    }

    private IEnumerator CheckAndFixDependencies()
    {
        var checkAndFixTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => checkAndFixTask.IsCompleted);

        var dependencyStatus = checkAndFixTask.Result;
        if (dependencyStatus == DependencyStatus.Available)
        {
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