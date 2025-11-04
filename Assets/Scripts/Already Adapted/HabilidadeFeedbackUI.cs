using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // necessário para TextMeshPro

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
        Dictionary<string, (int corretas, int total)> contagemHabilidades = new Dictionary<string, (int, int)>();

        foreach (var r in respostas)
        {
            if (!contagemHabilidades.ContainsKey(r.habilidade))
                contagemHabilidades[r.habilidade] = (0, 0);

            var atual = contagemHabilidades[r.habilidade];
            bool estaCorreta = false;

            if (r.respostaCorreta is List<object> lista)
            {
                if (r.respostaUsuario.Contains("|"))
                {
                    string[] respostasUsuario = r.respostaUsuario.Split('|');
                    estaCorreta = true;
                    for (int i = 0; i < lista.Count; i++)
                    {
                        if (i >= respostasUsuario.Length || respostasUsuario[i] != lista[i]?.ToString())
                        {
                            estaCorreta = false;
                            break;
                        }
                    }
                }
                else
                {
                    estaCorreta = r.respostaUsuario == lista[0]?.ToString();
                }
            }

            contagemHabilidades[r.habilidade] = (atual.corretas + (estaCorreta ? 1 : 0), atual.total + 1);
        }

        // Limpa o container antes de recriar os botões
        foreach (Transform child in containerBotoes)
            Destroy(child.gameObject);

        listaHabilidadesFinal = new List<HabilidadeResultado>();

        foreach (var entry in contagemHabilidades)
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

            // Tenta encontrar TextMeshProUGUI primeiro
            TMP_Text tmpTexto = botao.GetComponentInChildren<TMP_Text>();
            if (tmpTexto != null)
            {
                tmpTexto.text = habilidade;
            }
            else
            {
                // Fallback para o Text padrão
                Text textoBotao = botao.GetComponentInChildren<Text>();
                if (textoBotao != null)
                    textoBotao.text = habilidade;
            }

            // Define a cor do botão
            Image imagemBotao = botao.GetComponent<Image>();
            if (imagemBotao != null)
                imagemBotao.color = aprovado ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.9f, 0.3f, 0.3f);

            Debug.Log($"Habilidade: {habilidade} | Acertos: {corretas}/{total} | Aprovado? {aprovado}");
        }
    }
}
