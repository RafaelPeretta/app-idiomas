using UnityEngine;

// Mantém a fase carregada entre cenas
public class PhaseManager : MonoBehaviour
{
    public static PhaseManager Instance;

    public PhaseDataLoad currentPhase;

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
