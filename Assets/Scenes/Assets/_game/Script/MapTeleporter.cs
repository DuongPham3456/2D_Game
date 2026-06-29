using System.Collections.Generic;
using UnityEngine;

// Door -> map popup -> click a location -> teleport the player to that room's spawn.
//
// Wiring (all in the Inspector, no extra code):
//   1. Door object: an ActionTrigger whose onInteract -> MapTeleporter.OpenMap().
//   2. Map popup: a UI panel (the campus image), hidden by default, assigned to mapPanel.
//   3. Each of the 4 location buttons: OnClick -> MapTeleporter.TeleportTo(index 0..3),
//      in the SAME order as the locations list below.
//   4. Optional close/back button: OnClick -> MapTeleporter.CloseMap().
public class MapTeleporter : MonoBehaviour
{
    [System.Serializable]
    public class Location
    {
        public string name;              // Classroom / Bedroom / Cafe / Bank
        public Transform spawnPoint;     // where the player lands in that room
        public Transform cameraTarget;   // optional: camera jumps here; leave null to let it follow
    }

    [Header("Map UI")]
    [SerializeField] GameObject mapPanel;   // the popup, hidden on Start

    [Header("Locations (order must match the button indices)")]
    [SerializeField] List<Location> locations = new List<Location>();

    [Header("Player")]
    [SerializeField] Transform player;      // auto-found by "Player" tag if left empty

    void Awake()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Start()
    {
        if (mapPanel != null) mapPanel.SetActive(false);
    }

    public void OpenMap()
    {
        if (mapPanel == null) return;
        mapPanel.SetActive(true);
        Time.timeScale = 0f;   // pause so the player can't walk while choosing
    }

    public void CloseMap()
    {
        if (mapPanel != null) mapPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // Hook each location button's OnClick to this, typing the location NAME
    // (e.g. "Cafe") into the string field. Matching is case-insensitive.
    public void TeleportTo(string locationName)
    {
        Location loc = locations.Find(
            l => string.Equals(l.name, locationName, System.StringComparison.OrdinalIgnoreCase));

        if (loc == null)
        {
            Debug.LogWarning($"[MapTeleporter] No location named '{locationName}'. Check the spelling matches the Locations list.");
            return;
        }
        Teleport(loc);
    }

    // Also usable by index if you ever wire a button that way.
    public void TeleportTo(int index)
    {
        if (index < 0 || index >= locations.Count) return;
        Teleport(locations[index]);
    }

    void Teleport(Location loc)
    {
        if (player == null || loc.spawnPoint == null) return;

        player.position = loc.spawnPoint.position;

        if (loc.cameraTarget != null)
        {
            Camera cam = Camera.main;
            if (cam != null)
                cam.transform.position = new Vector3(
                    loc.cameraTarget.position.x,
                    loc.cameraTarget.position.y,
                    cam.transform.position.z);
        }

        CloseMap();
    }
}
