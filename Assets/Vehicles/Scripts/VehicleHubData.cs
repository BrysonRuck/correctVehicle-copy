using UnityEngine;

public class VehicleHubData : MonoBehaviour
{
    [Header("Visual")]
    public Transform wheelVisual;

    [Tooltip("The wheel's radius in meters, measured on its mesh. It sets how fast the wheel spins and how high it sits.")]
    [SerializeField] float wheelRadius = 0.4f;

    [Header("Steering")]
    [SerializeField] bool steers;

    public float WheelRadius => wheelRadius;
    public bool Steers => steers;
    public float wheelSpinSpeed;
    public Vector3 SuspensionForce { get; set; }
    public Vector3 GripForce { get; set; }
    public Vector3 DriveForce { get; set; }
}
