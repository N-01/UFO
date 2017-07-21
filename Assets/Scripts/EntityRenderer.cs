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

    public void UpdateViews()
    {
        foreach (var view in activeViews) {
            view.transform.position = view.entity.position;
            view.transform.rotation = Quaternion.Euler(0, 0, view.entity.angle * Mathf.Rad2Deg);

            if (view.entity.type == EntityType.Placeholder)
                view.sprite.color = (view.entity.behavior.IsColliding() ? new Color(1, 0, 0, 0.4f) : new Color(1, 1, 1, 0.4f));
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
        view.sprite.transform.localScale = new Vector3(entity.scale, entity.scale, 0);

        activeViews.Add(view);
        view.Activate();

        return view;
    }

    public void Recycle(EntityView view)
    {
        //much faster, than disabling whole GameObject
        view.sprite.enabled = false;

        activeViews.Remove(view);
        recycledViews.Add(view);
    }

    public void RecycleDelayed(Entity entity)
    {
        var target = activeViews.FirstOrDefault(v => v.entity == entity);
        target.animator.SetTrigger("die");
        StartCoroutine(DeathCoroutine(target, target.deathDelay));
    }

    private IEnumerator DeathCoroutine(EntityView view, float delay)
    {
        yield return new WaitForSeconds(delay);
        Recycle(view);
    }

    public void Reset()
    {
        foreach (var v in activeViews)
        {
            DestroyObject(v);
        }

        activeViews.Clear();
        recycledViews.Clear();
    }
}
