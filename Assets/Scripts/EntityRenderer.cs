using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Logic;
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

    public void UpdatePositions()
    {
        foreach (var view in activeViews) {
            view.gameObject.transform.position = view.entity.position;
            view.renderer.color = view.entity.body.IsColliding() ? Color.red : Color.white;
            view.x = view.entity.body.occupiedTile.x;
            view.y = view.entity.body.occupiedTile.y;
        }
    }

    public EntityView Show(Entity entity)
    {
        var view = recycledViews.FirstOrDefault(v => v.entity.type == entity.type);

        if (view != null) {
            recycledViews.Remove(view);
        }
        else {
            view = Instantiate(prefabs[entity.type]);
            view.transform.parent = this.transform;
        }

        view.entity = entity;
        view.transform.localScale = new Vector3(entity.scale, entity.scale, 0);

        activeViews.Add(view);
        view.Activate();

        entity.deathEvent.Listen(() => RecycleDelayed(view, 3));

        return view;
    }

    public void Recycle(EntityView view)
    {
        //much faster, than disabling GameObject
        view.renderer.enabled = false;

        activeViews.Remove(view);
        recycledViews.Add(view);
    }

    public void RecycleDelayed(EntityView view, FixedPoint delay)
    {
        StartCoroutine(DeathCoroutine(view, delay));
    }

    private IEnumerator DeathCoroutine(EntityView view, FixedPoint delay)
    {
        yield return new WaitForSeconds(1.0f);
        Recycle(view);
    }
}
