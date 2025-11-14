using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;

public class TrilhaHabilidadesLoader : MonoBehaviour
{
    [Header("Referências de UI")]
    public GameObject botaoPrefab;
    public Transform containerBotoes;

    [Header("Referência do contador")]
    public TMP_Text contadorTMP;

    [Header("Tela secundária 2")]
    public TelaSecundaria2Controller telaSecundaria2Controller;

    private FirebaseFirestore db;

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        BuscarTrilhasDoFirestore();
    }

    public void BuscarTrilhasDoFirestore()
    {
        if (botaoPrefab == null || containerBotoes == null)
        {
            Debug.LogError("[TrilhaHabilidadesLoader] Prefab ou container não atribuídos!");
            return;
        }

        DocumentReference trilhasRef = db.Collection("referencias").Document("Trilhas");
        trilhasRef.GetSnapshotAsync().ContinueWithOnMainThread(trilhaTask =>
        {
            if (trilhaTask.IsFaulted)
            {
                Debug.LogError($"Erro ao buscar trilhas: {trilhaTask.Exception}");
                return;
            }

            DocumentSnapshot trilhaSnap = trilhaTask.Result;
            if (!trilhaSnap.Exists)
            {
                Debug.LogWarning("[TrilhaHabilidadesLoader] Documento 'Trilhas' não encontrado.");
                return;
            }

            List<object> listaTrilhas = trilhaSnap.GetValue<List<object>>("Lista");
            if (listaTrilhas == null || listaTrilhas.Count == 0)
            {
                Debug.LogWarning("[TrilhaHabilidadesLoader] Nenhuma trilha encontrada.");
                return;
            }

            List<Dictionary<string, object>> trilhas = new List<Dictionary<string, object>>();
            foreach (var item in listaTrilhas)
            {
                if (item is Dictionary<string, object> dict)
                    trilhas.Add(dict);
            }

            CriarBotoes(trilhas);
        });
    }

    private void CriarBotoes(List<Dictionary<string, object>> trilhas)
    {
        foreach (Transform filho in containerBotoes)
            Destroy(filho.gameObject);

        // Ordena por número do ID (TRILHAXXX)
        trilhas.Sort((a, b) =>
        {
            int numA = 0, numB = 0;
            if (a.ContainsKey("ID")) int.TryParse(a["ID"].ToString().Substring(6), out numA);
            if (b.ContainsKey("ID")) int.TryParse(b["ID"].ToString().Substring(6), out numB);
            return numA.CompareTo(numB);
        });

        string userId = UserDataManager.userInstance.GetUserId();
        List<string> trilhasUsuario = new List<string>();

        if (!string.IsNullOrEmpty(userId))
        {
            DocumentReference userRef = db.Collection("Users").Document(userId);
            userRef.GetSnapshotAsync().ContinueWithOnMainThread(userTask =>
            {
                if (!userTask.IsFaulted && userTask.Result.Exists && userTask.Result.ContainsField("trilhas"))
                {
                    var lista = userTask.Result.GetValue<List<object>>("trilhas");
                    foreach (var t in lista)
                        trilhasUsuario.Add(t.ToString());
                }

                bool proximaEncontrada = false;
                int concluidas = 0;

                foreach (var trilha in trilhas)
                {
                    string id = trilha.ContainsKey("ID") ? trilha["ID"].ToString() : "(null)";
                    string nome = trilha.ContainsKey("Nome") ? trilha["Nome"].ToString() : "(null)";

                    GameObject botao = Instantiate(botaoPrefab, containerBotoes);
                    botao.SetActive(true);

                    TMP_Text tmp = botao.GetComponentInChildren<TMP_Text>(true);
                    if (tmp != null)
                        tmp.text = $"{nome}";
                    else
                    {
                        Text txt = botao.GetComponentInChildren<Text>(true);
                        if (txt != null)
                            txt.text = $"{nome}";
                    }

                    Transform checkTransform = botao.transform.Find("Check");
                    if (checkTransform != null && checkTransform.TryGetComponent(out Image checkImage))
                    {
                        if (trilhasUsuario.Contains(id))
                        {
                            checkImage.color = Color.green;
                            concluidas++;
                        }
                        else if (!proximaEncontrada)
                        {
                            checkImage.color = new Color(1f, 0.65f, 0f); // laranja
                            proximaEncontrada = true;
                        }
                        else
                        {
                            checkImage.color = Color.red;
                        }
                    }

                    // Adiciona listener para abrir a tela secundária 2 com o ID da trilha
                    if (telaSecundaria2Controller != null)
                    {
                        botao.GetComponent<Button>().onClick.AddListener(() =>
                        {
                            telaSecundaria2Controller.AbrirComTrilha(id);
                        });
                    }
                }

                if (contadorTMP != null)
                    contadorTMP.text = $"{concluidas} / {trilhas.Count}";

                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(containerBotoes as RectTransform);
                Debug.Log($"[TrilhaHabilidadesLoader] Total de botões criados: {containerBotoes.childCount}");
            });
        }
    }
}
