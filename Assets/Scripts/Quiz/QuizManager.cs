using UnityEngine;

public class QuizManager : MonoBehaviour
{
    // Constants

    // Local classes / structs

    // Enums

    // Variables

    [Header("Quiz")]
    [SerializeField] QuizData _quizData;

    int _currentQuestionIndex;
    public int CurrentQuestionIndex { get { return _currentQuestionIndex; } }
    bool _isComplete;
    public bool IsComplete { get { return _isComplete; } }
    bool _isReady;

    string _progressKey;
    string _completeKey;
    public int QuestionCount { get { return _quizData.Questions.Length; } }

    public QuizData.Question CurrentQuestion
    {
        get
        {
            if (!_isReady || _isComplete)
                return null;

            return _quizData.Questions[_currentQuestionIndex];
        }
    }

    // Methods

    void Awake()
    {
        if (_quizData == null || string.IsNullOrEmpty(_quizData.Id) ||
            _quizData.Questions == null || _quizData.Questions.Length != 5)
        {
            Debug.LogError("Quiz Data is invalid.", this);
            enabled = false;
            return;
        }

        for (int i = 0; i < _quizData.Questions.Length; i++)
        {
            QuizData.Question question = _quizData.Questions[i];

            if (question == null || string.IsNullOrEmpty(question.QuestionText) ||
                question.Options == null || question.Options.Length != 4 ||
                question.CorrectAnswerIndex < 0 || question.CorrectAnswerIndex >= 4)
            {
                Debug.LogError("Invalid quiz question: " + i, this);
                enabled = false;
                return;
            }
        }

        _progressKey = "PiggyTown.Quiz." + _quizData.Id + ".Progress";
        _completeKey = "PiggyTown.Quiz." + _quizData.Id + ".Complete";

        _currentQuestionIndex = PlayerPrefs.GetInt(_progressKey, 0);
        _currentQuestionIndex = Mathf.Clamp(_currentQuestionIndex, 0, _quizData.Questions.Length);

        _isComplete = PlayerPrefs.GetInt(_completeKey, 0) == 1;

        if (_currentQuestionIndex >= _quizData.Questions.Length)
            _isComplete = true;

        _isReady = true;
    }

    public bool TryAnswer(int answerIndex)
    {
        if (!_isReady || _isComplete)
            return false;

        if (answerIndex < 0 || answerIndex >= 4)
            return false;

        QuizData.Question question = _quizData.Questions[_currentQuestionIndex];

        if (answerIndex != question.CorrectAnswerIndex)
            return false;

        _currentQuestionIndex++;

        if (_currentQuestionIndex >= _quizData.Questions.Length)
            _isComplete = true;

        SaveProgress();

        return true;
    }

    void SaveProgress()
    {
        PlayerPrefs.SetInt(_progressKey, _currentQuestionIndex);
        PlayerPrefs.SetInt(_completeKey, _isComplete ? 1 : 0);

        PlayerPrefs.Save();
    }

    [ContextMenu("Reset Quiz Progress (Test)")]
    void ResetQuizProgress()
    {
        _currentQuestionIndex = 0;
        _isComplete = false;

        SaveProgress();

        Debug.Log("Quiz progress reset for testing.");
    }
}