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

    public string Objetivo; // NOVO CAMPO

    public List<string> Alternativas;

    public string Explicacoes; // AGORA É STRING

    public List<string> Habilidades; // AGORA É ARRAY

    public List<string> RespostaCorreta; // SEMPRE LISTA
}
