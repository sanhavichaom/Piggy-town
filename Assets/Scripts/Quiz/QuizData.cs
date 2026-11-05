using UnityEngine;

[CreateAssetMenu(fileName = "QuizData", menuName = "PiggyTown/Quiz Data")]
public class QuizData : ScriptableObject
{
    // Local classes / structs

    [System.Serializable]
    public class Question
    {
        [TextArea(2, 4)]
        [SerializeField] string _questionText;
        public string QuestionText { get { return _questionText; } }

        [SerializeField] string[] _options = new string[4];
        public string[] Options { get { return _options; } }

        [SerializeField, Range(0, 3)]
        int _correctAnswerIndex;
        public int CorrectAnswerIndex { get { return _correctAnswerIndex; } }
    }

    // Variables

    [Header("Quiz Information")]
    [SerializeField] string _id;
    public string Id { get { return _id; } }

    [Header("Questions")]
    [SerializeField] Question[] _questions = new Question[5];
    public Question[] Questions { get { return _questions; } }
}