using System.Collections;
using System.Collections.Generic;
using Logic;
using UnityEngine;

public enum EntityType
{
    Asteroid,
    Ufo,
    Bullet
}

public class Entity
{
    public EntityType type;
    public FixedPointVector3 position;
    public Collider collider;

    public virtual void LogicStep(float dt) { }
}

public class Ufo : Entity
{
    public int health = 20;

    public Ufo(FixedPointVector3 pos)
    {
        position = pos;
        collider = new CircleCollider(pos);

        collider.layer = 0;
        collider.collidesWithLayers[1] = true;
    }
}

public class Asteroid : Entity
{
    public int health = 5;

    public Asteroid(FixedPointVector3 pos)
    {
        position = pos;
        collider = new CircleCollider(pos);

        collider.layer = 1;
        collider.collidesWithLayers[0] = true;
    }
}

public class Blast : Entity
{
    public Blast(FixedPointVector3 pos)
    {
        position = pos;
        collider = new OrientedBoxCollider(pos, 5, 10, 0);

        collider.layer = 2;
        collider.collidesWithLayers[0] = true;
        collider.collidesWithLayers[1] = true;
    }
}