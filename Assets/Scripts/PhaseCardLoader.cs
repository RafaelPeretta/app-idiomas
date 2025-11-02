using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI; // Necessário para acessar Image

public class PhaseCardLoader : MonoBehaviour
{
    [Header("Referências UI")]
    public Transform contentParent;       // Onde os cards serão instanciados
    public GameObject phaseCardPrefab;    // Prefab do card (deve ter TMP_Text)
    public TMP_Text topicoTMP;            // TMP que exibirá o nome do tópico selecionado

    [Header("Cores")]
    public Color faseCompletaColor = Color.green;
    public Color faseIncompletaColor = Color.white;

    /// <summary>
    /// Chamado pelo botão. Recebe o nome do tópico e carrega as fases correspondentes.
    /// </summary>
    public void LoadSelectedTopic(string topico)
    {
        if (UserDataManager.userInstance == null || LevelDataManager.Instance == null)
        {
            Debug.LogWarning("UserDataManager ou LevelDataManager não inicializados.");
            return;
        }

        // Atualiza o TMP com o nome do tópico
        if (topicoTMP != null)
            topicoTMP.text = topico;

        LevelData levelData = LevelDataManager.Instance.CurrentLevelData;
        if (levelData == null)
        {
            Debug.LogWarning("LevelData não carregado ainda.");
            return;
        }

        List<PhaseData> fases = topico switch
        {
            "LEITURA" => levelData.LEITURA,
            "VOCABULARIO" => levelData.VOCABULARIO,
            "KANJI" => levelData.KANJI,
            "ESCUTA" => levelData.ESCUTA,
            "GRAMATICA" => levelData.GRAMATICA,
            "SIMULADO" => levelData.SIMULADO,
            _ => null
        };

        if (fases == null)
        {
            Debug.LogWarning("Tópico inválido ou sem fases: " + topico);
            return;
        }

        LoadPhaseCards(fases);
    }

    /// <summary>
    /// Instancia os cards para a lista de fases fornecida
    /// </summary>
    private void LoadPhaseCards(List<PhaseData> phases)
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        // var completedLessons = UserDataManager.userInstance.currentUserData.completedLessons;

        foreach (var phase in phases)
        {
            GameObject card = Instantiate(phaseCardPrefab, contentParent);

            // Define o texto do card
            TMP_Text text = card.GetComponentInChildren<TMP_Text>();
            if (text != null) text.text = phase.nome;

            // Define a cor do background
            Image bg = card.GetComponent<Image>();
           /*
           if (bg != null)
            {
                if (completedLessons != null && completedLessons.Contains(phase.id))
                    bg.color = faseCompletaColor;
                else
                    bg.color = faseIncompletaColor;
            }

            // Adiciona componente PhaseCard para armazenar o ID
            PhaseCard cardData = card.AddComponent<PhaseCard>();
            cardData.faseID = phase.id;
           */
        }



        Debug.Log($"Carregadas {phases.Count} fases com cores de realização.");
    }
}
