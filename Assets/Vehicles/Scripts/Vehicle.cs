using UnityEngine;
using UnityEngine.InputSystem;

/*
   A raycast car: one Rigidbody pushed by four hubs, one at each corner.

   Each physics step, every hub casts a ray down from itself to find the road. If it hits, the hub
   works out three forces, each along one of its own axes:
     suspension  along up       holds this corner off the road (a spring plus a damper)
     grip        along right    pushes against the tire sliding sideways
     drive       along forward  pushes the car where the hub points
   It stores them on the hub (hub.SuspensionForce and so on) for VehicleVisualizations to draw,
   then pushes the car once with their sum, at the hub's position. Pushing at a corner instead of
   the center is what tilts and turns the body; there is no rotation code.

   Steering adds no force. Update turns the front hubs, which turns their right and forward, and
   grip does the turning.

   Every force goes in FixedUpdate. Physics moves the car in its own steps, not once per frame, and
   FixedUpdate runs right before each step, so every step gets exactly one push.
*/
public class Vehicle : MonoBehaviour
{
    [Header("Car")]
    [SerializeField] Rigidbody carRigidBody;
    [SerializeField] VehicleHubData[] hubDataList;

    [Header("Suspension")]
    [SerializeField] float suspensionRestDist = 1f;
    [SerializeField] float raycastLength = 0.5f;
    [SerializeField] float stiffness = 30000f;
    [SerializeField] float damping = 1000f;
    [SerializeField] LayerMask groundLayers = ~0;

    [Header("Steering")]
    [SerializeField] float maxSteerAngle = 30f;
    [SerializeField] float steeringSpeed = 5f;

    [Header("Grip")]
    [SerializeField] float gripStrength = 750f;
    [SerializeField] float gripMultiplier = 2f;

    [Header("Drive")]
    [SerializeField] float driveForce = 2000f;
    [SerializeField] float topSpeed = 30f;
    [SerializeField] AnimationCurve engineCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    float _driveInput;

    public VehicleHubData[] Hubs => hubDataList;

    void Update()
    {
        _driveInput = 0f;
        if (Keyboard.current.wKey.isPressed) _driveInput += 1f;
        if (Keyboard.current.sKey.isPressed) _driveInput -= 1f;

        float steerInput = 0f;
        if (Keyboard.current.dKey.isPressed) steerInput += 1f;
        if (Keyboard.current.aKey.isPressed) steerInput -= 1f;


        //updated vehicle steering, so it's smooth
        foreach (VehicleHubData hub in hubDataList)
        {
            if (hub.Steers)
            {
                Quaternion targetRotation = Quaternion.Euler(0f, steerInput * maxSteerAngle, 0f);

                //slerp interpolates between current and target rotation
                hub.transform.localRotation = Quaternion.Slerp(
                    hub.transform.localRotation,
                    targetRotation,
                    steeringSpeed * Time.deltaTime
                );
            }
        }
    }
    //change
    void FixedUpdate()
    {
        foreach (VehicleHubData hub in hubDataList)
        {
            // TODO Part 1, setup: zero all three hub forces, then cast a ray along the hub's down.
            // On a miss, skip to the next hub with no forces left from the previous step.
            // TODO Part 2, spring: set the wheel visual on the road, then push along the hub's up with the spring.
            // TODO Part 3, damper: also resist how fast this hub moves along its up.

            hub.SuspensionForce = hub.DriveForce = hub.GripForce = Vector3.zero;

            Ray groundRay = new Ray(hub.transform.position, -hub.transform.up);
            if (!Physics.Raycast(groundRay, out RaycastHit groundHit, raycastLength, groundLayers))
            {
                hub.wheelVisual.transform.position = hub.transform.position - hub.transform.up * raycastLength;

                hub.wheelSpinSpeed = Mathf.Lerp(hub.wheelSpinSpeed, 0f, 2f * Time.fixedDeltaTime);

                hub.wheelVisual.Rotate(
                    hub.wheelSpinSpeed * Time.fixedDeltaTime,
                    0f,
                    0f,
                    Space.Self
                );

                continue;
            }
            Vector3 hubVelocity = carRigidBody.GetPointVelocity(hub.transform.position);

            float displacement = groundHit.distance - suspensionRestDist;
            float resotringForce = -stiffness * displacement;
            float suspensionSpeed = Vector3.Dot(hub.transform.up,hubVelocity);
            float dampingForce = -damping * suspensionSpeed;
            hub.SuspensionForce = (resotringForce + dampingForce) * hub.transform.up;

            carRigidBody.AddForceAtPosition(hub.SuspensionForce, hub.transform.position);







            float speed = Vector3.Dot(carRigidBody.linearVelocity, transform.forward);
            float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(speed) / topSpeed);

            float enginePower = engineCurve.Evaluate(normalizedSpeed);

            hub.DriveForce = hub.transform.forward * (_driveInput * driveForce * enginePower);




            // TODO Part 4, drive: push along the hub's forward.



            // TODO Part 5, wheels: find the distance rolled this step from the hub's forward speed.
            // Divide by the wheel circumference for revolutions, then multiply by 360 for degrees and spin the wheel visual.
            float rollingSpeed = Vector3.Dot(hubVelocity, hub.transform.forward);
            float wheelCircumference = 2f * Mathf.PI * hub.WheelRadius;

            hub.wheelSpinSpeed = rollingSpeed / wheelCircumference * 360f;

            hub.wheelVisual.Rotate(
                hub.wheelSpinSpeed * Time.fixedDeltaTime,
                0f,
                0f,
                Space.Self
            );

            // TODO Part 6, grip and steering: push against how fast this hub slides along its right.
            // gripStrength affects how strong the anti-slide force is (a damper turned sideways).
            float slippingSpeed = Vector3.Dot(hubVelocity, hub.transform.right);
            float gripForce = -slippingSpeed * gripStrength;
            float gripLimit = hub.SuspensionForce.magnitude * gripMultiplier;
            gripForce = Mathf.Clamp(gripForce, -gripLimit, gripLimit);
            hub.GripForce = gripForce * hub.transform.right;

            Vector3 totalForce = hub.SuspensionForce + hub.DriveForce + hub.GripForce;
            carRigidBody.AddForceAtPosition(totalForce, hub.transform.position);

            hub.wheelVisual.transform.position = groundHit.point + hub.WheelRadius * hub.transform.up;
        }
    }
}
