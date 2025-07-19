using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerSFX))]
public class PlayerController : MonoBehaviour
{
    // Existing fields
    public bool autoLedgeTarget = true;
    public float grabTime = 0.7f;
    [Header("Movement Speeds")]
    public float sprintSpeed = 4f;
    public float runSpeed = 3.36f;
    public float walkSpeed = 1.44f;
    public float stairSpeed = 2f;
    public float swimSpeed = 2f;
    public float treadSpeed = 1.2f;
    public float slideSpeed = 2f;
    public float stealthSpeed = 1.4f;
    [Header("UnderwaterDamageTimes")]
    public float breathTime = 60;
    public float damageTime = 10;
    public Image breathBar;
    public Image breathBackground;
    [Header("Physics")]
    public float gravity = 9.81f;
    public float damageVelocity = 5f;
    public float damageRate = 12f;
    public float terminalVelocity = 12f;
    [Header("Jump Speeds")]
    public float jumpYVel = 5f;
    public float jumpZBoost = 0.8f;
    [Header("IK Settings")]
    public float footYOffset = 0.1f;
    [Header("Offsets")]
    public float grabForwardOffset = 0.11f;
    public float grabUpOffset = 1.56f;
    public float hangForwardOffset = 0.11f;
    public float hangUpOffset = 1.975f;
    [Header("Axis Names")]
    public string right = "Horizontal";
    public string forward = "Vertical";

    [Header("References")]
    public CameraController camController;
    public GameObject larad;  // Referência ao GameObject que contém as armas (LARAD)
    public Transform waistBone;
    public Transform rightFootIK;
    public Transform leftFootIK;
    public Transform palmLocation;
    public GameObject pistolLHand;
    public GameObject pistolRHand;
    public GameObject pistolLLeg;
    public GameObject pistolRLeg;
    [Header("Ragdoll")]
    public Rigidbody[] ragRigidBodies;
    [Header("Combat")]
    public RectTransform lockOnCanvas;
    public Transform playerTarget;
    public float damage = 10f;
    public float shotDelay = .2f;
    public float rotationSpeed = 5f; // Added field for rotation speed

    [Header("WeaponSettings")]
    public GameObject currentWeapon;        // The currently equipped weapon
    public WeaponItem currentWeaponItem;    // Reference to the currently equipped WeaponItem
    public Animator anim;                   // Animator for the player
    private AmmoManager ammoManager; // Declare the AmmoManager reference
    
    [Header("Others")]
    // Existing fields (continued)
    private bool isGrounded = true;
    private bool isSliding = false;
    private bool isFootIK = false;
    private bool holdRotation = false;
    private bool forceWaistRotation = false;
    public bool locked = false;
    private float combatAngle = 0f;
    private float groundDistance = 0f;
    private float groundAngle = 0f;
    [HideInInspector]
    public bool isMovingAuto = false;
    private float targetAngle = 0f;
    private float targetSpeed = 0f;
    private float curBreathTime = 60f;

    private StateMachine<PlayerController> stateMachine;
    [HideInInspector]
    public CharacterController charControl;
    [HideInInspector]
    public PlayerInput playerInput;
    [HideInInspector]
    public Transform currentEnemyTarget;
    private Transform cam;
    private PlayerStats playerStats;
    private PlayerSFX playerSFX;
    private Weapon[] pistols = new Weapon[2];
    private Weapon[] auxiliaryWeapon;
    private Transform waistTarget;
    private Quaternion waistRotation;
    private Vector3 velocity;
    [HideInInspector]
    public Vector3 slopeDirection;
    [HideInInspector]
    public bool useRootMotion = true;
    private RaycastHit groundHit;
    private GameObject pushObject;
    private float pushPullState = 0;
    private bool isBack = false;
    private bool rotRock = false;
    private bool animationTriggered = false;

    [HideInInspector]
    public bool isBackJump = false;

    private void Awake()
    {
        DisableRagdoll();
    }

