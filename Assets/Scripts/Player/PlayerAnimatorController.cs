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

        // Velocidad
        animator.SetFloat("Speed", playerController.GetSpeed());

        // Velocidad vertical para animación de caída/salto
        animator.SetFloat("VerticalVelocity", playerController.GetVerticalVelocity());

        // Jump count para diferenciar primer y segundo salto
        animator.SetInteger("JumpCount", playerController.GetJumpCount());

        // Triggers para transiciones específicas
        if (playerController.GetJumpCount() == 1 && !playerController.IsGrounded()) animator.SetBool("IsJumping", true); // Primer salto
        else animator.SetBool("IsJumping", false);

        if (playerController.GetJumpCount() == 2) animator.SetBool("IsDoubleJumping", true); // Segundo salto
        else animator.SetBool("IsDoubleJumping", false);


        // Detectar si está cayendo
        if (!playerController.IsGrounded() && playerController.GetVerticalVelocity() < -0.1f) animator.SetBool("IsFalling", true); // Velocidad negativa, caída
        else animator.SetBool("IsFalling", false);

    }
}