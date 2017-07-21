using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Logic;
using Physics;
using UnityEngine;

public class EntityBehavior
{
	protected Entity entity;
	public EntityBehavior(Entity e)
	{
		entity = e;
	}

	public virtual void UpdateLogics(FixedPoint dt, GameController gc)
	{
		entity.position += entity.direction * dt * entity.speed;
		entity.body.position = entity.position;
	}

	public virtual void EnterCollision(Entity other) { }

	public virtual void ExitCollision(Entity other) { }

	public virtual bool IsColliding() { return false; }
}

public class UfoBehavior : EntityBehavior
{
	private FixedPoint _secondsSinceLastShot = 0;
	private Ufo ufo;
	private Entity cachedTarget = null;

	public UfoBehavior(Ufo e) : base(e)
	{
		ufo = e;
	}

	public override void UpdateLogics(FixedPoint dt, GameController gc)
	{
		if (_secondsSinceLastShot >= ufo.timeBetweenShots) {
			Shoot(gc);
			_secondsSinceLastShot = 0;
		}
		else
			_secondsSinceLastShot += dt;


		//I don't know if it's allowed to cache target until it's dead, but otherwise it'd be too wasteful
		if(cachedTarget == null || cachedTarget.dead)
			cachedTarget = gc.FindClosestEntity(entity, (e, f) => e is Ufo);

		if (cachedTarget != null)
		{
			var diff = (cachedTarget.position - entity.position);
			if (diff.Magnitude >= entity.scale * 2)
				ufo.direction = diff.Normalized;
			else
				ufo.direction = diff.Normalized.Turn;
		}
		else
			ufo.direction = new FixedPointVector3(0, 0, 0);

		ufo.originPosition += ufo.direction * dt * ufo.speed;
		ufo.currentOrbitAngle += ufo.orbitSpeedPerSecond * dt;

		ufo.position = ufo.originPosition + new FixedPointVector3(ufo.currentOrbitAngle.Sin(), ufo.currentOrbitAngle.Cos(), 0) * ufo.orbitRadius;
		ufo.body.position = ufo.position;
	}

	public override void EnterCollision(Entity other)
	{
		if (other is Asteroid)
			ufo.health = 0;
	}

	//do the pew pews
	//i didn't bother with grid here 
	//because it doesn't happen every frame and has low locality
	private void Shoot(GameController gc)
	{
		Entity closestEntity = gc.FindClosestEntity(entity, (e, dist) =>
		{
			if (e is Ufo)
				return true;

			if (e is Asteroid)
				return dist <= ufo.asteroidDetectionRadius;

			return false;
		});

		if (closestEntity != null)
		{
			FixedPointVector3 direction = (closestEntity.position - ufo.position).Normalized;
			gc.SpawnBlast(ufo.position + direction, direction, ufo);
		}
	}
}

public class AsteroidBehavior : EntityBehavior
{
	private Asteroid asteroid;
	public AsteroidBehavior(Asteroid e) : base(e)
	{
		asteroid = e;
	}

	public override void EnterCollision(Entity other)
	{
		if (other is Asteroid)
			asteroid.direction = (asteroid.position - other.position).Normalized;
	}
}

public class BlastBehavior : EntityBehavior
{
	private Blast blast;

	public BlastBehavior(Blast e) : base(e)
	{
		blast = e;
	}

	public override void EnterCollision(Entity other)
	{
		if (other != blast.source)
		{
			blast.health = 0;
			other.health--;
		}
	}
}

public class PlaceholderBehavior : EntityBehavior
{
	private Placeholder placeholder;
	private int collisionCounter = 0;

	public PlaceholderBehavior(Placeholder e) : base(e) { placeholder = e; }

	public override void EnterCollision(Entity other)
	{
		collisionCounter++;
	}

	public override void ExitCollision(Entity other)
	{
		collisionCounter--;
	}

	public override bool IsColliding()
	{
		return collisionCounter > 0;
	}
}

public class BehaviorController
{
	private GameController gameController;

	public BehaviorController(GameController gc)
	{
		gameController = gc;
	}

	public void SendCollisionEnter(Connection c)
	{
		c.first.owner.behavior.EnterCollision(c.second.owner);
		c.second.owner.behavior.EnterCollision(c.first.owner);
	}

	public void SendCollisionExit(Connection c)
	{
		c.first.owner.behavior.ExitCollision(c.second.owner);
		c.second.owner.behavior.ExitCollision(c.first.owner);
	}

	public void UpdateBehavior(FixedPoint dt)
	{
		foreach (var e in gameController.entities)
		{
			e.behavior.UpdateLogics(dt, gameController);
		}
	}
}
