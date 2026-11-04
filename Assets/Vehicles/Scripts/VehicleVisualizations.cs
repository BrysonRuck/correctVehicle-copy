using UnityEngine;

public class VehicleVisualizations : MonoBehaviour
{
    [Header("Vehicle")]
    [SerializeField] Vehicle vehicle;

    [Header("Toggles")]
    [SerializeField] bool showVisualizations = true;
    [SerializeField] bool showSuspensionForces = true;
    [SerializeField] bool showGripForces = true;
    [SerializeField] bool showDriveForces = true;
    [SerializeField] bool showHubBasis = false;

    [Header("Scale")]
    [SerializeField] float forceVisualizationScale = 0.001f;
    [SerializeField] float basisVisualizationScale = 0.5f;

    [Header("Colors")]
    [SerializeField] Color suspensionForceColor = Color.green;
    [SerializeField] Color gripForceColor = Color.red;
    [SerializeField] Color driveForceColor = Color.blue;
    [SerializeField] Color hubUpColor = Color.green;
    [SerializeField] Color hubForwardColor = Color.blue;
    [SerializeField] Color hubRightColor = Color.red;

    void OnDrawGizmos()
    {
        if (!Application.isPlaying || !showVisualizations) return;

        foreach (VehicleHubData hub in vehicle.Hubs)
        {
            Transform hubTransform = hub.transform;
            Vector3 hubPosition = hubTransform.position;

            if (showSuspensionForces)
            {
                Gizmos.color = suspensionForceColor;
                Gizmos.DrawRay(hubPosition, hub.SuspensionForce * forceVisualizationScale);
            }

            if (showGripForces)
            {
                Gizmos.color = gripForceColor;
                Gizmos.DrawRay(hubPosition, hub.GripForce * forceVisualizationScale);
            }

            if (showDriveForces)
            {
                Gizmos.color = driveForceColor;
                Gizmos.DrawRay(hubPosition, hub.DriveForce * forceVisualizationScale);
            }

            if (showHubBasis)
            {
                Gizmos.color = hubUpColor;
                Gizmos.DrawRay(hubPosition, hubTransform.up * basisVisualizationScale);

                Gizmos.color = hubForwardColor;
                Gizmos.DrawRay(hubPosition, hubTransform.forward * basisVisualizationScale);

                Gizmos.color = hubRightColor;
                Gizmos.DrawRay(hubPosition, hubTransform.right * basisVisualizationScale);
            }
        }
    }
}
