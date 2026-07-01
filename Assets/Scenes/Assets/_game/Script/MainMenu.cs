using UnityEngine;
using UnityEngine.SceneManagement; // Bắt buộc phải có dòng này để chuyển Scene

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        // THAY TÊN SCENE: Đổi chữ "TenSceneCuaBan" thành đúng tên file Scene game của bạn (viết hoa/thường y hệt)
        SceneManager.LoadScene("StudyMechanic");
    }
}