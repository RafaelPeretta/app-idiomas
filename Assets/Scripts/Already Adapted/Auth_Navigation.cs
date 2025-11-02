using UnityEngine;
using UnityEngine.UI;

public class AuthNavitagionManage : MonoBehaviour
{
    public GameObject loginScreen;
    public GameObject registerScreen;
    private GameObject actualScreen;

    public static AuthNavitagionManage AuthManager;

    private void Awake()
    {
        AuthManager = this;
    }

    void Start()
    {
        registerScreen.SetActive(false);
        loginScreen.SetActive(false);

        actualScreen = loginScreen;
        actualScreen.SetActive(true);
    }

    public void irParaTela(GameObject Screen)
    {
        actualScreen.SetActive(false);
        actualScreen = Screen;
        actualScreen.SetActive(true);
    }

}
