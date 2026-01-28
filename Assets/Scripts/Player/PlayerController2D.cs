using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    #region Serialized Fields
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;

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
    #endregion

    #region Private Variables
    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;

    private float horizontalInput;
    private float currentSpeed;
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
    #endregion

    #region Unity Methods
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        originalColliderHeight = capsuleCollider.size.y;
        originalColliderOffset = capsuleCollider.offset;
    }

    private void Update()
    {
        GetInput();
        CheckGroundStatus();
        CheckCeiling();
        UpdateTimers();
        HandleJump();
        HandleCrouch();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        ApplyJumpPhysics();
    }
    #endregion

    #region Input
    private void GetInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        isRunning = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetButtonDown("Jump")) jumpBufferCounter = jumpBufferTime;

    }

    private void UpdateTimers() // Actualizar temporizadores de coyote time y jump buffer
    {
        if (isGrounded) coyoteTimeCounter = coyoteTime; // Coyote time: pequeña ventana de tiempo después de dejar el suelo

        else coyoteTimeCounter -= Time.deltaTime;

        jumpBufferCounter -= Time.deltaTime;  // Jump buffer: recuerda el input de salto por un breve momento
    }
    #endregion

    #region Ground and Ceiling Check
    private void CheckGroundStatus() // Verificar si está en el suelo
    {
        bool previousGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded && !previousGrounded)
        {
            jumpCount = 0;
        }
    }

    private void CheckCeiling() // Verificar si hay techo encima
    {
        if (ceilingCheck != null) canStandUp = !Physics2D.OverlapCircle(ceilingCheck.position, ceilingCheckRadius, groundLayer);

        else canStandUp = true;

    }
    #endregion

    #region Movement
    private void HandleMovement() // Manejar movimiento horizontal
    {
        // Determinar velocidad según el estado
        float targetSpeed;

        if (isCrouching) targetSpeed = horizontalInput * crouchSpeed;

        else if (isRunning && !isCrouching) targetSpeed = horizontalInput * runSpeed;

        else targetSpeed = horizontalInput * moveSpeed;

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelRate * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);

        if (horizontalInput > 0 && !facingRight) Flip();
        else if (horizontalInput < 0 && facingRight) Flip();
    }

    private void Flip() // Voltear el sprite del jugador
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    #endregion

    #region Jump
    private void HandleJump() // Manejar saltos
    {
        // Si hay input de salto en el buffer
        if (jumpBufferCounter > 0f)
        {
            // Primer salto: si está en el suelo O dentro del coyote time
            if (coyoteTimeCounter > 0f && jumpCount == 0)
            {
                if (isCrouching) StopCrouch();  // Cancelar agachado si está agachado

                Jump(jumpForce);
                jumpCount = 1;
                jumpBufferCounter = 0f;
                coyoteTimeCounter = 0f;
            }
            else if (canDoubleJump && jumpCount == 1) // Segundo salto: si puede hacer doble salto, ya saltó una vez, y no ha usado el doble salto
            {
                Jump(secondJumpForce);
                jumpCount = 2;
                jumpBufferCounter = 0f;
            }
        }
    }

    private void Jump(float force) // Realizar salto
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }

    private void ApplyJumpPhysics() // Aplicar física de salto mejorada
    {
        if (rb.linearVelocity.y < 0) rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;

        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump")) rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;

    }
    #endregion

    #region Crouch
    private void HandleCrouch() // Manejar agachado
    {
        if (isGrounded && Input.GetKeyDown(KeyCode.S))
        {
            if (!isCrouching) StartCrouch();

            else if (canStandUp) StopCrouch();
        }
    }

    private void StartCrouch() // Iniciar agachado
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
    public void EnableDoubleJump() // Llamar para activar el doble salto (por ejemplo, al recoger un power-up)
    {
        canDoubleJump = true;
    }

    public void DisableDoubleJump() // Llamar para desactivar el doble salto (por ejemplo, al recoger un power-up temporal)
    {
        canDoubleJump = false;
    }

    // Getters para el Animator
    public bool IsGrounded() => isGrounded;
    public bool IsRunning() => isRunning && !isCrouching && Mathf.Abs(horizontalInput) > 0.1f;
    public bool IsCrouching() => isCrouching;
    public bool IsMoving() => Mathf.Abs(horizontalInput) > 0.1f;
    public float GetSpeed() => Mathf.Abs(currentSpeed);
    public float GetVerticalVelocity() => rb.linearVelocity.y;
    public int GetJumpCount() => jumpCount;
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