using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// alias pra evitar ambiguidade entre System.Object e UnityEngine.Object
using Object = UnityEngine.Object;

[System.Serializable]
public class UserData
{
    public string username;
    public string bio;
    public List<string> trilhas;
    public List<string> habilidades;
    public int pontos;
    public string userId;
    public Timestamp createdAt;
    public Timestamp? lastAction;
    public int vida;
}

public class UserDataManager : MonoBehaviour
{

    public static UserDataManager userInstance;
    private FirebaseFirestore DatabaseFirestore;

    public string faseSelecionada;
    public UserData currentUserData;

    // Evento disparado quando o usuário é carregado
    public event Action OnUserLoaded;

    private LevelDataManager levelDataManager;

    public TMPro.TextMeshPro hpText;

    private void Awake()
    {
        if (userInstance != null && userInstance != this)
        {
            Destroy(gameObject);
            return;
        }

        userInstance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                DatabaseFirestore = FirebaseFirestore.DefaultInstance;
                Debug.Log("[UserDataManager] Firestore inicializado.");
            }
            else
            {
                Debug.LogError("[UserDataManager] Erro ao inicializar Firebase: " + task.Result);
            }
        });

    }

    public void CreateDefaultUser(string name_)
    {
        string userId = GetUserId();
        Debug.Log("[UserDataManager] Usuário logado: " + userId);

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("[UserDataManager] Usuário não logado.");
            return;
        }

        DocumentReference docRef = DatabaseFirestore.Collection("Users").Document(userId);

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsCompleted) return;

            DocumentSnapshot snapshot = task.Result;
            if (!snapshot.Exists)
            {
                Dictionary<string, object> defaultData = new Dictionary<string, object>
                {
                    { "userId", userId },
                    { "username", name_ },
                    { "bio", "Sem bio ainda" },
                    { "trilhas", new List<string>() },
                    { "habilidades", new List<string>() },
                    { "createdAt", Timestamp.GetCurrentTimestamp() },
                    { "pontos", 0 },
                };

                docRef.SetAsync(defaultData).ContinueWithOnMainThread(saveTask =>
                {
                    if (saveTask.IsCompleted)
                    {
                        Debug.Log("[UserDataManager] Usuário padrão criado!");
                        LoadUser();
                        GameManager.Instance.irParaCena("Menu");
                    }
                    else
                    {
                        Debug.LogError("[UserDataManager] Erro ao criar usuário: " + saveTask.Exception);
                    }
                });
            }
            else
            {
                Debug.Log("[UserDataManager] Usuário já existe.");
                LoadUser();
            }
        });
    }

    public void LoadUser()
    {
        string userId = GetUserId();
        Debug.Log("[UserDataManager] Carregando usuário: " + userId);

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("[UserDataManager] Usuário não logado.");
            return;
        }

        DocumentReference docRef = DatabaseFirestore.Collection("Users").Document(userId);

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsCompleted) return;

            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                Dictionary<string, object> userData = snapshot.ToDictionary();

                currentUserData = new UserData
                {
                    userId = userId,
                    username = userData.TryGetValue("username", out var u) ? u.ToString() : "Jogador Desconhecido",
                    bio = userData.TryGetValue("bio", out var b) ? b.ToString() : "Sem bio",
                    trilhas = userData.TryGetValue("trilhas", out var cl) && cl is List<object> listObj ? listObj.ConvertAll(x => x.ToString()) : new List<string>(),
                    habilidades = userData.TryGetValue("habilidades", out var cl1) && cl is List<object> listObj1 ? listObj1.ConvertAll(x => x.ToString()) : new List<string>(),
                    pontos = userData.TryGetValue("pontos", out var d) ? Convert.ToInt32(d) : 0,
                    createdAt = userData.TryGetValue("createdAt", out var ca) && ca is Timestamp ts ? ts : Timestamp.GetCurrentTimestamp(),
                };

                Debug.Log($"[UserDataManager] Usuário carregado: {currentUserData.username}, Pontos: {currentUserData.pontos}, Criado em: {currentUserData.createdAt}");

                OnUserLoaded?.Invoke();
            }
            else
            {
                Debug.LogWarning("[UserDataManager] Documento do usuário não encontrado.");
            }
        });
    }


    public string GetUserId()
    {
        return AuthManager.AuthInstance.Auth.CurrentUser?.UserId;
    }
}
