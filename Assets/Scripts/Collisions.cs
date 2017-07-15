using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Logic;
using UnityEngine;

public class Collider
{
    public FixedPointVector3 position;

    public int layer = 0; //number up to 32
    public BitVector32 collidesWithLayers;

    public Action<Collider> onCollision = null;

    public Collider() { }
    public Collider(FixedPointVector3 pos) { position = pos; }

    public bool CanCollideWith(Collider other)
    {
        return collidesWithLayers[other.layer];
    }
}

public class CircleCollider : Collider
{
    public FixedPoint radius;

    public CircleCollider() { }
    public CircleCollider(FixedPointVector3 pos) : base(pos) { }
}

public class OrientedBoxCollider : Collider
{
    public FixedPoint angle;
    public FixedPoint width, height;

    public OrientedBoxCollider() { }

    public OrientedBoxCollider(FixedPointVector3 pos, FixedPoint w, FixedPoint h, FixedPoint a) : base(pos)
    {
        angle = a;
        width = w;
        height = h;
    }
}

public class CollisionProcessor
{
    List<CircleCollider> circleColliders = new List<CircleCollider>();
    List<OrientedBoxCollider> boxColliders = new List<OrientedBoxCollider>();

    public void AddCollider(Collider c)
    {
        if(c is CircleCollider)
            circleColliders.Add(c as CircleCollider);
        else
            boxColliders.Add(c as OrientedBoxCollider);
    }

    public void RemoveCollider<T>(T c) where T : Collider
    {
        if (c is CircleCollider)
            circleColliders.Remove(c as CircleCollider);
        else
            boxColliders.Remove(c as OrientedBoxCollider);
    }

    public void Step()
    {
        //circle vs circle
        for (int i = 0; i < circleColliders.Count; i++) {
            for (int j = i + 1; j < circleColliders.Count; j++)
            {
                var first = circleColliders[i];
                var second = circleColliders[j];

                if (first.CanCollideWith(second))
                {
                    if (CheckCollision(first, second))
                    {
                        first.onCollision(second);
                        second.onCollision(first);
                    }
                }
            }
        }

        //box vs circle
        for (int i = 0; i < boxColliders.Count; i++)
        {
            for (int j = i + 1; j < circleColliders.Count; j++)
            {
                var first = boxColliders[i];
                var second = circleColliders[j];

                if (first.CanCollideWith(second))
                {
                    if (CheckCollision(first, second))
                    {
                        first.onCollision(second);
                        second.onCollision(first);
                    }
                }
            }
        }
    }

    public bool CheckCollision(CircleCollider first, CircleCollider second)
    {
        return (first.position - second.position).Abs.Magnitude <= first.radius + second.radius;
    }

    public bool CheckCollision(OrientedBoxCollider first, CircleCollider second)
    {
        return false;
    }
}