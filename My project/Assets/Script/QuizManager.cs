using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuizManager : MonoBehaviour
{
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI ScriptNumQuestao; // Contador de questões (1 a 10)
    public Button[] answerButtons; // 4 buttons for A, B, C, D
    public Button nextButton;
    public Slider ProgressoLicao; // Slider para progresso visual
    private List<Question> questions;
    private Question currentQuestion;
    private int currentQuestionIndex = 0;
    private int correctAnswers = 0;

    [System.Serializable]
    public class Question
    {
        public string text;
        public string[] options;
        public int correctIndex;
    }

    [System.Serializable]
    public class QuestionList
    {
        public List<Question> questions;
    }

    void Start()
    {
        LoadQuestions();
        DisplayQuestion();
        nextButton.onClick.AddListener(NextQuestion);
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i; // Capture index for closure
            answerButtons[i].onClick.AddListener(() => CheckAnswer(index));
        }
    }

    void LoadQuestions()
    {
        TextAsset jsonText = Resources.Load<TextAsset>("questions");
        QuestionList questionList = JsonUtility.FromJson<QuestionList>(jsonText.text);
        questions = questionList.questions.GetRange(0, 10); // Use only first 10 for daily quiz
        ShuffleQuestions();
    }

    void ShuffleQuestions()
    {
        for (int i = questions.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            Question temp = questions[i];
            questions[i] = questions[rand];
            questions[rand] = temp;
        }
    }

    void DisplayQuestion()
    {
        if (currentQuestionIndex < questions.Count)
        {
            currentQuestion = questions[currentQuestionIndex];
            questionText.text = currentQuestion.text;
            ScriptNumQuestao.text = (currentQuestionIndex + 1).ToString() + "/10"; // Atualiza contador
            ProgressoLicao.value = currentQuestionIndex; // Atualiza slider
            for (int i = 0; i < answerButtons.Length; i++)
            {
                answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.options[i];
                answerButtons[i].image.color = Color.white; // Reset color
                answerButtons[i].interactable = true; // Re-enable buttons
            }
            nextButton.gameObject.SetActive(false);
            nextButton.interactable = false;
        }
        else
        {
            questionText.text = "Quiz completo! Acertos: " + correctAnswers + "/10";
            ScriptNumQuestao.text = "10/10"; // Finaliza o contador
            ProgressoLicao.value = 10; // Completa o slider
            foreach (var btn in answerButtons) btn.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(false);
        }
    }

    void CheckAnswer(int selectedIndex)
    {
        bool isCorrect = selectedIndex == currentQuestion.correctIndex;
        answerButtons[selectedIndex].image.color = isCorrect ? Color.green : Color.red; // Botão clicado
        if (!isCorrect)
        {
            answerButtons[currentQuestion.correctIndex].image.color = Color.green; // Mostra a correta
        }
        if (isCorrect) correctAnswers++;
        nextButton.gameObject.SetActive(true);
        nextButton.interactable = true;
        foreach (var btn in answerButtons) btn.interactable = false; // Disable buttons after answer
    }

    void NextQuestion()
    {
        currentQuestionIndex++;
        foreach (var btn in answerButtons) btn.interactable = true; // Re-enable buttons
        DisplayQuestion();
    }
}