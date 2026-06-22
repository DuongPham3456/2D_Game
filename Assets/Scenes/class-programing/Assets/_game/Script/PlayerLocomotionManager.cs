using System.Collections;
using UnityEngine;

public class PlayerLocomotionManager : MonoBehaviour
{
    [SerializeField] Transform playerTransform;
    [SerializeField] Rigidbody2D rb;

    public void Awake()
    {
        if (playerTransform == null) playerTransform = GetComponent<Transform>();
        // Cache the Rigidbody2D. Moving a dynamic body by writing transform
        // directly forces the physics engine to re-sync colliders every step,
        // which tanks the frame rate. We move through the body instead.
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }

    public void FixedUpdate()
    {
        HandleMovement();
    }

    [Header("Moving Properties")]
    public int verticalInput;
    public int horizontalInput;
    public bool allowToMove = true;
    public float speed;
    public void HandleMovement()
    {
        if (!allowToMove)
        {
            return;
        }

        Vector2 delta = new Vector2(horizontalInput, verticalInput) * speed * Time.fixedDeltaTime;

        if (rb != null)
            rb.MovePosition(rb.position + delta);
        else
            playerTransform.localPosition += (Vector3)delta;
    }


    [Header("Dodge Properties")]
    public bool allowToDodge = true;
    public float dodgeSpeed;
    public float dodgeDuration;
    public float dodgeCooldown;
    public IEnumerator HandleDodge()
    {
        // sửa lại idea thành trong fixed update sẽ chạy hàm này liên tục nếu bool dodgeInput bật lên true thì thực thi, đồng thời sửa lại logic while khả năng phải ngh
        // thêm đầu chặn để ko cho thực hiện dodge khi đang dodge
        if (allowToDodge)
        {
            allowToDodge = false;
            float tempDuration = dodgeDuration;
            // Set để ko di chuyển được khi đang dodge
            allowToMove = false;
            // không nhận input từ playerInput nữa để ko bị đổi hướng
            Vector2 dir = new Vector2(horizontalInput, verticalInput);
            while (dodgeDuration > 0)
            {
                Vector2 dodgeDelta = dir * dodgeSpeed * Time.fixedDeltaTime;
                if (rb != null)
                    rb.MovePosition(rb.position + dodgeDelta);
                else
                    playerTransform.localPosition += (Vector3)dodgeDelta;
                dodgeDuration -= Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            allowToMove = true;
            dodgeDuration = tempDuration;

            // thêm cool down để tránh spam dodge
            float tempCooldown = dodgeCooldown;
            while (dodgeCooldown > 0)
            {
                dodgeCooldown -= Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            dodgeCooldown = tempCooldown;
            allowToDodge = true;
        }
    }
}
