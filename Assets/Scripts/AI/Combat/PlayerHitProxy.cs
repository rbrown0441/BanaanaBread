using UnityEngine;

[RequireComponent(typeof(CharacterScript))]
public class PlayerHitProxy : MonoBehaviour
{
    private CharacterScript player;

    void Awake()
    {
        player = GetComponent<CharacterScript>();
    }

    // This method will be called by HurtOnTrigger2D when the player is hit
    public void TakeHit(int damage, Vector2 hitForce)
    {
        if (player != null)
            player.Hurt(damage, hitForce);
    }
}
