using UnityEngine;

// Kid running across the cafe. If it touches the player you pay damages; dodge it
// and it despawns at the far wall.
public class Npckid : MonoBehaviour
{
    public float speed = 5f;
    [SerializeField] int bumpCost = 200000;

    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            NotificationManager.Instance?.Show($"A kid bumped you! Spilled coffee, -{bumpCost:N0} VND.", 3f);
            FindFirstObjectByType<PlayerStats>()?.ApplyDailyEvent(-bumpCost, 0f, "Kid knocked over coffee");
            gameObject.SetActive(false);
        }
        else if (other.CompareTag("Wall"))
        {
            gameObject.SetActive(false); // player dodged it
        }
    }
}
