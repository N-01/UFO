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
    public FixedPoint scale = 1;
    public FixedPoint speed = 1;

    public GameController owner;
    public Body body;

    public OnceEmptyStream deathEvent = new OnceEmptyStream();

    public FixedPointVector3 direction = new FixedPointVector3(0.01f, 0.01f, 0);

    public virtual void DoLogics(FixedPoint dt)
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
        speed = (FixedPoint) 0.5f;
        orbitRadius = scale;
        asteroidDetectionRadius = _diameter * 5;

        body = new CircleBody(originPosition, _diameter * (FixedPoint)0.5f);

        body.layer = 0;
        body.collidesWithLayers[1 << 1] = true;
        body.owner = this;
        
        //randomize rotations
        if (Random.value > 0.5f)
            orbitSpeedPerSecond = -orbitSpeedPerSecond;

        currentOrbitAngle = Extensions.Range(0, Mathf.PI * 2);

        //destroy on collision with asteroid
        var subscription = body.collisionStream.Listen(other => {
            if (other.owner is Asteroid)
                deathEvent.Send();
        });

        //stop reacting to collisions once object is destroyed
        deathEvent.Listen(subscription.Dispose);
    }

    private FixedPoint _secondsSinceLastShot = 0;

    public override void DoLogics(FixedPoint dt)
    {
        if (health < 1)
        {
            deathEvent.Send();
            return;
        }

        if (_secondsSinceLastShot >= timeBetweenShots) {
            Shoot();
            _secondsSinceLastShot = 0;
        } else
            _secondsSinceLastShot += dt;

        originPosition += direction * dt * speed;
        currentOrbitAngle += orbitSpeedPerSecond * dt;

        position = originPosition + new FixedPointVector3(currentOrbitAngle.Sin(), currentOrbitAngle.Cos(), 0) * orbitRadius;
        body.position = position;
    }

    //do the pew pews
    //i didn't bother with grid here 
    //because it doesn't happen every frame and has low locality
    private void Shoot()
    {
        var minDistance = (FixedPoint)999;
        Entity closestEntity = null;

        foreach (var other in owner.entities)
        {
            if (other == this)
                continue;

            bool closer = false;
            var distance = (position - other.position).Magnitude;

            if (other is Ufo)
            {
                closer = distance < minDistance;
            }
            else if (other is Asteroid)
            {
                closer = distance < minDistance && distance <= asteroidDetectionRadius;
            }

            if (closer)
            {
                minDistance = distance;
                closestEntity = other;
            }
        }
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

        body = new CircleBody(position, scale * (FixedPoint)0.5f);

        body.layer = 1;
        body.collidesWithLayers[0 << 1] = true;
        body.collidesWithLayers[1 << 1] = true;
        body.owner = this;

        var subscription = body.collisionStream.Listen(other =>
        {
            if (other.owner is Asteroid)
                direction = (body.position - other.position).Normalized;
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