using UnityEngine;
using UnityEngine.SceneManagement;

public class joinLevel : MonoBehaviour
{
    public static joinLevel Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[joinLevel] Instância criada e não será destruída na troca de cena.");
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("[joinLevel] Instância duplicada destruída.");
        }
    }

    private PhaseDataLoad selectedPhase;
    private bool isLoadingPhase = false; // flag de carregamento

    /// <summary>
    /// Chamado pelo clique no card
    /// </summary>
    /// <param name="faseID"></param>
    public void levelSelected(string faseID)
    {
        if (isLoadingPhase)
        {
            Debug.Log("[joinLevel] Já está carregando uma fase. Aguarde.");
            return;
        }

        Debug.Log($"[joinLevel] Tentando carregar fase: {faseID}");
        isLoadingPhase = true;

        if (FirestorePhaseLoader.Instance == null)
        {
            Debug.LogError("[joinLevel] FirestorePhaseLoader.Instance não encontrado!");
            isLoadingPhase = false;
            return;
        }

        FirestorePhaseLoader.Instance.LoadPhaseByID(faseID, phaseData =>
        {
            isLoadingPhase = false; // libera flag

            if (phaseData == null)
            {
                Debug.LogError($"[joinLevel] Não foi possível carregar a fase {faseID} do Firestore.");
                return;
            }

            Debug.Log($"[joinLevel] Fase carregada com sucesso: {phaseData.titulo} ({phaseData.faseID})");
            selectedPhase = phaseData;

            if (PhaseManager.Instance != null)
            {
                PhaseManager.Instance.currentPhase = selectedPhase;
                Debug.Log("[joinLevel] Fase armazenada no PhaseManager.");
            }
            else
            {
                Debug.LogWarning("[joinLevel] PhaseManager.Instance não encontrado! Não será possível passar a fase para a cena.");
            }

            // Antes de trocar a cena, verificar se a cena existe
            if (Application.CanStreamedLevelBeLoaded("QuestionList"))
            {
                Debug.Log("[joinLevel] Mudando para cena: QuestionList");
                SceneManager.LoadScene("QuestionList");
            }
            else
            {
                Debug.LogError("[joinLevel] Cena 'QuestionList' não encontrada na Build Settings!");
            }
        });

        Debug.Log("[joinLevel] Chamado LoadPhaseByID, aguardando retorno...");
    }
}
