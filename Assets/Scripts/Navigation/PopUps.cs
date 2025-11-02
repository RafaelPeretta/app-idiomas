using UnityEngine;
using System.Collections;

public class PopUpsManager : MonoBehaviour
{
    public GameObject[] PopUps;

    private float delay = 3f;

    public void Start()
    {
        foreach (var s in PopUps)
            s.SetActive(false);
    }

    public void ActivatePopup(GameObject screen)
    {
        foreach (var s in PopUps)
            s.SetActive(false);

        if (System.Array.Exists(PopUps, s => s == screen))
            screen.SetActive(true);
        else
            Debug.LogWarning("Tela não encontrada no array de PopUps");
    }

    public void DeactivatePopup(GameObject screen)
    {
        if (System.Array.Exists(PopUps, s => s == screen))
            screen.SetActive(false);
    }

    public void DeactivateAllPopups()
    {
        foreach (var s in PopUps)
            s.SetActive(false);
    }

    public void DeactivatePopupWithDelay(GameObject screen)
    {
        StartCoroutine(DeactivateAfterDelay(screen)); //
    }

    private IEnumerator DeactivateAfterDelay(GameObject screen)
    {
        yield return new WaitForSeconds(delay);

        if (System.Array.Exists(PopUps, s => s == screen))
            screen.SetActive(false);
    }
}
