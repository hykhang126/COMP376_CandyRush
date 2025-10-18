using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Physics2DRaycaster))]
public class InputManager : MonoBehaviour
{
    private Physics2DRaycaster physics2DRaycaster;

    private void Start()
    {
        physics2DRaycaster = GetComponent<Physics2DRaycaster>();
    }

}
