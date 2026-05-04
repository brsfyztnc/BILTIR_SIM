using UnityEngine;
using UnityEngine.InputSystem;

public class Test : MonoBehaviour
{
    [SerializeField] private InputActionReference grabAction;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(grabAction.action.ReadValue<float>());
    }
}
