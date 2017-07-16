using System.Collections;
using System.Collections.Generic;
using Logic;
using Physics;
using UnityEngine;
using Utils;

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
    public FixedPoint scale;
    public Body body;

    public OnceEmptyStream deathEvent = new OnceEmptyStream();

    public FixedPointVector3 direction = new FixedPointVector3(0.01f, 0.01f, 0);
    public virtual void Step(FixedPoint dt)
    {
        direction.X += (FixedPoint)(Random.Range(-1f, 1f) * 0.1f);
        direction.Y += (FixedPoint)(Random.Range(-1f, 1f) * 0.1f);
        position += direction * dt;
        body.position = position;
    }
}

public class Ufo : Entity
{
    public int health = 20;

    public Ufo(FixedPointVector3 _pos, FixedPoint _scale)
    {
        position = _pos;
        scale = _scale;

        body = new CircleBody(position, scale);

        body.layer = 0;
        body.collidesWithLayers[1] = true;
        body.owner = this;
    }
}

public class Asteroid : Entity
{
    public int health = 3;

    public Asteroid(FixedPointVector3 _pos, FixedPoint _scale)
    {
        position = _pos;
        scale = _scale;
        body = new CircleBody(position, scale * (FixedPoint)0.5f);

        body.layer = 1;
        body.collidesWithLayers[0] = true;
        body.collidesWithLayers[1] = true;
        body.owner = this;

        var subscription = body.collisionStream.Listen(other =>
        {
            health -= 1;

            if (health < 1)
            {
                deathEvent.Send();
            }

            if (other.owner is Asteroid)
            {
                direction = (body.position - other.position).Normalized * 2;
            }
        });

        deathEvent.Listen(subscription.Dispose);
    }
}

public class Blast : Entity
{
    public Blast(FixedPointVector3 pos)
    {
        position = pos;
        body = new OrientedBoxBody(pos, 5, 10, 0);

        body.layer = 2;
        body.collidesWithLayers[0] = true;
        body.collidesWithLayers[1] = true;
        body.owner = this;
    }
}