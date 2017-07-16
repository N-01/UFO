using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityView : MonoBehaviour
{
    public Entity entity;
    public SpriteRenderer renderer;

    public int x, y;

    public void Activate() {
        renderer.enabled = true;
    }
}