    private void Start()
    {
        charControl = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        cam = camController.GetComponentInChildren<Camera>().transform;
        anim = GetComponent<Animator>();
        playerSFX = GetComponent<PlayerSFX>();
        pistols[0] = pistolLHand.GetComponent<Weapon>();
        pistols[1] = pistolRHand.GetComponent<Weapon>();
        playerStats = GetComponent<PlayerStats>();
        velocity = Vector3.zero;
        stateMachine = new StateMachine<PlayerController>(this);
        SetUpStateMachine();
        curBreathTime = breathTime;
        playerStats.HideCanvas();
        // Get the AmmoManager component from the player
        ammoManager = GetComponent<AmmoManager>();
    }

    private void SetUpStateMachine()
    {
        stateMachine.AddState(new Empty());
        stateMachine.AddState(new Locomotion());
        stateMachine.AddState(new Combat());
        stateMachine.AddState(new CombatJumping());
        stateMachine.AddState(new Climbing());
        stateMachine.AddState(new Freeclimb());
        stateMachine.AddState(new Drainpipe());
        stateMachine.AddState(new Ladder());
        stateMachine.AddState(new Crouch());
        stateMachine.AddState(new Dead());
        stateMachine.AddState(new InAir());
        stateMachine.AddState(new Jumping());
        stateMachine.AddState(new Swimming());
        stateMachine.AddState(new Grabbing());
        stateMachine.AddState(new AutoGrabbing());
        stateMachine.AddState(new MonkeySwing());
        stateMachine.AddState(new HorPole());
        stateMachine.AddState(new Sliding());
        stateMachine.AddState(new Stealth());
        stateMachine.GoToState<Locomotion>();
    }

    private void Update()

