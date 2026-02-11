using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    #region Serialized Fields
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float secondJumpForce = 10f;
    [SerializeField] private bool canDoubleJump = false;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Crouch Settings")]
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float crouchColliderHeight = 1f;
    [SerializeField] private Vector2 crouchColliderOffset = new Vector2(0f, -0.5f);
    [SerializeField] private Transform ceilingCheck;
    [SerializeField] private float ceilingCheckRadius = 0.2f;

    [Header("Climb Settings")]
    [SerializeField] private float climbSpeed = 3f;
    #endregion

    #region Private Variables
    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;
    private float horizontalInput;
    private float verticalInput;
    private bool facingRight = true;
    private bool isGrounded;
    private int jumpCount = 0;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool isCrouching;
    private bool canStandUp;
    private float originalColliderHeight;
    private Vector2 originalColliderOffset;
    private bool isRunning;

    // Climb variables
    private bool isClimbing;
    private bool canClimb;
    private float originalGravityScale;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        originalColliderHeight = capsuleCollider.size.y;
        originalColliderOffset = capsuleCollider.offset;
        originalGravityScale = rb.gravityScale;
    }

    private void Update()
    {
        GetInput();
        CheckGroundStatus();
        CheckCeiling();
        UpdateTimers();
        HandleClimb();
        HandleJump();
        HandleCrouch();
    }

    private void FixedUpdate()
    {
        if (isClimbing)
        {
            HandleClimbMovement();
        }
        else
        {
            HandleMovement();
            ApplyJumpPhysics();
        }
    }
    #endregion

    #region Input
    private void GetInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        isRunning = Input.GetKey(KeyCode.LeftShift);
        if (Input.GetButtonDown("Jump")) jumpBufferCounter = jumpBufferTime;
    }

    private void UpdateTimers()
    {
        coyoteTimeCounter = isGrounded ? coyoteTime : coyoteTimeCounter - Time.deltaTime;
        jumpBufferCounter -= Time.deltaTime;
    }
    #endregion

    #region Ground and Ceiling Check
    private void CheckGroundStatus()
    {
        bool previousGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded && !previousGrounded) jumpCount = 0;
    }

    private void CheckCeiling()
    {
        canStandUp = ceilingCheck == null || !Physics2D.OverlapCircle(ceilingCheck.position, ceilingCheckRadius, groundLayer);
    }
    #endregion

    #region Movement
    private void HandleMovement()
    {
        float targetSpeed = isCrouching ? horizontalInput * crouchSpeed :
                           (isRunning ? horizontalInput * runSpeed : horizontalInput * moveSpeed);

        rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);

        if (horizontalInput > 0 && !facingRight) Flip();
        else if (horizontalInput < 0 && facingRight) Flip();
    }

    private void Flip()
    {
        facingRight = !facingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }
    #endregion

    #region Climb
    private void HandleClimb()
    {
        if (canClimb && Mathf.Abs(verticalInput) > 0.1f && !isClimbing)
        {
            StartClimb();
        }
        else if (isClimbing && (!canClimb || jumpBufferCounter > 0f))
        {
            StopClimb();
        }
    }

    private void HandleClimbMovement()
    {
        float verticalSpeed = verticalInput * climbSpeed;
        float horizontalSpeed = horizontalInput * climbSpeed;
        rb.linearVelocity = new Vector2(horizontalSpeed, verticalSpeed);

        if (horizontalInput > 0 && !facingRight) Flip();
        else if (horizontalInput < 0 && facingRight) Flip();
    }

    private void StartClimb()
    {
        isClimbing = true;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        if (isCrouching) StopCrouch();
    }

    private void StopClimb()
    {
        isClimbing = false;
        rb.gravityScale = originalGravityScale;
    }

    // Llamar desde el trigger de la escalera
    public void SetCanClimb(bool value)
    {
        canClimb = value;
        if (!canClimb && isClimbing) StopClimb();
    }
    #endregion

    #region Jump
    private void HandleJump()
    {
        if (jumpBufferCounter <= 0f) return;

        // Si está escalando, salir de la escalera
        if (isClimbing)
        {
            StopClimb();
            Jump(jumpForce);
            jumpBufferCounter = 0f;
            return;
        }

        if (coyoteTimeCounter > 0f && jumpCount == 0)
        {
            if (isCrouching) StopCrouch();
            Jump(jumpForce);
            jumpCount = 1;
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }
        else if (canDoubleJump && jumpCount == 1)
        {
            Jump(secondJumpForce);
            jumpCount = 2;
            jumpBufferCounter = 0f;
        }
    }

    private void Jump(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }

    private void ApplyJumpPhysics()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }
    #endregion

    #region Crouch
    private void HandleCrouch()
    {
        if (isClimbing) return; // No agacharse mientras escala
        if (!isGrounded || !Input.GetKeyDown(KeyCode.S)) return;

        if (!isCrouching) StartCrouch();
        else if (canStandUp) StopCrouch();
    }

    private void StartCrouch()
    {
        isCrouching = true;
        capsuleCollider.size = new Vector2(capsuleCollider.size.x, crouchColliderHeight);
        capsuleCollider.offset = crouchColliderOffset;
    }

    private void StopCrouch()
    {
        isCrouching = false;
        capsuleCollider.size = new Vector2(capsuleCollider.size.x, originalColliderHeight);
        capsuleCollider.offset = originalColliderOffset;
    }
    #endregion

    #region Public Methods
    public void EnableDoubleJump() => canDoubleJump = true;
    public void DisableDoubleJump() => canDoubleJump = false;

    public bool IsGrounded() => isGrounded;
    public bool IsRunning() => isRunning && !isCrouching && !isClimbing && Mathf.Abs(horizontalInput) > 0.1f;
    public bool IsCrouching() => isCrouching;
    public bool IsMoving() => Mathf.Abs(horizontalInput) > 0.1f;
    public float GetSpeed() => Mathf.Abs(rb.linearVelocity.x);
    public float GetVerticalVelocity() => rb.linearVelocity.y;
    public int GetJumpCount() => jumpCount;
    public bool IsClimbing() => isClimbing;
    public float GetClimbSpeed() => Mathf.Abs(verticalInput);
    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (ceilingCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(ceilingCheck.position, ceilingCheckRadius);
        }
    }
    #endregion
}