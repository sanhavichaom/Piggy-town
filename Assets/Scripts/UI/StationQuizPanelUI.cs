using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StationQuizPanelUI : MonoBehaviour
{
    // Constants

    // Local classes / structs

    // Enums

    // Variables

    [Header("References")]
    [SerializeField] PlaceableObjectData _stationData;
    [SerializeField] QuizManager _quizManager;

    [Header("Texts")]
    [SerializeField] TMP_Text _questionNumberText;
    [SerializeField] TMP_Text _questionText;
    [SerializeField] TMP_Text _feedbackText;

    [Header("Answers")]
    [SerializeField] Button[] _answerButtons = new Button[4];
    [SerializeField] TMP_Text[] _answerTexts = new TMP_Text[4];

    [Header("Controls")]
    [SerializeField] Button _closeButton;

    public bool IsOpen { get { return gameObject.activeSelf; } }

    // Methods

    void Awake()
    {
        if (_stationData == null || _quizManager == null ||
            _questionNumberText == null || _questionText == null ||
            _feedbackText == null || _closeButton == null ||
            _answerButtons == null || _answerTexts == null ||
            _answerButtons.Length != 4 || _answerTexts.Length != 4)
        {
            Debug.LogError("Quiz Panel references are missing.", this);
            enabled = false;
            gameObject.SetActive(false);
            return;
        }

        for (int i = 0; i < 4; i++)
        {
            if (_answerButtons[i] == null || _answerTexts[i] == null)
            {
                Debug.LogError("Quiz answer references are missing.", this);
                enabled = false;
                gameObject.SetActive(false);
                return;
            }
        }

        _answerButtons[0].onClick.AddListener(Answer0);
        _answerButtons[1].onClick.AddListener(Answer1);
        _answerButtons[2].onClick.AddListener(Answer2);
        _answerButtons[3].onClick.AddListener(Answer3);

        _closeButton.onClick.AddListener(Close);

        gameObject.SetActive(false);
    }

    public bool OpenFor(PlaceableObject placedObject)
    {
        if (!enabled || placedObject == null)
            return false;

        if (placedObject.Data != _stationData)
            return false;

        gameObject.SetActive(true);

        _feedbackText.text = "";
        RefreshUI();

        return true;
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    void RefreshUI()
    {
        if (_quizManager.IsComplete)
        {
            _questionNumberText.text = "Congrats!!!";
            _questionText.text = "Completed";
            _feedbackText.text = "Quiz completed.";

            for (int i = 0; i < 4; i++)
                _answerButtons[i].gameObject.SetActive(false);

            return;
        }

        QuizData.Question question = _quizManager.CurrentQuestion;

        if (question == null)
        {
            _questionText.text = "Quiz unavailable.";
            return;
        }

        _questionNumberText.text = "Question " + (_quizManager.CurrentQuestionIndex + 1) + " / " + _quizManager.QuestionCount;

        _questionText.text = question.QuestionText;

        for (int i = 0; i < 4; i++)
        {
            _answerButtons[i].gameObject.SetActive(true);
            _answerTexts[i].text = question.Options[i];
        }
    }

    void SubmitAnswer(int answerIndex)
    {
        bool isCorrect = _quizManager.TryAnswer(answerIndex);

        if (!isCorrect)
        {
            _feedbackText.text = "Wrong answer. Try again.";
            return;
        }

        RefreshUI();

        if (!_quizManager.IsComplete)
            _feedbackText.text = "Correct!";
    }

    void Answer0()
    {
        SubmitAnswer(0);
    }

    void Answer1()
    {
        SubmitAnswer(1);
    }

    void Answer2()
    {
        SubmitAnswer(2);
    }

    void Answer3()
    {
        SubmitAnswer(3);
    }

    void OnDestroy()
    {
        if (_answerButtons != null && _answerButtons.Length == 4)
        {
            if (_answerButtons[0] != null)
                _answerButtons[0].onClick.RemoveListener(Answer0);

            if (_answerButtons[1] != null)
                _answerButtons[1].onClick.RemoveListener(Answer1);

            if (_answerButtons[2] != null)
                _answerButtons[2].onClick.RemoveListener(Answer2);

            if (_answerButtons[3] != null)
                _answerButtons[3].onClick.RemoveListener(Answer3);
        }

        if (_closeButton != null)
            _closeButton.onClick.RemoveListener(Close);
    }
}