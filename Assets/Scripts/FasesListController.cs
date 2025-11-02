using UnityEngine;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;

public class FasesListController : MonoBehaviour
{
    public Transform contentParent; // objeto que vai conter os cards (ex: painel de ScrollView)
    public GameObject faseCardPrefab; // prefab do card

    FirebaseFirestore db;

    async void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        await CarregarFases();
    }

    async Task CarregarFases()
    {
        QuerySnapshot snapshot = await db.Collection("FASES")
                                         .OrderBy("faseID")
                                         .GetSnapshotAsync();

        float posY = 0f; // posição acumulada no eixo Y

        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            string faseID = doc.GetValue<string>("faseID");
            string nome = doc.GetValue<string>("nome");
            string descricao = doc.GetValue<string>("descricao");

            // Cria o card
            GameObject cardGO = Instantiate(faseCardPrefab, contentParent);
            RectTransform rect = cardGO.GetComponent<RectTransform>();

            // Mantém X fixo em 0, só organiza no Y
            rect.anchoredPosition = new Vector2(0f, -posY);

            // Atualiza acumulador Y (altura do card + 100 de espaço extra)
            posY += rect.sizeDelta.y + 100f;

            // Configura conteúdo do card
            FaseCardUI cardUI = cardGO.GetComponent<FaseCardUI>();
            cardUI.Setup(faseID, nome, descricao);
        }
    }
}
