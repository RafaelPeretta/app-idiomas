using UnityEngine;
using System.Collections.Generic;

// Mantém a trilha carregada entre cenas
public class TrilhaManager : MonoBehaviour
{
    public static TrilhaManager Instance;

    public TrilhaDataLoad currentTrilha;

    private void Awake()
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
}

// Estrutura para guardar a trilha
[System.Serializable]
public class TrilhaDataLoad
{
    public string id;
    public List<QuestionData> questoes;
}
