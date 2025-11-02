using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Firestore;

public class FasesCategoriaUI : MonoBehaviour
{
    [Header("Prefab e Content")]
    public Categoria4FasesUI prefabCategoria; // prefab com 4 slots
    public Transform contentParent;           // Content do ScrollView

    private FirebaseFirestore db;

    [Header("ID do usuário atual")]
    public string userID;

    private async void Start()
    {
        userID = UserDataManager.userInstance.GetUserId();

        if (string.IsNullOrEmpty(userID))
        {
            Debug.LogError("UserID não definido!");
            return;
        }

        db = FirebaseFirestore.DefaultInstance;
        await CarregarTodasCategoriasN5();
    }

    private async Task CarregarTodasCategoriasN5()
    {
        

        // 1️⃣ Lê o documento do usuário para pegar fases completadas
        List<string> fasesCompletadas = new List<string>();
        DocumentSnapshot userDoc = await db.Collection("Users").Document(userID).GetSnapshotAsync();

        if (userDoc.Exists)
        {
            if (userDoc.ContainsField("fases"))
            {
                fasesCompletadas = userDoc.GetValue<List<string>>("fases");
                Debug.Log($"Fases completadas do usuário: {string.Join(", ", fasesCompletadas)}");
            }
        }
        else
        {
            Debug.LogWarning($"Documento do usuário {userID} não encontrado!");
        }

        // 2️⃣ Lê todas as categorias
        QuerySnapshot snapshot = await db.Collection("CATEGORIA")
            .OrderBy("id")
            .GetSnapshotAsync();

        foreach (var doc in snapshot.Documents)
        {
            string categoriaID = doc.Id;

            // Filtra apenas N5
            if (!categoriaID.StartsWith("N5")) continue;

            string categoriaTitulo = doc.ContainsField("titulo")
                ? doc.GetValue<string>("titulo")
                : categoriaID;

            // IDs das fases dessa categoria
            List<string> fasesID = doc.ContainsField("fases")
                ? doc.GetValue<List<string>>("fases")
                : new List<string>();

            // Lista final de FaseData
            List<FaseData> fases = new List<FaseData>();

            foreach (string faseID in fasesID)
            {
                DocumentSnapshot faseDoc = await db.Collection("FASES").Document(faseID).GetSnapshotAsync();
                string titulo = faseDoc.ContainsField("titulo")
                    ? faseDoc.GetValue<string>("titulo")
                    : $"Fase {faseID}";

                string descricao = faseDoc.ContainsField("descricao")
                    ? faseDoc.GetValue<string>("descricao")
                    : $"Descrição da fase {faseID}";

                // Cria FaseData
                fases.Add(new FaseData(faseID, titulo, descricao));
            }

            // 3️⃣ Instancia prefab e passa fases completadas
            var categoriaUI = Instantiate(prefabCategoria, contentParent, false);

            // Agora o Setup precisa aceitar também a lista de fases completadas
            categoriaUI.Setup(fases.ToArray(), categoriaID, categoriaTitulo, fasesCompletadas);
        }
    }
}
