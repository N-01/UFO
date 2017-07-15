using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//i considered just low-level rendering with just filling vbo/instancing
//but it would be too simplistic and unappliable to complex gameobjects
public class EntityRenderer : MonoBehaviour {
    private List<EntityView> activeViews = new List<EntityView>();

    //pool to reuse gameobjects
    private List<EntityView> recycledViews = new List<EntityView>();

    public Dictionary<EntityType, EntityView> prefabs = new Dictionary<EntityType, EntityView>();

    public void Awake()
    {
        foreach (EntityType t in Enum.GetValues(typeof(EntityType)))
        {
            prefabs.Add(t, Resources.Load<EntityView>(t.ToString()));
        }
    }

    public EntityView Create(Entity entity)
    {
        var found = recycledViews.FirstOrDefault(v => v.type == entity.type);

        if (found != null) {
            recycledViews.Remove(found);
        }
        else {
            found = Instantiate(prefabs[entity.type]);
        }

        activeViews.Add(found);
        found.Activate();

        return found;
    }

    public void Recycle(EntityView view)
    {
        //much faster, than disabling GameObject
        view.renderer.enabled = false;

        activeViews.Remove(view);
        recycledViews.Add(view);
    }

    public void RecycleDelayed(EntityView view, float delay = 1)
    {
        StartCoroutine(DeathCoroutine(view, delay));
    }

    private IEnumerator DeathCoroutine(EntityView view, float delay)
    {
        yield return new WaitForSeconds(1.0f);
        Recycle(view);
    }
}
