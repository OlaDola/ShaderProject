using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSController : PortalTraveller {

    public float walkSpeed = 3;
    public float runSpeed = 6;
    public float smoothMoveTime = 0.1f;
    public float jumpForce = 5;
    public float gravity = 18;

    public bool lockCursor;
    public float mouseSensitivity = 10;
    public Vector2 pitchMinMax = new Vector2 (-40, 85);
    public float rotationSmoothTime = 0.1f;

    CharacterController controller;
    
    [SerializeField]
    GameObject playerParent;

    Rigidbody rb;

    Camera cam;
    public float yaw;
    public float pitch;
    float smoothYaw;
    float smoothPitch;

    float yawSmoothV;
    float pitchSmoothV;
    float verticalVelocity;
    Vector3 velocity;
    Vector3 smoothV;
    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;

    bool jumping;
    float lastGroundedTime;
    bool disabled;

    public bool doTeleport = true;

    [SerializeField] IsGrounded groundCheck;

    void Start () {
        cam = Camera.main;
        if (lockCursor) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // controller = playerParent.GetComponent<CharacterController> ();
        rb = playerParent.GetComponent<Rigidbody> ();

        yaw = transform.eulerAngles.y;
        pitch = cam.transform.localEulerAngles.x;
        smoothYaw = yaw;
        smoothPitch = pitch;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Break();
        }
        if (Input.GetKeyDown(KeyCode.O)) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            disabled = !disabled;
        }
        if (Input.GetKeyDown(KeyCode.L)) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (IsGrounded() && Input.GetKeyDown(KeyCode.Space)) {
            jumping = true;
        }
    }

    void FixedUpdate () {

        if (disabled) {
            return;
        }

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        Vector3 inputDir = new Vector3(input.x, 0, input.y).normalized;
        Vector3 worldInputDir = transform.TransformDirection(inputDir);

        float currentSpeed = (Input.GetKey(KeyCode.LeftShift)) ? runSpeed * playerParent.transform.localScale.x : walkSpeed * playerParent.transform.localScale.x;
        
        Vector3 targetVelocity = worldInputDir * currentSpeed;
        velocity = Vector3.SmoothDamp(velocity, targetVelocity, ref smoothV, smoothMoveTime);

        if (Vector3.Distance(transform.position, velocity * Time.fixedDeltaTime) < 0.001f) {
            velocity = Vector3.zero;
        }

        if(jumping){
            rb.AddRelativeForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumping = false;
        }

        Vector3 moveDelta = velocity * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + moveDelta);

        float mX = Input.GetAxisRaw("Mouse X");
        float mY = Input.GetAxisRaw("Mouse Y");

        float mMag = Mathf.Sqrt(mX * mX + mY * mY);
        if (mMag > 5) {
            mX = 0;
            mY = 0;
        }

        yaw += mX * mouseSensitivity;
        yaw = NormalizeAngle(yaw);
        pitch -= mY * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);
        smoothPitch = Mathf.SmoothDampAngle(smoothPitch, pitch, ref pitchSmoothV, rotationSmoothTime);
        smoothYaw = Mathf.SmoothDampAngle(smoothYaw, yaw, ref yawSmoothV, rotationSmoothTime);

        transform.localEulerAngles = Vector3.up * smoothYaw;
        cam.transform.localEulerAngles = Vector3.right * smoothPitch;
    }

    bool IsGrounded() {
       return groundCheck.isGrounded;
    }

    public override void Teleport (Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot, Vector3 scale) {
        if(!doTeleport)
            return;
        
        Quaternion portalRotationDifference = toPortal.rotation * Quaternion.Inverse(fromPortal.rotation);
        // cam.transform.rotation = portalRotationDifference * cam.transform.rotation;
        playerParent.transform.rotation = portalRotationDifference * playerParent.transform.rotation;
        playerParent.transform.position = pos;
        playerParent.transform.localScale = Vector3.Scale(playerParent.transform.localScale, scale);


        Vector3 eulerRot = rot.eulerAngles;
        float delta = Mathf.DeltaAngle (smoothYaw, eulerRot.y);
        
        // transform.localEulerAngles = Vector3.up * smoothYaw;
        velocity = toPortal.TransformVector (fromPortal.InverseTransformVector (velocity));
        Physics.SyncTransforms ();
    }
    float NormalizeAngle(float angle) {
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }
}