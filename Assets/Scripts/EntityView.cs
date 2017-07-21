using UnityEngine;

public class EntityView : MonoBehaviour
{
    public Entity entity;
    public SpriteRenderer sprite;
    public Animator animator;

    public float deathDelay = 0;

    public void Activate() {
        sprite.enabled = true;
        animator.SetTrigger("idle");
    }
}
