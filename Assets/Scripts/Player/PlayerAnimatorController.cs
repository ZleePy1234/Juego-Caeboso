using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerController2D playerController;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (playerController == null) playerController = GetComponent<PlayerController2D>();
    }

    private void Update()
    {
        UpdateAnimatorParameters();
    }

    private void UpdateAnimatorParameters()
    {
        // Parámetros de movimiento
        animator.SetBool("IsGrounded", playerController.IsGrounded());
        animator.SetBool("IsMoving", playerController.IsMoving());
        animator.SetBool("IsRunning", playerController.IsRunning());
        animator.SetBool("IsCrouching", playerController.IsCrouching());

        // Climb
        bool isClimbing = playerController.IsClimbing();
        float climbSpeed = playerController.GetClimbSpeed();

        animator.SetBool("IsClimbing", isClimbing);
        animator.SetFloat("ClimbSpeed", climbSpeed);

        // Velocidad
        animator.SetFloat("Speed", playerController.GetSpeed());
        animator.SetFloat("VerticalVelocity", playerController.GetVerticalVelocity());

        // Jump count
        animator.SetInteger("JumpCount", playerController.GetJumpCount());

        // Jumping states
        if (playerController.GetJumpCount() == 1 && !playerController.IsGrounded())
            animator.SetBool("IsJumping", true);
        else
            animator.SetBool("IsJumping", false);

        if (playerController.GetJumpCount() == 2)
            animator.SetBool("IsDoubleJumping", true);
        else
            animator.SetBool("IsDoubleJumping", false);

        // Falling
        if (!playerController.IsGrounded() && !playerController.IsClimbing() && playerController.GetVerticalVelocity() < -0.1f)
            animator.SetBool("IsFalling", true);
        else
            animator.SetBool("IsFalling", false);
    }
}