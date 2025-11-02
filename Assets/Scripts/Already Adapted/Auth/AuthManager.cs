using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AuthManager : MonoBehaviour
{
    public FirebaseAuth Auth { get; private set; }

    public static AuthManager AuthInstance;

    void Awake()
    {
        if (AuthInstance == null)
        {
            AuthInstance = this;
            Auth = FirebaseAuth.DefaultInstance;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
