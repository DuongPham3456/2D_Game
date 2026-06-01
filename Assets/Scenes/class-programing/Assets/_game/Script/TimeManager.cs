using UnityEngine;
using TMPro; // Dùng để hiển thị chữ UI

public class TimeManager : MonoBehaviour
{
    [Header("Thời gian trong game")]
    public int day = 1;
    public float hour = 7f; // Bắt đầu ngày mới lúc 7h sáng
    public float minute = 0f;
    
    // Tốc độ trôi: 1 giây ngoài đời = 10 phút trong game (Bạn có thể chỉnh lại)
    public float timeSpeed = 10f; 

    [Header("UI Text")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI dayText;

    void Update()
    {
        // Tính toán thời gian trôi qua mỗi khung hình
        minute += timeSpeed * Time.deltaTime;

        // Cộng dồn phút thành giờ
        if (minute >= 60f)
        {
            hour += 1f;
            minute -= 60f; // Trừ đi 60 thay vì gán về 0 để mượt hơn
        }

        // Hết một ngày lúc 24h (12h đêm)
        if (hour >= 24f)
        {
            EndDay();
        }

        UpdateUI();
    }

    void EndDay()
    {
        hour = 7f; // Tỉnh dậy lúc 7h sáng hôm sau
        minute = 0f;
        day++;
        
        Debug.Log("Tiếng gà gáy... Đã sang Ngày " + day);
        // Lưu ý: Sau này bạn có thể gọi hàm trừ thể lực ở đây nếu người chơi thức đến tận 12h đêm!
    }

    void UpdateUI()
    {
        if (timeText != null)
        {
            // Ép kiểu về int và format định dạng giờ 00:00 (Ví dụ: 07:05, 14:30)
            timeText.text = string.Format("{0:00}:{1:00}", (int)hour, (int)minute);
        }
        
        if (dayText != null)
        {
            dayText.text = "Ngày: " + day;
        }
    }

    // Hàm mở rộng: Dùng khi làm bài tập lớn hoặc đi làm thêm sẽ tốn luôn vài tiếng
    public void SkipTime(float hoursToSkip)
    {
        hour += hoursToSkip;
        if (hour >= 24f)
        {
            EndDay();
        }
    }
}