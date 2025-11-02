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

        var itens = PhaseManager.Instance.currentPhase.itens;
        if (itens == null || itens.Count == 0) return;

        // Agora o progresso só considera questões já concluídas
        float progresso = (float)(phaseUI.currentID) / itens.Count;
        barraProgresso.value = progresso;

        int percent = Mathf.RoundToInt(progresso * 100);
        if (progressoPercentText != null)
            progressoPercentText.text = "Progresso: " + percent + "%";
    }
}
