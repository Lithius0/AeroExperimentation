using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public FlightModel FlightModel;

    public InputAction PitchAction;
    public InputAction YawAction;
    public InputAction RollAction;

    private Vector3 previousControlVector = Vector3.zero;

    private void OnEnable()
    {
        PitchAction.Enable();
        YawAction.Enable();
        RollAction.Enable();
    }

    private void OnDisable()
    {
        PitchAction.Disable();
        YawAction.Disable();
        RollAction.Disable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 controlVector;
        controlVector.x = PitchAction.ReadValue<float>();
        controlVector.y = YawAction.ReadValue<float>();
        controlVector.z = RollAction.ReadValue<float>();

        previousControlVector = Vector3.MoveTowards(previousControlVector, controlVector, Time.deltaTime * 10);

        FlightModel.ApplyControl(previousControlVector);

    }
}
