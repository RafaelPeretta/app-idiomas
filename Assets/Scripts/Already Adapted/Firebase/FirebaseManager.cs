using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;

// =======================
// FirebaseManager
// =======================
public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    public bool IsReady { get; private set; } = false;
    public FirebaseFirestore DB { get; private set; }
    public FirebaseAuth Auth { get; private set; }

    public event Action OnFirebaseReady;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                DB = FirebaseFirestore.DefaultInstance;
                Auth = FirebaseAuth.DefaultInstance;
                IsReady = true;
                Debug.Log("[FirebaseManager] Firebase e Firestore inicializados.");
                OnFirebaseReady?.Invoke();
            }
            else
            {
                Debug.LogError("[FirebaseManager] Firebase não disponível: " + task.Result);
            }
        });
    }
}