    {
        if (RingMenu.isPaused)
        {
            anim.speed = 0f;
            return;
        }
        else
        {
            anim.speed = 1f;
        }

        if (Input.GetKeyDown(playerInput.stealth) && isGrounded)
        {
            stateMachine.GoToState<Stealth>();
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("SwimSubmerged") || anim.GetCurrentAnimatorStateInfo(0).IsName("TreadSubmerged") || anim.GetCurrentAnimatorStateInfo(0).IsName("UnderwaterPick"))
        {
            curBreathTime -= Time.deltaTime;
            if (curBreathTime < -damageTime / playerStats.maxHealth)
            {
                curBreathTime = 0f;
                playerStats.Health -= 1;
            }
            breathBar.gameObject.SetActive(true);
            breathBackground.gameObject.SetActive(true);
            breathBar.rectTransform.sizeDelta = new Vector2(120 * curBreathTime / breathTime, 10);
        }
        else if (curBreathTime < breathTime)
        {
            curBreathTime += Time.deltaTime;
            breathBar.gameObject.SetActive(true);
            breathBackground.gameObject.SetActive(true);
            breathBar.rectTransform.sizeDelta = new Vector2(120 * curBreathTime / breathTime, 10);
        }
        else
        {
            breathBackground.gameObject.SetActive(false);
            breathBar.gameObject.SetActive(false);
        }

        if (!locked)
        {
            UpdateAnimator();
            CheckForGround();
            stateMachine.Update();
        }

        if (locked)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                if (Input.GetAxis(playerInput.verticalAxis) > 0.7f && !animationTriggered)
                {
                    anim.SetTrigger("PushReady");
                    animationTriggered = true;
                }
                else if (Input.GetAxis(playerInput.verticalAxis) < -0.7f && !animationTriggered)
                {
                    anim.SetTrigger("PullReady");
                    animationTriggered = true;
                }
            }
            else
            {
                if (Input.GetAxis(playerInput.verticalAxis) < 0.7f && anim.GetCurrentAnimatorStateInfo(0).IsName("PushObject"))
                {
                    anim.SetTrigger("PushExit");
                    pushObject.transform.parent = null;
                    pushObject = null;
                    locked = false;
                    animationTriggered = false;
                }
                if (Input.GetAxis(playerInput.verticalAxis) > -0.7f && anim.GetCurrentAnimatorStateInfo(0).IsName("PullObject"))
                {
                    anim.SetTrigger("PullExit");
                    pushObject.transform.parent = null;
                    pushObject = null;
                    locked = false;
                    animationTriggered = false;
                }
            }
        }
        else if (Input.GetButtonDown("Fire") && anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            RaycastHit hitout;
            if (Physics.Raycast(transform.position + transform.forward * 0.1f + transform.up * 0.1f, transform.forward, out hitout, 1f))
            {
                if (hitout.collider.gameObject.CompareTag("Pushable"))
                {
                    pushObject = hitout.collider.gameObject;
                    pushObject.transform.parent = transform;
                    Input.ResetInputAxes();
                    locked = true;
                }
            }
        }

        if (charControl.enabled)
            charControl.Move((anim.applyRootMotion ? Vector3.Scale(velocity, Vector3.up) : velocity) * Time.deltaTime);

        // New code for weapon handling
        HandleWeaponUsage();
    }

    private void HandleWeaponUsage()
    {
        if (currentWeaponItem != null)
        {
            if (Input.GetButtonDown("Fire") && currentWeaponItem.currentAmmo > 0)
            {
                FireWeapon();
            }
        }
    }

    private void FireWeapon()
    {
        currentWeaponItem.currentAmmo--;
        anim.SetTrigger("Fire");

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 10f))
        {
            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(currentWeaponItem.damage);
            }
        }
    }

    public void ReloadWeapon()
    {
        if (currentWeaponItem != null)
        {
            currentWeaponItem.Reload();
        }
    }

    public void RotateTowards(Vector3 target, float rotationSpeed = 5f)
    {
         // Calcula a direção até o alvo, ignorando a componente Y
         Vector3 directionToTarget = (target - transform.position).normalized;
         directionToTarget.y = 0f; // Ignora a rotação vertical

         // Calcula a rotação desejada
         Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

        // Rotaciona suavemente em direção ao alvo
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void CheckForGround()
    {
        isGrounded = charControl.isGrounded && velocity.y <= 0.0f;
        anim.SetBool("isGrounded", isGrounded);

        groundDistance = 2f;
        groundAngle = 0f;

        Vector3 centerStart = transform.position + Vector3.up * 0.2f;

        if ((Physics.Raycast(centerStart, Vector3.down, out groundHit, GroundDistance)
            && !groundHit.collider.CompareTag("Water")))
        {
            groundDistance = transform.position.y - groundHit.point.y;
            groundAngle = UMath.GroundAngle(groundHit.normal);
        }

        anim.SetFloat("groundDistance", GroundDistance);
        anim.SetFloat("groundAngle", GroundAngle);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        stateMachine.SendMessage(hit);
    }

    public void DisableRagdoll()
    {
        foreach (Rigidbody rb in ragRigidBodies)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.gameObject.GetComponent<Collider>().enabled = false;
        }
    }

    public void EnableRagdoll()
    {
        foreach (Rigidbody rb in ragRigidBodies)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.gameObject.GetComponent<Collider>().enabled = true;
        }
    }

    private void LateUpdate()
    {
        if (forceWaistRotation)
        {
            waistBone.rotation = waistRotation;

            waistBone.rotation = Quaternion.Euler(
                waistBone.eulerAngles.x - 90f, waistBone.eulerAngles.y,
                waistBone.eulerAngles.z);
        }
    }

    public void AnimWait(float seconds)
    {
        StartCoroutine(StopDrop(seconds));
    }

    public void MoveWait(Vector3 point, Quaternion rotation, float tRate = 1f, float rRate = 1f)
    {
        StartCoroutine(MoveTo(point, rotation, tRate, rRate));
    }

    private IEnumerator StopDrop(float secs)
    {
        float startTime = Time.time;
        anim.SetBool("isWaiting", true);
        while (Time.time - startTime < secs)
        {
            yield return null;
        }
        anim.SetBool("isWaiting", false);
    }

    private IEnumerator MoveTo(Vector3 point, Quaternion rotation, float tRate = 1f, float rRate = 1f)
    {
        anim.applyRootMotion = false;

        velocity = Vector3.zero;

        float distance = Vector3.Distance(transform.position, point);
        float difference = Quaternion.Angle(transform.rotation, rotation);
        Vector3 direction = (point - transform.position).normalized;
        bool isNotOk = true;

        isMovingAuto = true;
        anim.SetBool("isWaiting", true);

        while (isNotOk)
        {
            isNotOk = false;

            if (Mathf.Abs(distance) > 0.05f)
            {
                isNotOk = true;
                transform.position = Vector3.Lerp(transform.position, point, tRate * Time.deltaTime);
                distance = Vector3.Distance(transform.position, point);
            }
            else
            {
                velocity = Vector3.zero;
            }

            if (Mathf.Abs(difference) > 5f)
            {
                isNotOk = true;
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rRate * Time.deltaTime);
                difference = Quaternion.Angle(transform.rotation, rotation);
            }

            yield return null;
        }

        transform.position = point;
        transform.rotation = rotation;
        velocity = Vector3.zero;

        isMovingAuto = false;
        anim.SetBool("isWaiting", false);
    }

    private void UpdateAnimator()
    {
        AnimatorStateInfo animState = anim.GetCurrentAnimatorStateInfo(0);
        float animTime = animState.normalizedTime <= 1.0f ? animState.normalizedTime
            : animState.normalizedTime % (int)animState.normalizedTime;

        anim.SetFloat("AnimTime", animTime);
    }

    public Vector3 RawTargetVector(float speed = 1f)
    {
        Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = cam.right;

        Vector3 targetVector = camForward * Input.GetAxisRaw(playerInput.verticalAxis)
            + camRight * Input.GetAxisRaw(playerInput.horizontalAxis);
        if (rotRock) targetVector = transform.forward;
        if (targetVector.magnitude > 1f)
            targetVector.Normalize();
        targetVector.y = 0f;
        targetVector *= speed;

        return targetVector;
    }

    public bool adjustingRot = false;

    public void MoveGrounded(float speed, bool pushDown = true, float smoothing = 10f)
    {
        Vector3 targetVector = RawTargetVector(speed);

        if (targetVector.magnitude < 0.3f)
            targetVector = Vector3.zero;

        velocity.y = 0f;

        AnimatorStateInfo animState = anim.GetCurrentAnimatorStateInfo(0);

        bool turning = animState.IsName("IdleTurns") || animState.IsName("RunTurns");

        targetAngle = Vector3.SignedAngle(transform.forward, targetVector.normalized, Vector3.up);
        targetSpeed = UMath.GetHorizontalMag(targetVector);

        holdRotation = Mathf.Abs(TargetAngle) > (animState.IsName("Idle") ? 80f : 170f) || turning;

        anim.SetFloat("SignedTargetAngle", TargetAngle, turning ? 1000000f : 0, Time.deltaTime);
        anim.SetFloat("TargetAngle", Mathf.Abs(TargetAngle), turning ? 1000000f : 0, Time.deltaTime);
        if(!locked) anim.SetFloat("TargetSpeed", TargetSpeed);

        if (UMath.GetHorizontalMag(velocity) < 0.1f)
        {
            if (!adjustingRot && targetVector.magnitude > 0.1f && Mathf.Abs(TargetAngle) > 5f)
            {
                adjustingRot = true;
                velocity = Mathf.Abs(TargetAngle) > 80f ? targetVector : transform.forward * 3f;

            }
            else if (UMath.GetHorizontalMag(targetVector) < 0.3f)
            {
                velocity = Vector3.zero;
            }
        }
        else if (Mathf.Abs(TargetAngle) > 36f)
        {
            adjustingRot = true;
        }

        if (turning)
            adjustingRot = false;

        if (!turning)
        {
            if (adjustingRot)
            {
                velocity = Vector3.Slerp(velocity, targetVector, Time.deltaTime * smoothing);
                if (Vector3.Angle(velocity, targetVector) < 5f)
                {
                    adjustingRot = false;
                }
            }
            else
            {
                velocity = targetVector;
            }
        }

        anim.SetFloat("Speed", UMath.GetHorizontalMag(!turning ? velocity : targetVector), 0.1f, Time.deltaTime);
        anim.SetFloat("Right", 0f);

        if (pushDown)
            velocity.y = -gravity;
    }

    public void MoveStrafeGround(float speed, bool pushDown = true, float smoothing = 10f)
    {
        Vector3 targetVector = RawTargetVector(speed);

        if (targetVector.magnitude < 0.3f)
            targetVector = Vector3.zero;

        velocity.y = 0f;

        AnimatorStateInfo animState = anim.GetCurrentAnimatorStateInfo(0);

        Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 pForward = Vector3.Scale(transform.forward, new Vector3(1, 0, 1)).normalized;

        targetAngle = Vector3.SignedAngle(transform.forward, targetVector.normalized, Vector3.up);
        combatAngle = Vector3.SignedAngle(camForward, velocity.normalized, Vector3.up);
        targetSpeed = UMath.GetHorizontalMag(targetVector);

        anim.SetFloat("SignedTargetAngle", targetAngle);
        anim.SetFloat("TargetAngle", 0f);
        anim.SetFloat("combatAngle", combatAngle);
        anim.SetFloat("TargetSpeed", targetSpeed);

        if (UMath.GetHorizontalMag(velocity) < 0.1f)
        {
            if (!adjustingRot && targetVector.magnitude > 0.1f && Mathf.Abs(targetAngle) > 5f)
            {
                adjustingRot = true;
                velocity = Mathf.Abs(TargetAngle) > 80f ? targetVector : transform.forward * 3f;

            }
            else if (UMath.GetHorizontalMag(targetVector) < 0.3f)
            {
                velocity = Vector3.zero;
            }
        }
        else if (Mathf.Abs(TargetAngle) > 36f)
        {
            adjustingRot = true;
        }

        if (adjustingRot)
        {
            velocity = Vector3.Slerp(velocity, targetVector, Time.deltaTime * smoothing);
            if (Vector3.Angle(velocity, targetVector) < 5f)
            {
                adjustingRot = false;
            }
        }
        else
        {
            velocity = targetVector;
        }

        anim.SetFloat("Speed", direction * UMath.GetHorizontalMag(velocity));
        anim.SetFloat("Right", 0f);

        if (pushDown)
            velocity.y = -gravity;
    }

    public void MoveFree(float speed, float smoothing = 16f, float maxTurnAngle = 20f)
    {
        Vector3 targetVector = cam.forward * Input.GetAxisRaw("Vertical")
            + cam.right * Input.GetAxisRaw("Horizontal");
        if (targetVector.magnitude > 1.0f)
            targetVector = targetVector.normalized;

        if (velocity.magnitude < 0.1f && targetVector.magnitude > 0f)
            velocity = transform.forward * 0.1f;

        if (Vector3.Angle(velocity.normalized, targetVector) > maxTurnAngle)
        {
            Vector3 direction = Vector3.Cross(velocity.normalized, targetVector);
            targetVector = Quaternion.AngleAxis(maxTurnAngle, direction) * velocity.normalized;
        }

        targetVector *= speed;

        velocity = Vector3.Slerp(velocity, targetVector, Time.deltaTime * smoothing);

        anim.SetFloat("Speed", velocity.magnitude);
        anim.SetFloat("TargetSpeed", targetVector.magnitude);
    }

    public void MoveInDirection(float speed, Vector3 dir, float smoothing = 8f, float maxTurnAngle = 24f)
    {
        Vector3 targetVector = dir;

        if (velocity.magnitude < 0.1f && targetVector.magnitude > 0f)
            velocity = transform.forward * 0.1f;

        if (Vector3.Angle(velocity.normalized, targetVector) > maxTurnAngle)
        {
            Vector3 direction = Vector3.Cross(velocity.normalized, targetVector);
            targetVector = Quaternion.AngleAxis(maxTurnAngle, direction) * velocity.normalized;
        }

        targetVector *= speed;

        velocity = Vector3.Slerp(velocity, targetVector, Time.deltaTime * smoothing);

        anim.SetFloat("Speed", velocity.magnitude);
        anim.SetFloat("TargetSpeed", targetVector.magnitude);
    }

    public void RotateToCamera()
    {
        if (UMath.GetHorizontalMag(velocity) > 0.1f)
        {
            Quaternion target = Quaternion.LookRotation(cam.transform.forward, Vector3.up);
            target = Quaternion.Euler(0f, target.eulerAngles.y, 0f);
            transform.rotation = target;
        }
    }

    public void RotateToVelocityGround(float smoothing = 0f)
    {
        if (UMath.GetHorizontalMag(velocity) > 0.3f && !holdRotation)
        {
            Quaternion target = Quaternion.Euler(0.0f, Mathf.Atan2(velocity.x, velocity.z) * Mathf.Rad2Deg, 0.0f);
            if (smoothing == 0f)
                transform.rotation = target;
            else
                transform.rotation = Quaternion.Slerp(transform.rotation, target, smoothing * Time.deltaTime);
        }
    }

    int direction = 1;
    bool adjustRotCombat = false;

    public void RotateToVelocityStrafe(float smoothing = 8f)
    {
        if (UMath.GetHorizontalMag(velocity) > 0.3f && !holdRotation)
        {
            float theAngle = Mathf.Atan2(velocity.x, velocity.z) * Mathf.Rad2Deg;

            if (combatAngle < -47f || combatAngle > 137f)
            {
                if (direction != -1)
                    adjustRotCombat = true;
                direction = -1;
            }
            else if (combatAngle > -43f && combatAngle < 133f)
            {
                if (direction != 1)
                    adjustRotCombat = true;
                direction = 1;
            }

            if (direction == -1)
                theAngle += 180f;

            Quaternion target = Quaternion.Euler(0.0f, theAngle, 0.0f);
            if (!adjustRotCombat)
            {
                transform.rotation = target;
            }
            else
            {
                if (Mathf.Abs(Quaternion.Angle(target, transform.rotation)) > 10f)
                    transform.rotation = Quaternion.Lerp(transform.rotation, target, smoothing * Time.deltaTime);
                else
                    adjustRotCombat = false;
            }
        }
    }

    public void RotateToVelocity(float smoothing = 0f)
    {
        if (velocity.magnitude > 0.1f)
        {
            if (smoothing == 0f)
                transform.rotation = Quaternion.LookRotation(velocity);
            else
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(velocity),
                    smoothing * Time.deltaTime);
        }
    }

    public void RotateToTarget(Vector3 target)
    {
        Vector3 direction = Vector3.Scale((target - transform.position), new Vector3(1.0f, 0.0f, 1.0f));
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    public void ApplyGravity(float amount)
    {
        if(velocity.y > -terminalVelocity)
            velocity.y -= amount * Time.deltaTime;
    }

    public void FireRightPistol()
    {
        pistols[1].Fire();
    }

    public void FireLeftPistol()
    {
        pistols[0].Fire();
    }

    public void MinimizeCollider(float size = 0f)
    {
        charControl.radius = size;
    }

    public void MaximizeCollider()
    {
        charControl.radius = 0.2f;
    }

    public void DisableCharControl()
    {
        charControl.enabled = false;
    }

    public void EnableCharControl()
    {
        charControl.enabled = true;
    }

    public void Lock(float duration)
    {
        StartCoroutine(LockCoroutine(duration));
    }

    private IEnumerator LockCoroutine(float duration)
    {
        locked = true;
        yield return new WaitForSeconds(duration);
        locked = false;
    }

    public StateMachine<PlayerController> StateMachine
    {
        get { return stateMachine; }
    }

    public CharacterController Controller
    {
        get { return charControl; }
    }

    public Transform Cam
    {
        get { return cam; }
    }

    public Transform WaistTarget
    {
        get { return waistTarget; }
        set { waistTarget = value; }
    }

    public Quaternion WaistRotation
    {
        get { return WaistRotation; }
        set { waistRotation = value; }
    }

    public Animator Anim
    {
        get { return anim; }
    }

    public PlayerSFX SFX
    {
        get { return playerSFX; }
    }

    public PlayerStats Stats
    {
        get { return playerStats; }
    }

    public bool Grounded
    {
        get { return isGrounded; }
    }

    public bool IsFootIK
    {
        get { return isFootIK; }
        set { isFootIK = value; }
    }

    public bool ForceWaistRotation
    {
        get { return forceWaistRotation; }
        set { forceWaistRotation = value; }
    }

    public float CombatAngle
    {
        get { return combatAngle; }
    }

    public float GroundDistance
    {
        get { return groundDistance; }
    }

    public float GroundAngle
    {
        get { return groundAngle;  }
    }

    public float TargetAngle
    {
        get { return targetAngle; }
    }

    public float TargetSpeed
    {
        get { return targetSpeed;  }
    }

    public Vector3 Velocity
    {
        get { return velocity; }
        set
        {
            velocity = value;
        }
    }

    public RaycastHit GroundHit
    {
        get { return groundHit; }
    }
}
