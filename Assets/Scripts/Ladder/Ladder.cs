using UnityEngine;

public class LadderTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (collision.TryGetComponent<PlayerController2D>(out var player))
            {
                player.SetCanClimb(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (collision.TryGetComponent<PlayerController2D>(out var player))
            {
                player.SetCanClimb(false);
            }
        }
    }
}