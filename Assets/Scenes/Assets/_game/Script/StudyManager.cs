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

    [Header("Config (optional ScriptableObject)")]
    [SerializeField] private StudentGameConfig config;

    [Header("Quiz Tuning (overridden by config if set)")]
    [SerializeField] private float studyEnergyCost = 20f;
    [SerializeField] private float examEnergyCost = 40f;
    [SerializeField] private float studySanityCost = 8f;
    [SerializeField] private float normalKnowledgeGain = 5f;
    [SerializeField] private float midtermKnowledgeGain = 15f;
    [SerializeField] private float finalKnowledgeGain = 25f;

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
        ApplyConfigIfPresent();

        // Ẩn quyển vở khi mới vào game
        if (bookPanel != null) bookPanel.SetActive(false);
        
        // Đổi cách lắng nghe sự kiện để chạy Coroutine
        if (buttonA != null) buttonA.onClick.AddListener(() => StartCoroutine(OnAnswerSelected("A")));
        if (buttonB != null) buttonB.onClick.AddListener(() => StartCoroutine(OnAnswerSelected("B")));
        if (buttonC != null) buttonC.onClick.AddListener(() => StartCoroutine(OnAnswerSelected("C")));
    }

    void ApplyConfigIfPresent()
    {
        if (config == null) return;
        studyEnergyCost = config.quizStudyEnergyCost;
        examEnergyCost = config.quizExamEnergyCost;
        studySanityCost = config.quizStudySanityCost;
        normalKnowledgeGain = config.quizNormalKnowledgeGain;
        midtermKnowledgeGain = config.quizMidtermKnowledgeGain;
        finalKnowledgeGain = config.quizFinalKnowledgeGain;
    }

    // Hàm gọi khi tương tác với bàn học
    public void InteractWithDesk()
    {
        if (bookPanel == null || bookPanel.activeSelf) return;

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

        // Không có câu hỏi cho hôm nay thì đừng trừ chỉ số (tránh mất lượt vì vở rỗng).
        if (CountQuestionsForDay(currentDay) == 0)
        {
            Debug.Log("Chưa có câu hỏi cho hôm nay.");
            return;
        }

        // Bắt đầu phiên học: cùng luật với core loop — kiểm tra suy sụp, trừ thể lực
        // (có nhân theo mức stress) và trừ tinh thần. Không đủ điều kiện thì không mở vở.
        float energyCost = (currentDay == 5 || currentDay == 10) ? examEnergyCost : studyEnergyCost;
        if (playerStats != null && !playerStats.TryStartQuizStudy(energyCost, studySanityCost))
            return;

        // Bốc đề 3 câu ngẫu nhiên theo ngày
        GenerateSessionQuestions(currentDay);

        if (currentSessionQuestions.Count < 3)
        {
            usedQuestionTexts.Clear(); // Tái sử dụng nếu cày hết kho câu hỏi
            GenerateSessionQuestions(currentDay);
        }

        studyCountToday++;
        bookPanel.SetActive(true);
        UIModal.Open();
        currentQuestionIndex = 0;
        ShowQuestion(currentQuestionIndex);
    }

    int CountQuestionsForDay(int day)
    {
        if (allQuestions == null) return 0;
        QuestionType t = day == 5 ? QuestionType.Midterm
                       : day == 10 ? QuestionType.Final
                       : QuestionType.Normal;
        int n = 0;
        foreach (var q in allQuestions)
            if (q != null && q.type == t) n++;
        return n;
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
        if (questionTextUI != null)
            questionTextUI.text = $"<b>CÂU {index + 1}:</b> {q.questionText}\n\n<b>A.</b> {q.optionA}\n<b>B.</b> {q.optionB}\n<b>C.</b> {q.optionC}";

        // Đổ chữ vào trang gợi ý (Bên phải) - Ngày thi không có hint
        if (hintTextUI != null)
        {
            if (timeManager.day == 5 || timeManager.day == 10)
                hintTextUI.text = "<color=red><b>BÀI KIỂM TRA</b></color>\nKhông có gợi ý cho kỳ thi!";
            else
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
            if (timerTextUI != null)
                timerTextUI.text = $"Thời gian: <color=yellow>{Mathf.CeilToInt(timeRemaining)}s</color>";
            yield return new WaitForSeconds(1f);
            timeRemaining -= 1f;
        }

        if (timerTextUI != null)
            timerTextUI.text = "<color=red>HẾT GIỜ!</color>";
        isAnswering = false;
        yield return new WaitForSeconds(0.8f);
        NextQuestion();
    }

    private IEnumerator OnAnswerSelected(string choice)
    {
        if (!isAnswering) yield break;
        isAnswering = false;
        
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);

        QuestionData currentQ = currentSessionQuestions[currentQuestionIndex];
        
        if (choice == currentQ.correctAnswer)
        {
            // Điểm thưởng quy đổi ra chỉ số Kiến thức (đã nhân theo mức stress)
            float knowledgeGain = normalKnowledgeGain;
            if (timeManager.day == 5) knowledgeGain = midtermKnowledgeGain;
            else if (timeManager.day == 10) knowledgeGain = finalKnowledgeGain;

            if (playerStats != null) playerStats.AddQuizKnowledge(knowledgeGain);

            if (hintTextUI != null)
                hintTextUI.text = "<color=green><b>CHÍNH XÁC!</b></color>\nBạn được cộng điểm kiến thức.";
            yield return new WaitForSeconds(1f); // Chờ 1 giây rồi qua câu mới
        }
        else
        {
            // KHI TRẢ LỜI SAI: Ghi đè chữ vào trang bên phải để thông báo
            if (hintTextUI != null)
                hintTextUI.text = "<color=red><b>Chưa chính xác!</b></color>\nHãy cố gắng hơn nhé!";
            
            // Chờ 1.5 giây để người chơi đọc thông báo trước khi nhảy câu
            yield return new WaitForSeconds(1.5f); 
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
        if (bookPanel != null) bookPanel.SetActive(false);
        UIModal.Close();

        // Suy sụp nếu tinh thần cạn — giống FinishActivity() ở core loop.
        if (playerStats != null) playerStats.EndQuizStudy();

        // Học ngày thường tự động tăng Slot thời gian (Sáng -> Chiều -> Tối)
        if (timeManager.day != 5 && timeManager.day != 10)
        {
            timeManager.AdvanceSlot();
        }
    }
}