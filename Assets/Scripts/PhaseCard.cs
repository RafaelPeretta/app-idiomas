using UnityEngine;
using UnityEngine.UI;

public class PhaseCard : MonoBehaviour
{
    public string faseID;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        joinLevel.Instance.levelSelected(faseID);
    }
}
