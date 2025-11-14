using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HabilidadeFeedbackUI : MonoBehaviour
{
    [System.Serializable]
    public class HabilidadeResultado
    {
        public string habilidade;
        public bool correta;
    }

    public GameObject botaoPrefab;
    public Transform containerBotoes;

    public List<HabilidadeResultado> listaHabilidadesFinal;

    public void GerarFeedbackVisual(List<answerQuestion.RespostaUsuario> respostas)
    {
        // Agora habilidades são listas → precisamos expandir todas
        Dictionary<string, (int corretas, int total)> contagem = new Dictionary<string, (int, int)>();

        foreach (var r in respostas)
        {
            foreach (var habilidade in r.habilidades)
            {
                if (!contagem.ContainsKey(habilidade))
                    contagem[habilidade] = (0, 0);

                var atual = contagem[habilidade];

                bool estaCorreta = false;

                // Agora respostaCorreta é List<string>
                if (r.respostaCorreta != null && r.respostaCorreta.Count > 0)
                {
                    if (r.respostaUsuario.Contains("|"))
                    {
                        // resposta múltipla
                        string[] usuarioSplit = r.respostaUsuario.Split('|');
                        estaCorreta = usuarioSplit.Length == r.respostaCorreta.Count;

                        for (int i = 0; i < usuarioSplit.Length && i < r.respostaCorreta.Count; i++)
                        {
                            if (usuarioSplit[i] != r.respostaCorreta[i])
                            {
                                estaCorreta = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        // resposta única
                        estaCorreta = r.respostaUsuario == r.respostaCorreta[0];
                    }
                }

                contagem[habilidade] = (atual.corretas + (estaCorreta ? 1 : 0), atual.total + 1);
            }
        }

        // Limpa botões anteriores
        foreach (Transform child in containerBotoes)
            Destroy(child.gameObject);

        listaHabilidadesFinal = new List<HabilidadeResultado>();

        // Criar botões por habilidade
        foreach (var entry in contagem)
        {
            string habilidade = entry.Key;
            int corretas = entry.Value.corretas;
            int total = entry.Value.total;

            bool aprovado = ((float)corretas / total) >= 0.66f;

            listaHabilidadesFinal.Add(new HabilidadeResultado
            {
                habilidade = habilidade,
                correta = aprovado
            });

            GameObject botao = Instantiate(botaoPrefab, containerBotoes);

            TMP_Text tmpTexto = botao.GetComponentInChildren<TMP_Text>();
            if (tmpTexto != null)
                tmpTexto.text = habilidade;
            else
            {
                Text texto = botao.GetComponentInChildren<Text>();
                if (texto != null)
                    texto.text = habilidade;
            }

            Image img = botao.GetComponent<Image>();
            if (img != null)
                img.color = aprovado
                    ? new Color(0.3f, 0.8f, 0.3f)
                    : new Color(0.9f, 0.3f, 0.3f);

            Debug.Log($"[HabilidadeFeedbackUI] Hab: {habilidade} | Acertos: {corretas}/{total} | Aprovado: {aprovado}");
        }
    }
}
