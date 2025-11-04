using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressBarManager : MonoBehaviour
{
    public Slider barraProgresso;
    public TMP_Text progressoPercentText; // arraste o TMP_Text pelo Inspector
    public PhaseUIManager phaseUI;         // arraste no Inspector

    public void AtualizarProgress()
    {
        if (phaseUI == null) return;
        if (PhaseManager.Instance == null || PhaseManager.Instance.currentPhase == null) return;

        var fase = PhaseManager.Instance.currentPhase;
        if (fase.diagnostica_6ano == null || fase.diagnostica_6ano.Count == 0) return;

        // Progresso considerando o índice atual da questão
        float progresso = (float)(phaseUI.currentID) / fase.diagnostica_6ano.Count;
        barraProgresso.value = progresso;

        int percent = Mathf.RoundToInt(progresso * 100);
        if (progressoPercentText != null)
            progressoPercentText.text = $"Progresso: {percent}%";
    }
}
