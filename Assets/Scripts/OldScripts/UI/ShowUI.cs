using UnityEngine;
using UnityEngine.InputSystem;

public class ShowUI : MonoBehaviour
{
    [Header("UI to toggle")]
    public Canvas[] UITables; 

    void Update()
    {
        if (Keyboard.current.oKey.wasPressedThisFrame)
        {
            bool anyActive = false;
            foreach (var table in UITables)
            {
                if (table.enabled)
                {
                    anyActive = true;
                    break;
                }
            }

            foreach (var table in UITables)
            {
                table.enabled = !anyActive;
            }
        }
    }
}