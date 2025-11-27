using UnityEngine;

public class FirstLogin : MonoBehaviour
{
    public static FirstLogin Instance;
    private void Awake()
    {
        Instance = this;
    }

    public void Avaliacao()
    {
        FirestorePhaseLoader.Instance.LoadPhase("diagnostica_6ano");
        GameManager.Instance.irParaCena("QuestionList");
    }

}
