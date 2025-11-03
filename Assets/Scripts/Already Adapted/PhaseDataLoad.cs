using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Representa uma fase carregada do Firestore.
/// </summary>
[Serializable]
public class PhaseDataLoad
{
    public List<QuestionData> diagnostica_6ano;
}

/// <summary>
/// Representa cada questão dentro da fase.
/// </summary>
[Serializable]
public class QuestionData
{
    public string Tipo;
    public string Midia;
    public string Texto;
    public string Questao;
    public List<string> Alternativas;
    public List<string> Explicacoes;
    public string Habilidades;

    // Pode ser string ou lista de strings — usamos object para suportar ambos
    public object RespostaCorreta;
}
