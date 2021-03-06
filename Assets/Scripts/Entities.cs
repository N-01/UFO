﻿using System;
using Logic;
using Physics;
using UnityEngine;
using Random = UnityEngine.Random;

public enum EntityType
{
	Ufo = 0,
	Asteroid = 1,
	Blast = 2,
	Placeholder = 3
}
[Serializable]
public class Entity
{
	public EntityType type;
	public FixedPointVector3 position;
	public FixedPoint angle = 0;
	public FixedPoint scale = 1;
	public FixedPoint speed = 1;

	public FixedPointVector3 direction = new FixedPointVector3(0.01f, 0.01f, 0);

	public int health = 1;

	public Body body;
	public EntityBehavior behavior;

	public bool dead {
		get { return health < 1; }
	}
}

public class Ufo : Entity
{
	public FixedPoint asteroidDetectionRadius;

	public FixedPoint orbitRadius;
	public FixedPoint orbitSpeedPerSecond = (FixedPoint) Mathf.PI * 2;
	public FixedPoint currentOrbitAngle = 0;

    public FixedPointVector3 currentOrbitedPosition;
	public FixedPointVector3 pushDirection;

	public FixedPointVector3 originPosition;
	public FixedPoint timeBetweenShots = FixedPoint.Float05;

	public Ufo(FixedPointVector3 _pos, FixedPoint _diameter)
	{
		type = EntityType.Ufo;

		originPosition = _pos;
		speed = Extensions.Range(0.5f, 1f);
		orbitRadius = scale;
		asteroidDetectionRadius = _diameter * 5;

		health = 20;

		body = new CircleBody(originPosition, _diameter * (FixedPoint)0.5f, this);

		body.SetLayer(type);
		body.SetCollidesWith(EntityType.Asteroid, true);
		body.SetCollidesWith(EntityType.Blast, true);
		body.SetCollidesWith(EntityType.Placeholder, true);
		body.SetCollidesWith(EntityType.Ufo, true);

		//randomize rotations
		if (Random.value > 0.5f)
			orbitSpeedPerSecond = -orbitSpeedPerSecond;

		currentOrbitAngle = Extensions.Range(0, Mathf.PI * 2);

		behavior = new UfoBehavior(this);
	}
}

public class Asteroid : Entity
{
	public Asteroid(FixedPointVector3 _pos, FixedPoint _scale)
	{
		position = _pos;
		scale = _scale;
		speed = Extensions.Range(1, 2);
		health = 5;
		type = EntityType.Asteroid;

		body = new CircleBody(position, scale * (FixedPoint)0.5f, this);

		body.SetLayer(type);
		body.SetCollidesWith(EntityType.Ufo, true);
		body.SetCollidesWith(EntityType.Asteroid, true);
		body.SetCollidesWith(EntityType.Blast, true);
		body.SetCollidesWith(EntityType.Placeholder, true);

		behavior = new  AsteroidBehavior(this);
	}
}

public class Blast : Entity
{
	public Entity source;

	public Blast(FixedPointVector3 pos, FixedPointVector3 dir, Entity _source)
	{
		position = pos;
		direction = dir;
		speed = Extensions.Range(2, 3);
		angle = (FixedPoint)(Mathf.Atan2(-dir.X.Float, dir.Y.Float));

		type = EntityType.Blast;
		body = new OrientedBoxBody(pos, (FixedPoint)0.13f, (FixedPoint)0.39f, -angle, this);
		scale = (FixedPoint) 0.25f;

		body.SetLayer(type);
		body.SetCollidesWith(EntityType.Ufo, true);
		body.SetCollidesWith(EntityType.Asteroid, true);

		behavior = new BlastBehavior(this);

		source = _source;
	}
}

public class Placeholder : Entity
{
	public Placeholder(FixedPoint _scale)
	{
		scale = _scale;
		type = EntityType.Placeholder;
		body = new OrientedBoxBody(position, scale, scale, 0, this);

		body.SetLayer(type);
		body.SetCollidesWith(EntityType.Ufo, true);
		body.SetCollidesWith(EntityType.Asteroid, true);

		behavior = new PlaceholderBehavior(this);
	}
}