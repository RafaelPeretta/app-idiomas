using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections.Generic;
using UnityEngine;
using System;


[System.Serializable]
public class PhaseData
{
    public string id;
    public string nome;
}

[System.Serializable]
public class LevelData
{
    public List<PhaseData> VOCABULARIO = new List<PhaseData>();
    public List<PhaseData> ESCUTA = new List<PhaseData>();
    public List<PhaseData> KANJI = new List<PhaseData>();
    public List<PhaseData> LEITURA = new List<PhaseData>();
    public List<PhaseData> GRAMATICA = new List<PhaseData>();
    public List<PhaseData> SIMULADO = new List<PhaseData>();
}

public class LevelDataManager : MonoBehaviour
{
    public static LevelDataManager Instance { get; private set; }

    private FirebaseFirestore db;
    public LevelData CurrentLevelData { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        db = FirebaseFirestore.DefaultInstance;
        Debug.Log("[LevelDataManager] Firestore instanciado.");
    }

   
}