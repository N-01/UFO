using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Logic;
using Utils;
using Physics;
using UnityEngine;

using FPRandom = Extensions;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
	public EntityRenderer viewController;
	public CollisionProcessor collisionController;
	public BehaviorController behaviorController;

	public FixedPoint boundarySize = 2;

	public FixedPoint sceneWidth = 18, sceneHeight = 10;
	public List<Entity> entities = new List<Entity>();

	public int maxAsteroids = 10;
	public FixedPoint asteroidSpawnInterval = 1;

	private Placeholder mouseTarget;

	public void Start()
	{
		behaviorController = new BehaviorController(this);
		collisionController = new CollisionProcessor(8, sceneWidth, sceneHeight, boundarySize, behaviorController, this);

		Init();
	}

	void Init ()
	{
		mouseTarget = new Placeholder(2);
		RegisterEntity(mouseTarget);
	}

	private FixedPoint timeSinceAsteroidSpawned = 0;

	void Update () {
		FixedPoint dt = (FixedPoint)Time.deltaTime;

		var v3 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		mouseTarget.position = new FixedPointVector3(v3.x, v3.y, 0);

		entities.Where(e => e.health < 1).ToList().ForEach(DestroyEntity);

		if (timeSinceAsteroidSpawned >= asteroidSpawnInterval && entities.Count(e => e.type == EntityType.Asteroid) < 10)
		{
			SpawnAsteroid();
			timeSinceAsteroidSpawned = 0;
		}

		timeSinceAsteroidSpawned += dt;

		behaviorController.UpdateBehavior(dt);
		collisionController.UpdateCollisions();

		viewController.UpdateViews();

		if (Input.GetMouseButtonUp(0) && mouseTarget.behavior.IsColliding() == false)
		{
			SpawnUfo(mouseTarget.position);
		}

	}

	public Entity FindClosestEntity(Entity target, Func<Entity, float, bool> predicate)
	{
		var minDistance = (FixedPoint)999;
		Entity closestEntity = null;

		foreach (var other in entities)
		{
			if (other == target)
				continue;

			var distance = (target.position - other.position).Magnitude;

			if (distance < minDistance && predicate(other, distance))
			{
				minDistance = distance;
				closestEntity = other;
			}

		}

		return closestEntity;
	}

	public void SpawnAsteroid()
	{
		bool horizontal = Random.value > 0.5f;
		bool left = Random.value > 0.5f;
		bool top = Random.value > 0.5f;

		FixedPointVector3 position = new FixedPointVector3();

		if (horizontal)
		{
			position.X = FPRandom.Range(0, sceneWidth);
			position.Y = top ? -boundarySize : sceneHeight + boundarySize;
		}
		else
		{
			position.X = left ? -boundarySize : sceneWidth + boundarySize;
			position.Y = FPRandom.Range(0, sceneHeight);
		}

		var scale = FPRandom.Range(1.0f, 1.5f);
		var asteroid = new Asteroid(position, scale);

		asteroid.direction = (new FixedPointVector3(sceneWidth / 2, sceneHeight / 2, 0) - position).Normalized;
		asteroid.speed = sceneWidth / FPRandom.Range(5, 10);

		RegisterEntity(asteroid);
	}

	public void SpawnUfo(FixedPointVector3 pos)
	{
		RegisterEntity(new Ufo(pos, 1));
	}

	public void SpawnBlast(FixedPointVector3 pos, FixedPointVector3 dir, Entity source)
	{
		RegisterEntity(new Blast(pos, dir, source));
	}

	private void RegisterEntity(Entity e)
	{
		collisionController.AddCollider(e.body);
		entities.Add(e);

		viewController.Show(e);
	}

	public void DestroyEntity(Entity e)
	{
		collisionController.RemoveCollider(e.body);
		viewController.RecycleDelayed(e);
		entities.Remove(e);
	}

	public void Reset()
	{
		foreach (var e in entities.ToList())
		{
			DestroyEntity(e);
		}
	}
}
