using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Logic.Math;
using Logic;
using Physics;
using UnityEngine;
using Utils;

public enum EntityType
{
    Asteroid,
    Ufo,
    Blast
}

public class Entity
{
    public EntityType type;
    public FixedPointVector3 position;
    public FixedPoint scale;
    public FixedPoint speed;

    public GameController owner;
    public Body body;

    public OnceEmptyStream deathEvent = new OnceEmptyStream();

    public FixedPointVector3 direction = new FixedPointVector3(0.01f, 0.01f, 0);

    public virtual void Step(FixedPoint dt)
    {
        position += direction * dt * speed;
        body.position = position;
    }
}

public class Ufo : Entity
{
    public int health = 20;

    public FixedPoint asteroidDetectionRadius;

    public FixedPoint orbitRadius;
    public FixedPoint orbitSpeedPerSecond = (FixedPoint) Mathf.PI * 2;
    public FixedPoint currentOrbitAngle = 0;

    public FixedPointVector3 originPosition = new FixedPointVector3(0, 0 , 0);
    public FixedPoint timeBetweenShots = 2;

    public Ufo(FixedPointVector3 _pos, FixedPoint _diameter, GameController _owner)
    {
        owner = _owner;

        type = EntityType.Ufo;

        originPosition = _pos;
        scale = _diameter;
        speed = (FixedPoint) 0.5f;
        orbitRadius = scale;
        asteroidDetectionRadius = _diameter * 5;

        body = new CircleBody(originPosition, _diameter * (FixedPoint)0.5f);

        body.layer = 0;
        body.collidesWithLayers[1] = true;
        body.owner = this;

        direction.X = (FixedPoint)Random.Range(-1f, 1f);
        direction.Y = (FixedPoint)Random.Range(-1f, 1f);

        if (Random.value > 0.5f)
            orbitSpeedPerSecond *= -1;

        var subscription = body.collisionStream.Listen(other =>
        {
            if (other.owner is Asteroid)
                deathEvent.Send();
        });

        //stop reacting to collisions once object is destroyed
        deathEvent.Listen(subscription.Dispose);
    }

    private FixedPoint _timeSinceLastShot = 0;

    public override void Step(FixedPoint dt)
    {
        originPosition += direction * dt * speed;
        currentOrbitAngle += orbitSpeedPerSecond * dt;

        position = originPosition + new FixedPointVector3(currentOrbitAngle.Sin(), currentOrbitAngle.Cos(), 0) * orbitRadius;
        body.position = position;

        //do the pew pews
        //i didn't bother with grid here because it doesn't happen every frame and has low locality
        if (_timeSinceLastShot >= timeBetweenShots)
        {
            var minDistance = (FixedPoint)999;
            Entity closestEntity = null;

            foreach (var other in owner.entities)
            {
                if(other == this)
                    continue;

                var distance = (position - other.position).Magnitude;

                if (other is Ufo)
                {
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestEntity = other;
                    }
                } else if (other is Asteroid)
                {
                    if (distance < minDistance && distance <= asteroidDetectionRadius)
                    {
                        minDistance = distance;
                        closestEntity = other;
                    }
                }
            }

            _timeSinceLastShot = 0;
        }

        _timeSinceLastShot += dt;
    }
}

public class Asteroid : Entity
{
    public int health = 3;

    public Asteroid(FixedPointVector3 _pos, FixedPoint _scale, GameController _owner)
    {
        owner = _owner;

        position = _pos;
        scale = _scale;
        speed = (FixedPoint)1.0f;

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
                direction = (body.position - other.position).Normalized * 5;
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