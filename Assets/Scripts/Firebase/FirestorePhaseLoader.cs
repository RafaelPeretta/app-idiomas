using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Representa cada item da fase
[System.Serializable]
public class PhaseItem
{
    public int id;
    public string tipo;
    public string conteudo;
    public string pergunta;
    public List<string> opcoes;
    public string resposta;
}

// Representa a fase completa
[System.Serializable]
public class PhaseDataLoad
{
    public int fase;
    public string titulo;
    public string topico;
    public string descricao;
    public string faseID;
    public List<PhaseItem> itens;
}

public class FirestorePhaseLoader : MonoBehaviour
{
    public static FirestorePhaseLoader Instance;

    private FirebaseFirestore db;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            db = FirebaseFirestore.DefaultInstance;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[FirestorePhaseLoader] Instância criada e DB inicializado.");
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("[FirestorePhaseLoader] Instância duplicada destruída.");
        }
    }

    /// <summary>
    /// Carrega uma fase completa pelo seu ID, com todos os itens da lista
    /// </summary>
    public void LoadPhaseByID(string faseID, System.Action<PhaseDataLoad> onComplete)
    {
        Debug.Log($"[FirestorePhaseLoader] CARREGANDO FASE: {faseID}");

        if (db == null)
        {
            Debug.LogError("[FirestorePhaseLoader] DB Firebase não inicializado!");
            onComplete?.Invoke(null);
            return;
        }

        db.Collection("fases").Document(faseID).GetSnapshotAsync()
          .ContinueWithOnMainThread(task =>
          {
              if (task.IsFaulted)
              {
                  Debug.LogError($"[FirestorePhaseLoader] Erro ao carregar fase {faseID}: {task.Exception}");
                  onComplete?.Invoke(null);
                  return;
              }

              DocumentSnapshot doc = task.Result;
              if (!doc.Exists)
              {
                  Debug.LogWarning($"[FirestorePhaseLoader] Fase não encontrada no Firestore: {faseID}");
                  onComplete?.Invoke(null);
                  return;
              }

              Debug.Log($"[FirestorePhaseLoader] Documento encontrado: {doc.Id}");

              Dictionary<string, object> data = null;
              try
              {
                  data = doc.ToDictionary();
                  Debug.Log("[FirestorePhaseLoader] Documento convertido para Dictionary<string, object> com sucesso.");
              }
              catch (System.Exception ex)
              {
                  Debug.LogError($"[FirestorePhaseLoader] Falha ao converter documento para Dictionary: {ex}");
                  onComplete?.Invoke(null);
                  return;
              }

              PhaseDataLoad phase = new PhaseDataLoad
              {
                  fase = data.ContainsKey("fase") ? System.Convert.ToInt32(data["fase"]) : 0,
                  titulo = data.ContainsKey("titulo") ? data["titulo"]?.ToString() ?? "SEM TÍTULO" : "SEM TÍTULO",
                  topico = data.ContainsKey("topico") ? data["topico"]?.ToString() ?? "SEM TÓPICO" : "SEM TÓPICO",
                  descricao = data.ContainsKey("descricao") ? data["descricao"]?.ToString() ?? "SEM DESCRIÇÃO" : "SEM DESCRIÇÃO",
                  faseID = data.ContainsKey("faseID") ? data["faseID"]?.ToString() ?? faseID : faseID,
                  itens = new List<PhaseItem>()
              };

              Debug.Log($"[FirestorePhaseLoader] Título da fase: {phase.titulo}");
              Debug.Log($"[FirestorePhaseLoader] Descrição da fase: {phase.descricao}");

              if (!data.ContainsKey("itens"))
              {
                  Debug.LogWarning("[FirestorePhaseLoader] Não há itens no documento da fase.");
                  onComplete?.Invoke(phase);
                  return;
              }

              var rawItens = data["itens"];
              Debug.Log($"[FirestorePhaseLoader] rawItens tipo: {rawItens?.GetType().Name ?? "null"}");

              if (!(rawItens is IEnumerable<object> itensEnumerable))
              {
                  Debug.LogWarning("[FirestorePhaseLoader] rawItens não é IEnumerable<object> e não pode ser iterado.");
                  onComplete?.Invoke(phase);
                  return;
              }

              int index = 0;
              foreach (var obj in itensEnumerable)
              {
                  index++;
                  if (obj == null)
                  {
                      Debug.LogWarning($"[FirestorePhaseLoader] Item {index} é null, ignorando.");
                      continue;
                  }

                  Dictionary<string, object> itemDict = null;

                  if (obj is Dictionary<string, object> dict)
                  {
                      itemDict = dict;
                  }
                  else if (obj.GetType().Name == "MapField")
                  {
                      Debug.Log($"[FirestorePhaseLoader] Item {index} é MapField. Listando propriedades...");
                      itemDict = new Dictionary<string, object>();
                      foreach (var prop in obj.GetType().GetProperties())
                      {
                          Debug.Log($"[FirestorePhaseLoader] MapField property: {prop.Name}");
                      }
                  }
                  else
                  {
                      Debug.LogWarning($"[FirestorePhaseLoader] Item {index} não é Dictionary nem MapField. Tipo: {obj.GetType().Name}");
                      continue;
                  }

                  if (itemDict != null)
                  {
                      try
                      {
                          List<string> opcoesList = new List<string>();
                          if (itemDict.ContainsKey("opcoes") && itemDict["opcoes"] is IEnumerable<object> opts)
                          {
                              opcoesList = opts.Cast<string>().ToList();
                          }

                          PhaseItem item = new PhaseItem
                          {
                              id = itemDict.ContainsKey("id") ? System.Convert.ToInt32(itemDict["id"]) : 0,
                              tipo = itemDict.ContainsKey("tipo") ? itemDict["tipo"]?.ToString() ?? "SEM TIPO" : "SEM TIPO",
                              conteudo = itemDict.ContainsKey("conteudo") ? itemDict["conteudo"]?.ToString() ?? "" : "",
                              pergunta = itemDict.ContainsKey("pergunta") ? itemDict["pergunta"]?.ToString() ?? "" : "",
                              opcoes = opcoesList,
                              resposta = itemDict.ContainsKey("resposta") ? itemDict["resposta"]?.ToString() ?? "" : ""
                          };

                          phase.itens.Add(item);
                          Debug.Log($"[FirestorePhaseLoader] ✅ Item {index} carregado: ID {item.id}, Tipo {item.tipo}");
                      }
                      catch (System.Exception ex)
                      {
                          Debug.LogError($"[FirestorePhaseLoader] Falha ao processar item {index}: {ex}");
                      }
                  }
              }

              Debug.Log($"[FirestorePhaseLoader] ✅ Todos os {phase.itens.Count} itens carregados com sucesso. Chamando callback.");
              onComplete?.Invoke(phase);
          });
    }
}
