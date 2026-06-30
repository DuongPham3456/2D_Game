using UnityEngine;

public class PlayerFootsteps : MonoBehaviour
{
    [Header("Cài đặt Âm thanh")]
    public AudioSource footstepSound; 
    public float stepDelay = 0.3f;    

    private float stepTimer = 0f;

    void Start()
    {
        if (footstepSound == null) 
        {
            footstepSound = GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        // Kiểm tra xem người chơi có đang bấm nút di chuyển không
        bool isMoving = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;

        if (isMoving)
        {
            stepTimer -= Time.deltaTime; 
            
            if (stepTimer <= 0f)
            {
                // Nếu loa đang không phát thì mới phát để tránh tiếng âm thanh bị đè lên nhau
                if (footstepSound != null && !footstepSound.isPlaying) 
                {
                    footstepSound.Play();
                }
                stepTimer = stepDelay; 
            }
        }
        else
        {
            // BỔ SUNG: KHI THẢ TAY (KHÔNG DI CHUYỂN) -> Tắt tiếng loa ngay lập tức!
            if (footstepSound != null && footstepSound.isPlaying)
            {
                footstepSound.Stop();
            }
            
            // Đặt lại timer về 0 để lần sau vừa ấn nút là nhân vật phát tiếng bước đi ngay
            stepTimer = 0f; 
        }
    }
}