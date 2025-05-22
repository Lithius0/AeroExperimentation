using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public FlightModel FlightModel;
    public Camera ShipCamera;
    public RectTransform Aimpoint;

    public Vector3 ControlGain = Vector3.one;

    public InputAction PitchAction;
    public InputAction YawAction;
    public InputAction RollAction;
    public InputAction MouseMove;
    public InputAction LockMouse;

    private Vector3 previousControlVector = Vector3.zero;
    private Vector3 targetVector = Vector3.forward;

    public float RollLimit = 45f;
    public float SideslipCorrectionRollGain = 30f;
    public float RollTowardsGain = 30f;
    public float RollRecoveryGain = 30f;

    private void OnEnable()
    {
        PitchAction.Enable();
        YawAction.Enable();
        RollAction.Enable();
        MouseMove.Enable();
        LockMouse.Enable();
        LockMouse.performed += ToggleMouseLock;
        targetVector = ShipCamera.transform.forward;
    }

    private void ToggleMouseLock(InputAction.CallbackContext obj)
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        { 
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void OnDisable()
    {
        PitchAction.Disable();
        YawAction.Disable();
        RollAction.Disable();
        MouseMove.Disable();
        LockMouse.Disable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    private float WrapAngle(float angle)
    {
        angle %= 360;
        angle = angle > 180 ? angle - 360 : angle;
        return angle;
    }

    private Vector3 Flatten(Vector3 vector)
    {
        return new Vector3(vector.x, 0, vector.z);
    }

    /// <summary>
    /// Calculates a normalized roll response to the sideslip. [-1, 1].
    /// </summary>
    private float SideslipCorrectionRollTarget()
    {
        Vector3 projectedTargetVector = Flatten(targetVector);
        Vector3 projectedVelocity = Flatten(FlightModel.Velocity);
        // Target vector rotated 90 to the right. Used to measure how much of the velocity is perpendicular to the target vector.
        Vector3 targetRight = new(projectedTargetVector.z, 0, -projectedTargetVector.x);
        if (projectedVelocity.sqrMagnitude > 0.1f && targetRight.sqrMagnitude > 0.1f)
        {
            return Mathf.Asin(Vector3.Dot(projectedVelocity.normalized, targetRight.normalized)) / Mathf.PI * 2f;
        }
        else
        {
            return 0;
        }
    }

    private float CalculateRollTarget()
    {
        Quaternion inverse = Quaternion.Inverse(FlightModel.transform.rotation);
        Vector3 localTargetVector = inverse * targetVector;
        Vector3 cross = Vector3.Cross(Vector3.forward, localTargetVector);
        float rollTowards = Vector3.SignedAngle(Vector3.left, cross, Vector3.forward);

        // Normalize the roll towards value to [-1, 1]
        rollTowards /= 180f;

        // Reduces the roll when the nose is pointed in roughly the correct direction 
        rollTowards *= cross.magnitude;
        float sideslipCorrection = SideslipCorrectionRollTarget();

        return Mathf.Clamp(sideslipCorrection * SideslipCorrectionRollGain + rollTowards * RollTowardsGain, -RollLimit, RollLimit);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 input;
        input.x = PitchAction.ReadValue<float>();
        input.y = YawAction.ReadValue<float>();
        input.z = RollAction.ReadValue<float>();


        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 delta = MouseMove.ReadValue<Vector2>();
            targetVector = Quaternion.AngleAxis(delta.x * 0.1f, ShipCamera.transform.up) * targetVector;
            targetVector = Quaternion.AngleAxis(delta.y * 0.1f, -ShipCamera.transform.right) * targetVector;
            targetVector.Normalize();
        }
        Vector3 targetScreenPosition = ShipCamera.WorldToScreenPoint(ShipCamera.transform.position + targetVector);
        Aimpoint.position = targetScreenPosition;

        Quaternion inverse = Quaternion.Inverse(FlightModel.transform.rotation);
        Vector3 localTargetVector = inverse * targetVector;
        Vector3 cross = Vector3.Cross(Vector3.forward, localTargetVector);

        float rollTarget = CalculateRollTarget();
        float roll = -WrapAngle(FlightModel.transform.eulerAngles.z) / 180 * RollRecoveryGain + rollTarget;

        Vector3 controlVector = new Vector3(cross.x, cross.y, roll) + input;
        controlVector.Scale(ControlGain);
        controlVector.x = Mathf.Clamp(controlVector.x, -1, 1);
        controlVector.y = Mathf.Clamp(controlVector.y, -1, 1);
        controlVector.z = Mathf.Clamp(controlVector.z, -1, 1);

        previousControlVector = Vector3.MoveTowards(previousControlVector, controlVector, Time.deltaTime * 10);

        FlightModel.ApplyControl(previousControlVector);
    }
}
