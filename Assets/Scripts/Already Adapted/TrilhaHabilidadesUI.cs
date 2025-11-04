using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;

public class TrilhaHabilidadesLoader : MonoBehaviour
{
    [Header("Referências de UI")]
    [Tooltip("Prefab do botão que será instanciado para cada habilidade.")]
    public GameObject botaoPrefab;

    [Tooltip("Local onde os botões serão criados (geralmente o Content de um ScrollView).")]
    public Transform containerBotoes;

    private FirebaseFirestore db;

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    public void BuscarHabilidadesDoFirestore()
    {
        Debug.Log("[TrilhaHabilidadesLoader] Buscando habilidades da trilha do 6º ano...");

        if (botaoPrefab == null || containerBotoes == null)
        {
            Debug.LogError("[TrilhaHabilidadesLoader] Prefab ou container não atribuídos!");
            return;
        }

        DocumentReference trilhaRef = db.Collection("trilhas").Document("6ano");
        string userId = UserDataManager.userInstance.GetUserId();

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("[TrilhaHabilidadesLoader] Usuário não autenticado — habilidades do usuário não podem ser verificadas.");
            return;
        }

        DocumentReference userRef = db.Collection("Users").Document(userId);

        trilhaRef.GetSnapshotAsync().ContinueWithOnMainThread(trilhaTask =>
        {
            if (trilhaTask.IsFaulted)
            {
                Debug.LogError($"[TrilhaHabilidadesLoader] Erro ao buscar trilha: {trilhaTask.Exception}");
                return;
            }

            DocumentSnapshot trilhaSnap = trilhaTask.Result;
            if (!trilhaSnap.Exists)
            {
                Debug.LogWarning("[TrilhaHabilidadesLoader] Documento '6ano' não encontrado.");
                return;
            }

            List<object> habilidadesTrilha = trilhaSnap.GetValue<List<object>>("habilidades");
            if (habilidadesTrilha == null || habilidadesTrilha.Count == 0)
            {
                Debug.LogWarning("[TrilhaHabilidadesLoader] Nenhuma habilidade encontrada na trilha.");
                return;
            }

            userRef.GetSnapshotAsync().ContinueWithOnMainThread(userTask =>
            {
                if (userTask.IsFaulted)
                {
                    Debug.LogError($"[TrilhaHabilidadesLoader] Erro ao buscar dados do usuário: {userTask.Exception}");
                    CriarBotoes(habilidadesTrilha, new List<string>());
                    return;
                }

                List<string> habilidadesUsuario = new List<string>();
                DocumentSnapshot userSnap = userTask.Result;

                if (userSnap.Exists && userSnap.ContainsField("habilidades"))
                {
                    var lista = userSnap.GetValue<List<object>>("habilidades");
                    foreach (var h in lista)
                        habilidadesUsuario.Add(h.ToString());
                }

                Debug.Log($"[TrilhaHabilidadesLoader] {habilidadesUsuario.Count} habilidades do usuário encontradas.");
                CriarBotoes(habilidadesTrilha, habilidadesUsuario);
            });
        });
    }

    private void CriarBotoes(List<object> habilidades, List<string> habilidadesUsuario)
    {
        foreach (Transform filho in containerBotoes)
            Destroy(filho.gameObject);

        foreach (var habilidadeObj in habilidades)
        {
            string habilidade = habilidadeObj?.ToString() ?? "(null)";
            GameObject botao = Instantiate(botaoPrefab, containerBotoes);
            botao.SetActive(true);

            // Define texto (TMP ou Text)
            TMP_Text tmp = botao.GetComponentInChildren<TMP_Text>(true);
            if (tmp != null)
                tmp.text = habilidade;
            else
            {
                Text txt = botao.GetComponentInChildren<Text>(true);
                if (txt != null)
                    txt.text = habilidade;
            }

            // Verifica se o botão possui um filho chamado "Check"
            Transform checkTransform = botao.transform.Find("Check");
            if (checkTransform != null && checkTransform.TryGetComponent(out Image checkImage))
            {
                bool concluida = habilidadesUsuario.Contains(habilidade);
                checkImage.color = concluida ? Color.green : Color.red;
                Debug.Log($"[TrilhaHabilidadesLoader] Habilidade '{habilidade}' → {(concluida ? "verde" : "vermelha")}");
            }
            else
            {
                Debug.LogWarning($"[TrilhaHabilidadesLoader] O botão '{habilidade}' não possui filho chamado 'Check' com componente Image.");
            }
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(containerBotoes as RectTransform);

        Debug.Log($"[TrilhaHabilidadesLoader] Total de botões criados: {containerBotoes.childCount}");
    }
}
