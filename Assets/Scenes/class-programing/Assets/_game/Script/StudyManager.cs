using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StudyManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private PlayerStats playerStats;

    [Header("Question Bank")]
    [SerializeField] private List<QuestionData> allQuestions;
    private HashSet<string> usedQuestionTexts = new HashSet<string>();

    [Header("UI Book Panel")]
    [SerializeField] private GameObject bookPanel;
    [SerializeField] private TextMeshProUGUI questionTextUI;
    [SerializeField] private TextMeshProUGUI hintTextUI;
    [SerializeField] private Button buttonA;
    [SerializeField] private Button buttonB;
    [SerializeField] private Button buttonC;
    [SerializeField] private TextMeshProUGUI timerTextUI;

    private List<QuestionData> currentSessionQuestions = new List<QuestionData>();
    private int currentQuestionIndex = 0;
    private int studyCountToday = 0;
    private int lastCheckedDay = -1;
    
    private Coroutine timerCoroutine;
    private bool isAnswering = false;

    void Start()
    {
        // Ẩn quyển vở khi mới vào game
        if (bookPanel != null) bookPanel.SetActive(false);
        
        // Lắng nghe sự kiện bấm nút đáp án
        buttonA.onClick.AddListener(() => OnAnswerSelected("A"));
        buttonB.onClick.AddListener(() => OnAnswerSelected("B"));
        buttonC.onClick.AddListener(() => OnAnswerSelected("C"));
    }

    // Hàm gọi khi tương tác với bàn học
    public void InteractWithDesk()
    {
        if (bookPanel.activeSelf) return;

        int currentDay = timeManager.day;
        DaySlot currentSlot = timeManager.CurrentSlot;

        if (currentDay != lastCheckedDay)
        {
            studyCountToday = 0;
            lastCheckedDay = currentDay;
        }

        // Kiểm tra giới hạn số lần học/thi theo ngày
        if (currentDay == 5 || currentDay == 10)
        {
            if (studyCountToday >= 1)
            {
                Debug.Log("Bạn đã làm bài kiểm tra của ngày hôm nay rồi!");
                return;
            }
        }
        else 
        {   
             if (studyCountToday >= 2)
            {
                Debug.Log("Hôm nay bạn đã học đủ 2 lần rồi!");
                return;
            }
            
            if (currentSlot == DaySlot.Evening)
            {
                Debug.Log("Buổi tối rồi, hãy đi ngủ!");
                return;
            }
        }

        // Tính toán thể lực tiêu hao
        float energyCost = (currentDay == 5 || currentDay == 10) ? 40f : 20f;
        
        if (playerStats != null && playerStats.Energy < energyCost)
        {
            Debug.Log("Không đủ thể lực để học!");
            return;
        }

        // Trừ thể lực trực tiếp qua hàm public mới tạo ở PlayerStats
        if (playerStats != null) playerStats.ConsumeEnergyForStudy(energyCost);

        // Bốc đề 3 câu ngẫu nhiên theo ngày
        GenerateSessionQuestions(currentDay);

        if (currentSessionQuestions.Count < 3)
        {
            usedQuestionTexts.Clear(); // Tái sử dụng nếu cày hết kho câu hỏi
            GenerateSessionQuestions(currentDay);
        }

        studyCountToday++;
        bookPanel.SetActive(true);
        currentQuestionIndex = 0;
        ShowQuestion(currentQuestionIndex);
    }

    private void GenerateSessionQuestions(int day)
    {
        currentSessionQuestions.Clear();
        QuestionType targetType = QuestionType.Normal;
        
        if (day == 5) targetType = QuestionType.Midterm;
        else if (day == 10) targetType = QuestionType.Final;

        List<QuestionData> validPool = allQuestions.FindAll(q => q.type == targetType && !usedQuestionTexts.Contains(q.questionText));

        int countToPick = Mathf.Min(3, validPool.Count);
        for (int i = 0; i < countToPick; i++)
        {
            int randIndex = Random.Range(0, validPool.Count);
            currentSessionQuestions.Add(validPool[randIndex]);
            usedQuestionTexts.Add(validPool[randIndex].questionText);
            validPool.RemoveAt(randIndex);
        }
    }

    private void ShowQuestion(int index)
    {
        if (index >= currentSessionQuestions.Count)
        {
            EndStudySession();
            return;
        }

        QuestionData q = currentSessionQuestions[index];
        isAnswering = true;

        // Đổ chữ vào trang câu hỏi (Bên trái)
        questionTextUI.text = $"<b>CÂU {index + 1}:</b> {q.questionText}\n\n<b>A.</b> {q.optionA}\n<b>B.</b> {q.optionB}\n<b>C.</b> {q.optionC}";

        // Đổ chữ vào trang gợi ý (Bên phải) - Ngày thi không có hint
        if (timeManager.day == 5 || timeManager.day == 10)
        {
            hintTextUI.text = "<color=red><b>BÀI KIỂM TRA</b></color>\nKhông có gợi ý cho kỳ thi!";
        }
        else
        {
            hintTextUI.text = $"<b>GỢI Ý:</b>\n{q.hintText}";
        }

        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(CountdownTimer(10f));
    }

    private IEnumerator CountdownTimer(float duration)
    {
        float timeRemaining = duration;
        while (timeRemaining > 0)
        {
            timerTextUI.text = $"Thời gian: <color=yellow>{Mathf.CeilToInt(timeRemaining)}s</color>";
            yield return new WaitForSeconds(1f);
            timeRemaining -= 1f;
        }
        
        timerTextUI.text = "<color=red>HẾT GIỜ!</color>";
        isAnswering = false;
        yield return new WaitForSeconds(0.8f);
        NextQuestion();
    }

    private void OnAnswerSelected(string choice)
    {
        if (!isAnswering) return;
        isAnswering = false;
        
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);

        QuestionData currentQ = currentSessionQuestions[currentQuestionIndex];
        
        if (choice == currentQ.correctAnswer)
        {
            // Điểm thưởng quy đổi ra chỉ số Kiến thức (+5 điểm = +0.1 GPA)
            float knowledgeGain = 5f; 
            if (timeManager.day == 5) knowledgeGain = 15f; 
            else if (timeManager.day == 10) knowledgeGain = 25f; 

            if (playerStats != null) playerStats.AddKnowledgeFromQuiz(knowledgeGain);
        }

        NextQuestion();
    }

    private void NextQuestion()
    {
        currentQuestionIndex++;
        ShowQuestion(currentQuestionIndex);
    }

    private void EndStudySession()
    {
        bookPanel.SetActive(false);
        
        // Học ngày thường tự động tăng Slot thời gian (Sáng -> Chiều -> Tối)
        if (timeManager.day != 5 && timeManager.day != 10)
        {
            timeManager.AdvanceSlot();
        }
    }
}