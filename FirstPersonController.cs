using DG.Tweening;
using System.Collections;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public bool CanMove = true;
    public bool CanLook = true;

    private bool isSprinting;
    private bool ShouldJump => Input.GetKeyDown(JumpKey) && characterController.isGrounded;
    private bool ShouldCrouch => Input.GetKeyDown(CrouchKey) && !duringCrouchAnimation && characterController.isGrounded;
    private bool ShouldProne => Input.GetKeyDown(ProneKey) && !duringProneAnimation && characterController.isGrounded;
    private Vector3 CurrentPosition;

    [Header("Functional Options")]
    [SerializeField] private bool CanSprint = true;
    [SerializeField] private bool UseSprint = true;
    [SerializeField] private bool CanJump = true;
    [SerializeField] private bool UseJump = true;
    [SerializeField] private bool CanCrouch = true;
    [SerializeField] private bool UseCrouch = true;
    [SerializeField] private bool CanProne = true;
    [SerializeField] private bool UseProne = true;
    [SerializeField] private bool CanDash = true;
    [SerializeField] private bool UseDash = true;
    [SerializeField] private bool CanSlide = true;
    [SerializeField] private bool UseSlide = true;
    [SerializeField] private bool CanClimb = true;
    [SerializeField] private bool CanInteract = true;
    [SerializeField] private bool UseStamina = true;
    [SerializeField] public bool UseHeadbob = true;
    [SerializeField] private bool TiltCam = true;
    [SerializeField] private bool UseFootsteps = true;
    [SerializeField] private bool DetectFallDamage = true;

    [Header("Controls")]
    [SerializeField] public KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode JumpKey = KeyCode.Space;
    [SerializeField] private KeyCode CrouchKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode ProneKey = KeyCode.C;
    [SerializeField] private KeyCode DashKey = KeyCode.RightShift;
    [SerializeField] private KeyCode InteractKey = KeyCode.E;

    [Header("Movement Parameters")]
    [SerializeField] public float WalkSpeed = 3.0f;
    [SerializeField] public float SprintSpeed = 6.0f;
    [SerializeField] private float CrouchSpeed = 1.5f;
    [SerializeField] private float Gravity = 1.5f;
    private float MaxSprintHold = 0.5f;
    private float SprintHoldTimer;
    private bool SprintTap;
    private bool SprintHold;

    [Header("Look Parameters")]
    [SerializeField, Range(0, 10)] private float LookSpeedX = 2.0f;
    [SerializeField, Range(0, 10)] private float LookSpeedY = 2.0f;
    [SerializeField, Range(0, 100)] private float UpperLookLimit = 80.0f;
    [SerializeField, Range(0, 100)] private float LowerLookLimit = 80.0f;

    [Header("FallDetection")]
    [SerializeField] private float MaxAirTime;
    [SerializeField] private float CurrentAirTime;
    [SerializeField] private float DamageToBeDealt;
    [SerializeField] private float DamageIncrement = 1f;

    [Header("Jump Parameters")]
    [SerializeField] private float JumpForce = 8.0f;
    [SerializeField] private float JumpGravity = 30.0f;
    [SerializeField] private bool CanDoubleJump;
    private bool DoubleJump;
    [SerializeField] private bool UseCoyoteTime;
    [SerializeField] private float CoyoteTime = 0.2f;
    private float CoyoteTimeCounter;
    [SerializeField] private bool UseJumpBuffer;
    [SerializeField] private float JumpBufferTime = 0.2f;
    private float BufferTimeCounter;

    [Header("Crouch Parameters")]
    [SerializeField] private float CrouchHeight = 0.5f;
    [SerializeField] private float StandingHeight = 2f;
    [SerializeField] private float TimeToCrouch = 0.25f;
    [SerializeField] private Vector3 CrouchingCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 StandingCenter = new Vector3(0, 0, 0);
    public bool isCrouching;
    private bool duringCrouchAnimation;

    [Header("Prone Parameters")]
    [SerializeField] private float ProneHeight = 0.25f;
    [SerializeField] private float StandingHeights = 2f;
    [SerializeField] private float TimeToProne = 0.25f;
    [SerializeField] private Vector3 ProningCenter = new Vector3(0, 0.25f, 0);
    [SerializeField] private Vector3 StandingCenters = new Vector3(0, 0, 0);
    public bool isProning;
    private bool duringProneAnimation;

    [Header("Dash Parameters")]
    [SerializeField] private float DashSpeed = 20f;
    [SerializeField] private float DashTime = 0.25f;
    [SerializeField] private float DashDelay = 0.25f;
    [SerializeField] private bool UseDashInAir = true;
    [SerializeField] private float NormalJumpGravity = 30f;
    [SerializeField] private float AirDashJumpGravity = 0f;
    [SerializeField] private bool UseDashOnGround = true;
    [SerializeField] private bool UseDashDuringCrouch = true;
    [SerializeField] private bool UseDashDuringProne = true;
    [SerializeField] private bool UseFOVEffect = true;
    private float DashCD;
    private bool isDashing;

    [Header("Slide Parameters")]
    [SerializeField] private float SlideSpeed = 100f;
    [SerializeField] private float SlideDistance = 5f;
    [SerializeField] private float SlideDelay = 0.25f;
    [SerializeField] private bool UseSlideDuringCrouch = true;
    [SerializeField] private bool UseSlideDuringProne = true;
    private float SlideCD;
    private float isSliding;

    [Header("Health Parameters")]
    [SerializeField] private float MaxHealth = 100f;
    [SerializeField] public float CurrentHealth = 100f;

    [Header("Stamina Parameters")]
    [SerializeField] public float MaxStamina = 100f;
    [SerializeField] public float CurrentStamina = 100f;
    [SerializeField] private float StaminaRegenrationAmount = 2f;
    [SerializeField] private float StaminaRegenerationDelay = 1.5f;
    [Space]
    [SerializeField] private bool UseStaminaOnSprint = true;
    [SerializeField] private float SprintDepletionAmount = 2f;
    [SerializeField] private float NormalSprintSpeed;
    [SerializeField] private float NormalSprintBob;
    [SerializeField] private float NormalSprintMultiplier;
    [SerializeField] private float DecreasedSprintSpeed = 3f;
    [SerializeField] private float DecreasedSprintBob;
    [SerializeField] private float DecreasedSprintMultiplier = 1;
    [Space]
    [SerializeField] private bool UseStaminaOnDash = true;
    [SerializeField] private float DashDepletionAmount = 2f;
    [SerializeField] private float NormalDashSpeed;
    [SerializeField] private float DecreasedDashSpeed = 100f;
    [Space]
    [SerializeField] private bool UseStaminaOnSlide = true;
    [SerializeField] private float SlideDepletionAmount = 2f;
    [SerializeField] private float NormalSlideSpeed;
    [SerializeField] private float DecreasedSlideSpeed = 50f;
    [Space]
    [SerializeField] private bool UseStaminaOnJump = true;
    [SerializeField] private float JumpDepletionAmount = 10f;
    [SerializeField] private float NormalJumpForce = 25f;
    [SerializeField] private float DecreasedJumpForce = 40f;

    private float DepletionAmount;
    [HideInInspector] public bool StaminaInUse;

    [Header("Headbob Parameters")]
    [SerializeField] private float WalkBobSpeed = 14f;
    [SerializeField] private float WalkBobAmount = 0.5f;
    [SerializeField] private float SprintBobSpeed = 20f;
    [SerializeField] private float SprintBobAmount = 1f;
    [SerializeField] private float CrouchBobSpeed = 8f;
    [SerializeField] private float CrouchBobAmount = 0.25f;

    private float defaultYPos;
    private float timer;

    public Camera playerCamera;
    public Animator CameraAnim;
    private CharacterController characterController;

    private Vector3 MoveDirection;
    private Vector2 CurrentInput;

    private float rotationX = 0f;

    [Header("Interaction Parameters")]
    [SerializeField] private Vector3 InteractionRayPoint = default;
    [SerializeField] private float InteractionDistance = default;
    [SerializeField] private LayerMask InteractionLayer = default;
    //private Interctable currentInteractable;

    [Header("Footsteps Parameters")]
    [SerializeField] public float baseStepSpeed = 0.5f;
    [SerializeField] private float crouchStepMultiplier = 1.5f;
    [SerializeField] public float sprintStepMultiplier = 0.6f;
    [SerializeField] private AudioSource footstepAudioSource = default;
    [SerializeField] private AudioClip[] WoodClips;
    [SerializeField] private AudioClip[] GrassClips;
    [SerializeField] private AudioClip[] MetalClips;
    private float footstepTimer = 0;
    private float GetCurrentOffset => isCrouching ? baseStepSpeed * crouchStepMultiplier : isSprinting ? baseStepSpeed * sprintStepMultiplier : baseStepSpeed;
    [SerializeField] private float Raycast = 0.5f;

    [Header("Camera Effects")]
    [SerializeField] private float DashFOV = 70f;
    [SerializeField] private float FOVEffectTime = 0.25f;

    public float maxTiltAngle = 5.0f;
    public float tiltSpeed = 5.0f;
    public string horizontalAxis = "Horizontal";

    [Header("Miscellaneous")]
    public bool CanChase = true;

    private float currentTiltAngle = 0.0f;

    public static FirstPersonController Instance;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }

        NormalSprintSpeed = SprintSpeed;
        NormalSprintBob = SprintBobSpeed;
        NormalSprintMultiplier = sprintStepMultiplier;
        NormalDashSpeed = DashSpeed;
        NormalSlideSpeed = SlideSpeed;

        if(CanSprint)
        {
            UseSprint = true;
        }

        if (!CanSprint)
        {
            UseSprint = false;
        }

        if(CanJump)
        {
            UseJump = true;
        }

        if(!UseJump)
        {
            UseJump = false;
        }

        if (CanCrouch)
        {
            UseCrouch = true;
        }

        if (!CanCrouch)
        {
            UseCrouch = false;
        }

        if (CanProne)
        {
            UseProne = true;
        }

        if (!CanProne)
        {
            UseProne = false;
        }

        if (CanDash)
        {
            UseDash = true;
        }

        if (!CanDash)
        {
            UseDash = false;
        }

        if(CanSlide)
        {
            UseSlide = true;
        }

        if(!CanSlide)
        {
            UseSlide = false;
        }

        DashCD = DashDelay;
        SlideCD = SlideDelay;

        characterController = GetComponent<CharacterController>();
        defaultYPos = playerCamera.transform.localPosition.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        CurrentPosition = transform.position;

        if (sprintKey != DashKey)
        {
            if (CanSprint && Input.GetKey(sprintKey))
            {
                isSprinting = true;
            }

            if (CanSprint && !Input.GetKey(sprintKey))
            {
                isSprinting = false;
            }
        }

        if (sprintKey == DashKey)
        {
            if (CanSprint && SprintHold)
            {
                isSprinting = true;
            }

            if (CanSprint && !SprintHold)
            {
                isSprinting = false;
            }
        }

        if (CanMove)
        {
            HandleMovementInput();

            ApplyFinalMovements();

            if (CanJump)
            {
                HandleJump();
            }

            if (CanCrouch)
            {
                HandleCrouch();
            }

            if(CanProne)
            {
                HandleProne();
            }

            if (CanDash)
            {
                HandleDash();
            }

            if (UseStamina)
            {
                HandleStamina();
            }

            if(DetectFallDamage)
            {
                HandleFallDamage();
            }

            if (UseHeadbob)
            {
                HandleHeadbob();
            }

            if (UseFootsteps)
            {
                HandleFootsteps();
            }
        }

        if (CanInteract)
        {
            HandleInteractonCheck();
            HandleInteractionInput();
        }

        if (sprintKey == DashKey && Input.GetKey(sprintKey))
        {
            SprintHoldTimer += Time.deltaTime;
        }

        if (SprintHoldTimer < MaxSprintHold && SprintHoldTimer > 0f && !Input.GetKey(sprintKey))
        {
            SprintHold = false;
            SprintTap = true;
        }

        if (SprintTap == true)
        {
            Invoke("NotTappingSprint", 0.05f);
        }

        if (SprintHoldTimer < MaxSprintHold)
        {
            SprintHold = false;
        }

        if (SprintHoldTimer > MaxSprintHold && Input.GetKey(sprintKey))
        {
            SprintTap = false;
            SprintHold = true;
        }

        if (SprintHold && !Input.GetKey(sprintKey))
        {
            SprintTap = false;

            SprintHoldTimer = 0f;

            SprintHold = false;
        }

        if (CanCrouch)
        {
            if (UseSprint)
            {
                if (isCrouching)
                {
                    CanSprint = false;
                }

                if (!isCrouching)
                {
                    CanSprint = true;
                }
            }

            if (UseDash)
            {
                if (!UseDashDuringCrouch)
                {
                    if (isCrouching)
                    {
                        CanDash = false;
                    }

                    if (!isCrouching)
                    {
                        CanDash = true;
                    }
                }
            }

            if(UseSlide)
            {
                if(!UseSlideDuringCrouch)
                {
                    if(isCrouching)
                    {
                        CanDash = false;
                    }

                    if(!isCrouching)
                    {
                        CanDash = true;
                    }
                }
            }

            if(UseJump)
            {
                if(isCrouching)
                {
                    CanJump = false;
                }

                if(!isCrouching)
                {
                    CanJump = true;
                }
            }
        }

        if (CanProne)
        {
            if (UseSprint)
            {
                if (isProning)
                {
                    CanSprint = false;
                }

                if (!isProning)
                {
                    CanSprint = true;
                }
            }

            if (UseDash)
            {
                if (!UseDashDuringProne)
                {
                    if (isProning)
                    {
                        CanDash = false;
                    }

                    if (!isProning)
                    {
                        CanDash = true;
                    }
                }
            }

            if (UseSlide)
            {
                if (!UseSlideDuringProne)
                {
                    if (isProning)
                    {
                        CanDash = false;
                    }

                    if (!isProning)
                    {
                        CanDash = true;
                    }
                }
            }

            if (UseJump)
            {
                if (isProning)
                {
                    CanJump = false;
                }

                if (!isProning)
                {
                    CanJump = true;
                }
            }
        }

        if (characterController.isGrounded)
        {
            if(UseCrouch)
            {
                CanCrouch = true;
            }
        }

        if (!characterController.isGrounded)
        {
            if (UseCrouch)
            {
                CanCrouch = false;
            }
        }

        if (UseDash)
        {
            if (!UseDashInAir)
            {
                if (characterController.isGrounded)
                {
                    if (UseDash)
                    {
                        CanDash = true;
                    }
                }

                if (!characterController.isGrounded)
                {
                    if (UseDash)
                    {
                        CanDash = false;
                    }
                }
            }
        }

        if (UseDash)
        {
            if (!UseDashOnGround)
            {
                if (!characterController.isGrounded)
                {
                    CanDash = true;
                }

                if (characterController.isGrounded)
                {
                    CanDash = false;
                }
            }
        }

        if (CanDash)
        {
            if (UseJump)
            {
                if (isDashing)
                {
                    CanJump = false;
                }

                if (!isDashing)
                {
                    CanJump = true;
                }
            }

            if (UseCrouch)
            {
                if (isDashing)
                {
                    CanCrouch = false;
                }

                if (!isDashing)
                {
                    CanCrouch = true;
                }
            }
        }

        if(characterController.isGrounded && !Input.GetKeyDown(JumpKey))
        {
            DoubleJump = false;
        }

        CurrentStamina = Mathf.Clamp(CurrentStamina, 0f, MaxStamina);
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);
    }

    void LateUpdate()
    {
        if (CanLook)
        {
            HandleMouseLook();
        }
    }

    public void NotTappingSprint()
    {
        SprintTap = false;
        SprintHold = false;

        SprintHoldTimer = 0f;

        CancelInvoke("NotTappingSprint");
    }

    private void HandleInteractonCheck()
    {
        /*if(Physics.Raycast(playerCamera.ViewportPointToRay(InteractionRayPoint), out RaycastHit hit, InteractionDistance))
        {
            if(hit.collider.gameObject.layer == 7 && (currentInteractable == null) || hit.collider.gameObject.GetInstanceID() != currentInteractable.GetInstanceID())
            {
                hit.collider.TryGetComponent<Interctable>(out currentInteractable);

                if (currentInteractable)
                    currentInteractable.OnFocus();

                else if (currentInteractable)
                {
                    currentInteractable.OnLoseFocus();
                    currentInteractable = null;
                }
            }
        }*/
    }

    private void HandleInteractionInput()
    {
        /*if(Input.GetKeyDown(InteractKey) && currentInteractable != null && Physics.Raycast(playerCamera.ViewportPointToRay(InteractionRayPoint), out RaycastHit hit, InteractionLayer, InteractionLayer))
        {
            currentInteractable.OnInteract();
        }*/
    }

    private void HandleMovementInput()
    {
        CurrentInput = new Vector2((isSprinting ? SprintSpeed : isCrouching ? CrouchSpeed : WalkSpeed) * Input.GetAxis("Vertical"), (isSprinting ? SprintSpeed : isCrouching ? CrouchSpeed : WalkSpeed) * Input.GetAxis("Horizontal"));

        float MoveDirectionY = MoveDirection.y;
        MoveDirection = (transform.TransformDirection (Vector3.forward) * CurrentInput.x) + (transform.TransformDirection (Vector3.right) * CurrentInput.y);
        MoveDirection.y = MoveDirectionY;
    }

    private void HandleMouseLook()
    {
        if (TiltCam)
        {
            float horizontalMovement = Input.GetAxisRaw(horizontalAxis);

            if (horizontalMovement != 0f)
            {
                float targetTiltAngle = -horizontalMovement * maxTiltAngle;
                currentTiltAngle = Mathf.Lerp(currentTiltAngle, targetTiltAngle, Time.deltaTime * tiltSpeed);
            }
            else
            {
                currentTiltAngle = Mathf.Lerp(currentTiltAngle, 0f, Time.deltaTime * tiltSpeed);
            }

            rotationX -= Input.GetAxis("Mouse Y") * LookSpeedY;
            rotationX = Mathf.Clamp(rotationX, -UpperLookLimit, LowerLookLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, currentTiltAngle);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * LookSpeedX, 0);
        }

        else
        {
            rotationX -= Input.GetAxis("Mouse Y") * LookSpeedY;
            rotationX = Mathf.Clamp(rotationX, -UpperLookLimit, LowerLookLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * LookSpeedX, 0);
        }
    }

    private void HandleFootsteps()
    {
        if (!characterController.isGrounded) return;
        if (CurrentInput == Vector2.zero) return;

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0)
        {
            if (Physics.Raycast(characterController.transform.position, Vector3.down, out RaycastHit hit, Raycast))
            {
                switch (hit.collider.tag)
                {
                    case "FootSteps/Grass":
                        footstepAudioSource.PlayOneShot(GrassClips[Random.Range(0, GrassClips.Length - 1)]);
                        break;
                    case "FootSteps/Metal":
                        footstepAudioSource.PlayOneShot(MetalClips[Random.Range(0, MetalClips.Length - 1)]);
                        break;
                    case "FootSteps/Wood":
                        footstepAudioSource.PlayOneShot(WoodClips[Random.Range(0, WoodClips.Length - 1)]);
                        break;
                    default:
                        footstepAudioSource.PlayOneShot(GrassClips[Random.Range(0, GrassClips.Length - 1)]);
                        break;
                }
            }

            footstepTimer = GetCurrentOffset;
        }
    }

    private void ApplyFinalMovements()
    {
        if (!characterController.isGrounded)
            MoveDirection.y -= JumpGravity * Time.deltaTime;

        characterController.Move(MoveDirection * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (characterController.isGrounded)
        {
            CoyoteTimeCounter = CoyoteTime;
        }

        else
        {
            CoyoteTimeCounter -= Time.deltaTime;
        }


        if (UseJumpBuffer)
        {
            if (UseCoyoteTime)
            {
                if (!CanDoubleJump)
                {
                    if (CoyoteTimeCounter > 0f && Input.GetKeyDown(JumpKey))
                    {
                        MoveDirection.y = JumpForce;

                        CoyoteTimeCounter = 0f;
                    }
                }

                if (CanDoubleJump)
                {
                    if (Input.GetKeyDown(JumpKey))
                    {
                        if (CoyoteTimeCounter > 0f || DoubleJump)
                        {
                            MoveDirection.y = JumpForce;
                            
                            DoubleJump = !DoubleJump;
                            CoyoteTimeCounter = 0f;
                        }
                    }
                }
            }

            if (!UseCoyoteTime)
            {
                if (!CanDoubleJump)
                {
                    if (ShouldJump)
                    {
                        MoveDirection.y = JumpForce;
                    }
                }

                if (CanDoubleJump)
                {
                    if (Input.GetKeyDown(JumpKey))
                    {
                        if (characterController.isGrounded || DoubleJump)
                        {
                            MoveDirection.y = JumpForce;

                            DoubleJump = !DoubleJump;
                        }
                    }
                }
            }
        }

        if(!UseJumpBuffer)
        {
            if (UseCoyoteTime)
            {
                if (!CanDoubleJump)
                {
                    if (CoyoteTimeCounter > 0f && Input.GetKeyDown(JumpKey))
                    {
                        MoveDirection.y = JumpForce;

                        CoyoteTimeCounter = 0f;
                    }
                }

                if (CanDoubleJump)
                {
                    if (Input.GetKeyDown(JumpKey))
                    {
                        if (CoyoteTimeCounter > 0f || DoubleJump)
                        {
                            MoveDirection.y = JumpForce;

                            DoubleJump = !DoubleJump;
                            CoyoteTimeCounter = 0f;
                        }
                    }
                }
            }

            if (!UseCoyoteTime)
            {
                if (!CanDoubleJump)
                {
                    if (ShouldJump)
                    {
                        MoveDirection.y = JumpForce;
                    }
                }

                if (CanDoubleJump)
                {
                    if (Input.GetKeyDown(JumpKey))
                    {
                        if (characterController.isGrounded || DoubleJump)
                        {
                            MoveDirection.y = JumpForce;

                            DoubleJump = !DoubleJump;
                        }
                    }
                }
            }
        }
    }
    
    private void HandleCrouch()
    {
        if (ShouldCrouch)
        {
            StartCoroutine(CrouchStand());
        }
    }

    private void HandleProne()
    {
        if (ShouldProne)
        {
            StartCoroutine(ProneStand());
        }
    }

    private void HandleDash()
    {
        DashCD -= Time.deltaTime;

        if (DashKey != sprintKey)
        {
            if (Input.GetKeyDown(DashKey))
            {
                if (DashCD <= 0)
                {
                    StartCoroutine(Dash());

                    JumpGravity = 0;
                    isDashing = true;
                }
            }
        }

        if(DashKey == sprintKey)
        {
            if(SprintTap)
            {
                if (DashCD <= 0)
                {
                    StartCoroutine(Dash());

                    JumpGravity = 0;
                    isDashing = true;
                }
            }
        }
    }

    IEnumerator Dash()
    {
        float StartTime = Time.time;

        Vector3 direction = GetDirection(transform);

        if(Time.time < StartTime + DashTime)
        {
            if (UseFOVEffect)
            {
                FOVeffect(DashFOV);
            }

            characterController.Move(direction * DashSpeed * DashTime * Time.deltaTime);

            ResetDash();

            yield return null;
        }
    }

    private void ResetDash()
    {
        JumpGravity = NormalJumpGravity;
        isDashing = false;
        DashCD = DashDelay;

        if (UseFOVEffect)
        {
            FOVeffect(60f);
        }
    }

    public void FOVeffect(float EndValue)
    {
        playerCamera.DOFieldOfView(EndValue, FOVEffectTime);
    }

    private Vector3 GetDirection(Transform transform)
    {
        float HorizontalInput = Input.GetAxisRaw("Horizontal");
        float VerticalInput = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3();

        direction = transform.forward * VerticalInput + transform.right * HorizontalInput;

        if(VerticalInput == 0 && HorizontalInput == 0)
        {
            direction = transform.forward;
        }

        return direction.normalized;
    }

    private void HandleStamina()
    {
        if(UseStaminaOnSprint)
        {
            if(isSprinting && CurrentInput != Vector2.zero)
            {
                DepletionAmount = SprintDepletionAmount;
                StaminaInUse = true;
            }

            else
            {
                DepletionAmount = 0;
                StaminaInUse = false;
            }
        }

        if(UseStaminaOnDash)
        {
            if (isDashing)
            {
                CurrentStamina -= DashDepletionAmount / 2;
                StaminaInUse = true;
            }

            else
            {
                if (UseStaminaOnSprint)
                {
                    if (!StaminaInUse)
                    {
                        StaminaInUse = false;
                    }
                }

                else if(!UseStaminaOnSprint)
                {
                    StaminaInUse = false;
                }
            }
        }

        /*if(UseStaminaOnJump)
        {
            if(Input.GetKeyDown(JumpKey) && !characterController.isGrounded)
            {
                CurrentStamina -= JumpDepletionAmount / 2;
            }
        }*/

        if (CurrentStamina <= 0)
        {
            CurrentStamina = 0;

            if(UseStaminaOnSprint)
            {
                SprintSpeed = DecreasedSprintSpeed;
                SprintBobSpeed = DecreasedSprintBob;
                sprintStepMultiplier = DecreasedSprintMultiplier;
            }

            if(UseStaminaOnDash)
            {
                DashSpeed = DecreasedDashSpeed;
            }
        }

        if(CurrentStamina > 0.1f)
        {
            if(UseStaminaOnSprint)
            {
                SprintSpeed = NormalSprintSpeed;
                SprintBobSpeed = NormalSprintBob;
                sprintStepMultiplier = NormalSprintMultiplier;
            }

            if(UseStaminaOnDash)
            {
                DashSpeed = NormalDashSpeed;
            }
        }

        if (CurrentStamina > MaxStamina)
        {
            CurrentStamina = MaxStamina;
        }

        if (StaminaInUse)
        {
            CancelInvoke("RegenStamina");
            Invoke("DepleteStamina", 0.0f);
        }

        if(!StaminaInUse)
        {
            CancelInvoke("DepleteStamina");

            if (CurrentStamina < MaxStamina)
            {
                Invoke("RegenStamina", StaminaRegenerationDelay);
            }
        }
    }

    void DepleteStamina()
    {
        CurrentStamina -= DepletionAmount * Time.deltaTime;
    }

    void RegenStamina()
    {
        CurrentStamina += StaminaRegenrationAmount * Time.deltaTime;
    }
    
    IEnumerator CrouchStand()
    {
        if (isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
        {
            yield break;
        }

        duringCrouchAnimation = true;

        float TimeElapsed = 0f;
        float TargetHeight = isCrouching ? StandingHeight : CrouchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? StandingCenter : CrouchingCenter;
        Vector3 currentCenter = characterController.center;

        if(TimeElapsed < TimeToCrouch)
        {
            characterController.height = Mathf.Lerp(currentHeight, TargetHeight, TimeElapsed / TimeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, TimeElapsed / TimeToCrouch);
            TimeElapsed += Time.deltaTime;
            yield return null;
        }

        characterController.height = TargetHeight;
        characterController.center = targetCenter;

        isCrouching = !isCrouching;

        duringCrouchAnimation = false;
    }

    IEnumerator ProneStand()
    {
        if (isProning && Physics.Raycast(playerCamera.transform.position, Vector3.up, 2f))
        {
            yield break;
        }

        duringProneAnimation = true;

        float TimeElapsed = 0f;
        float TargetHeight = isProning ? StandingHeights : ProneHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isProning ? StandingCenters : ProningCenter;
        Vector3 currentCenter = characterController.center;

        if(TimeElapsed < TimeToProne)
        {
            characterController.height = Mathf.Lerp(currentHeight, TargetHeight, TimeElapsed / TimeToProne);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, TimeElapsed / TimeToProne);
            TimeElapsed += Time.deltaTime;
            yield return null;
        }

        characterController.height = TargetHeight;
        characterController.center = targetCenter;

        isProning = !isProning;

        duringProneAnimation = false;
    }

    public void HandleFallDamage()
    {
        float AirTimeIncrement = 1f;

        if(!characterController.isGrounded)
        {
            CurrentAirTime += AirTimeIncrement * Time.deltaTime;
        }

        if(!characterController.isGrounded && isDashing)
        {
            AirTimeIncrement = 0f;
        }

        if (!characterController.isGrounded && !isDashing)
        {
            AirTimeIncrement = 1f;
        }

        if (CurrentAirTime >= MaxAirTime)
        {
            DamageToBeDealt += DamageIncrement * Time.deltaTime;
        }

        if(characterController.isGrounded && CurrentAirTime <= MaxAirTime)
        {
            CurrentAirTime = 0;
            DamageToBeDealt = 0;
        }

        if(characterController.isGrounded && CurrentAirTime >= MaxAirTime)
        {
            CurrentHealth -= DamageToBeDealt;
            CurrentAirTime = 0;
            DamageToBeDealt = 0;
        }
    }

    private void HandleHeadbob()
    {
        if (!characterController.isGrounded) return;

        if(Mathf.Abs(MoveDirection.x) > 0.1f || Mathf.Abs(MoveDirection.z) > 0.1f)
        {
            timer += Time.deltaTime * (isCrouching ? CrouchBobSpeed : isSprinting ? SprintBobSpeed : WalkBobSpeed);
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * (isCrouching ? CrouchBobAmount : isSprinting ? SprintBobAmount : WalkBobAmount),
                playerCamera.transform.localPosition.z);
        }

        else
        {
            timer += Time.deltaTime * (isCrouching ? CrouchBobSpeed : isSprinting ? SprintBobSpeed : WalkBobSpeed);
            playerCamera.transform.localPosition = new Vector3(playerCamera.transform.localPosition.x, defaultYPos, playerCamera.transform.localPosition.z);
        }
    }
}
