using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class FasePanelController : MonoBehaviour
{
    public static FasePanelController FaseInstance;

    [Header("Referências de UI")]
    public TMP_Text FaseTitulo;
    public TMP_Text FaseDescricao;
    private string faseID;

    private void Awake()
    {
        // Garante que só exista uma instância
        if (FaseInstance != null && FaseInstance != this)
        {
            Destroy(gameObject);
            return;
        }

        FaseInstance = this;

        // Inicialmente o painel começa invisível
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;

        // Clique do mouse
        if (Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverUIObject())
            {
                Hide();
            }
        }

        // Toque na tela
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (!IsPointerOverUIObject())
            {
                Hide();
            }
        }

        // Scroll do mouse
        if (Input.mouseScrollDelta.y != 0)
        {
            Hide();
        }

        // Movimento de arrasto no touch (scroll mobile)
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            Hide();
        }
    }


    // Verifica se o ponteiro está sobre algum UI
    private bool IsPointerOverUIObject()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    // Mostra o painel da fase com os dados informados.
    public void Show(string faseID_, string titulo, string descricao, Vector3 posicaoDoBotao)
    {
        faseID = faseID_;
        FaseTitulo.text = titulo;
        FaseDescricao.text = descricao;

        float posX = posicaoDoBotao.x;
        float offsetY = 50f;
        float posY = posicaoDoBotao.y - offsetY;

        transform.position = new Vector3(posX, posY, posicaoDoBotao.z);

        // Garante que o painel fique na frente
        transform.SetAsLastSibling();

        gameObject.SetActive(true);
    }

    // Esconde o painel.
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
