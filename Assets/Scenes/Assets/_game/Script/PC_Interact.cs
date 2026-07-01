using UnityEngine;

// Open the bedroom PC menu (PC_Manager lives on canvasPC). Interaction is handled by
// the ActionTrigger on this object (press E). PC_Manager.OnEnable takes the UI lock.
[RequireComponent(typeof(ActionTrigger))]
public class PC_Interact : MonoBehaviour
{
    public GameObject canvasPC;

    void Awake()
    {
        var trigger = GetComponent<ActionTrigger>();
        if (trigger != null) trigger.Interacted += OpenPc;
    }

    void OpenPc()
    {
        if (canvasPC != null) canvasPC.SetActive(true);
    }
}
