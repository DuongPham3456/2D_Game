using UnityEngine;

public enum QuestionType { Normal, Midterm, Final }

[CreateAssetMenu(fileName = "NewQuestion", menuName = "StudySystem/Question Asset")]
public class QuestionData : ScriptableObject
{
    public QuestionType type;
    
    [TextArea(3, 6)]
    public string questionText;
    
    public string optionA;
    public string optionB;
    public string optionC;
    
    [Tooltip("Chỉ điền chữ in hoa: A, B hoặc C")]
    public string correctAnswer;
    
    [TextArea(2, 5)]
    public string hintText;
}