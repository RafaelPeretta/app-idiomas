using UnityEngine;
using UnityEngine.UI;

public class MenuNavigation : MonoBehaviour
{
    [Header("Botões do Menu")]
    public Button[] buttons; // 3 botões fixos
    public GameObject[] telas; // 3 telas correspondentes

    [Header("Escala do botão")]
    public float normalScale = 1f;
    public float selectedScale = 1.4f;

    [Header("Swipe Config")]
    public float minSwipeDistance = 50f;

    private int currentScreenIndex = 1; // 1 é a central (principal)

    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private bool isSwiping = false;

    private void Start()
    {
        UpdateScreens();
        UpdateButtonScales();

        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(() => OnButtonClicked(index));
        }
    }

    private void Update()
    {
        DetectTouch();
        DetectMouse();
    }

    void DetectTouch()
    {
        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                startTouchPosition = touch.position;
                isSwiping = true;
                break;

            case TouchPhase.Ended:
                if (!isSwiping) return;
                endTouchPosition = touch.position;
                DetectSwipe();
                isSwiping = false;
                break;
        }
    }

    void DetectMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPosition = Input.mousePosition;
            isSwiping = true;
        }

        if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            endTouchPosition = Input.mousePosition;
            DetectSwipe();
            isSwiping = false;
        }
    }

    void DetectSwipe()
    {
        float distanceX = endTouchPosition.x - startTouchPosition.x;

        if (Mathf.Abs(distanceX) < minSwipeDistance) return;

        if (distanceX > 0)
            SwipeRight();
        else
            SwipeLeft();
    }

    public void OnButtonClicked(int index)
    {
        currentScreenIndex = index;
        UpdateScreens();
        UpdateButtonScales();
    }

    public void SwipeLeft()
    {
        if (currentScreenIndex < telas.Length - 1) // 0..2
        {
            currentScreenIndex++;
            UpdateScreens();
            UpdateButtonScales();
        }
    }

    public void SwipeRight()
    {
        if (currentScreenIndex > 0)
        {
            currentScreenIndex--;
            UpdateScreens();
            UpdateButtonScales();
        }
    }

    void UpdateScreens()
    {
        for (int i = 0; i < telas.Length; i++)
        {
            telas[i].SetActive(i == currentScreenIndex);
        }
    }

    void UpdateButtonScales()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].transform.localScale =
                (i == currentScreenIndex) ? Vector3.one * selectedScale : Vector3.one * normalScale;
        }
    }
}
