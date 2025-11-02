using System.Security.Policy;
using UnityEngine;

public class SecondaryMenu_Navigation : MonoBehaviour
{
    public GameObject[] secondaryScreens;

    public void Start()
    {
        foreach (var s in secondaryScreens)
            s.SetActive(false);
    }

    public void ActivateScreen(GameObject screen)
    {
        foreach (var s in secondaryScreens)
            s.SetActive(false);

        if (System.Array.Exists(secondaryScreens, s => s == screen))
            screen.SetActive(true);
        else
            Debug.LogWarning("Tela não encontrada no array de secondaryScreens");
    }


    public void DeactivateScreen(GameObject screen)
    {
        if (System.Array.Exists(secondaryScreens, s => s == screen))
            screen.SetActive(false);
    }

    public void DeactivateAllScreens()
    {
        foreach (var s in secondaryScreens)
            s.SetActive(false);
    }

}
