using UnityEngine;

// Scales this SpriteRenderer to exactly fill the orthographic camera's view, at any
// resolution/aspect. Put it on the world Background sprite. If the camera follows the
// player, make the Background a CHILD of the camera so it stays centered.
[RequireComponent(typeof(SpriteRenderer))]
[ExecuteAlways]
public class FitBackgroundToCamera : MonoBehaviour
{
    [SerializeField] Camera cam;
    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (cam == null) cam = Camera.main;
    }

    void OnEnable() => Fit();
    void Update() { if (!Application.isPlaying) Fit(); }   // live-preview in the editor

    void Fit()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (cam == null) cam = Camera.main;
        if (cam == null || sr == null || sr.sprite == null || !cam.orthographic) return;

        float worldH = 2f * cam.orthographicSize;
        float worldW = worldH * cam.aspect;
        Vector2 size = sr.sprite.bounds.size;   // world size at scale 1
        if (size.x <= 0f || size.y <= 0f) return;
        transform.localScale = new Vector3(worldW / size.x, worldH / size.y, 1f);
    }
}
