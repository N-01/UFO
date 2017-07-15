using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityView : MonoBehaviour
{
    public EntityType type;
    public Renderer renderer;

    public void Activate() {
        renderer.enabled = true;
    }
}
