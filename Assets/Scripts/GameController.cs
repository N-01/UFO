using System;
using System.Collections.Generic;
using System.Linq;
using Logic;
using Physics;
using UnityEngine;

using FPRandom = Extensions;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
	public EntityRenderer viewController;
	public CollisionProcessor collisionController;
	public BehaviorController behaviorController;

	public MenuScreen mainMenu;

	public FixedPoint boundarySize = 2;
	public int maxAsteroids = 10;
	public FixedPoint asteroidSpawnInterval = 1;
	public FixedPoint ufoSpawnInterval = FixedPoint.Float01;

	public FixedPoint sceneWidth = 18, sceneHeight = 10;

	public bool paused = true;

	public List<Entity> entities = new List<Entity>();
	private Queue<Entity> entitiesToSpawn = new Queue<Entity>();

	private Placeholder mouseTarget;

	public void Start()
	{
		behaviorController = new BehaviorController(this);
		collisionController = new CollisionProcessor(8, sceneWidth, sceneHeight, boundarySize, behaviorController);

		mouseTarget = new Placeholder(2);
		MaterializeEntity(mouseTarget);

		mainMenu.start.onClick.AddListener(() =>
		{
			paused = false;
			mainMenu.gameObject.SetActive(false);
		});
	}

	private FixedPoint timeSinceUfoSpawned = 0;
	private FixedPoint timeSinceAsteroidSpawned = 0;

	void Update () {
		if(paused)
			return;

		FixedPoint dt = (FixedPoint)Time.deltaTime;

		//process ufo spawn near mouse region
		var v3 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		mouseTarget.position = new FixedPointVector3(v3.x, v3.y, 0);

		if (Input.GetMouseButtonUp(0) &&  timeSinceUfoSpawned > ufoSpawnInterval)
		{
			SpawnUfo(mouseTarget.position);
			timeSinceUfoSpawned = 0;
		}
		timeSinceUfoSpawned += dt;


		//spawn random asteroids
		if (timeSinceAsteroidSpawned >= asteroidSpawnInterval && entities.Count(e => e.type == EntityType.Asteroid) < 10)
		{
			SpawnAsteroid();
			timeSinceAsteroidSpawned = 0;
		}
		timeSinceAsteroidSpawned += dt;

		//process spawn queue
		while (entitiesToSpawn.Count > 0)
		{
			MaterializeEntity(entitiesToSpawn.Dequeue());
		}

		behaviorController.UpdateBehavior(dt);
		collisionController.UpdateCollisions();
		viewController.UpdateViews();

		//despawn dead stuff
		BringOutTheDead();
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

		entitiesToSpawn.Enqueue(asteroid);
	}

	public void SpawnUfo(FixedPointVector3 pos)
	{
		entitiesToSpawn.Enqueue(new Ufo(pos, 1));
	}

	public void SpawnBlast(FixedPointVector3 pos, FixedPointVector3 dir, Entity source)
	{
		entitiesToSpawn.Enqueue(new Blast(pos, dir, source));
	}

	private void MaterializeEntity(Entity e)
	{
		collisionController.AddCollider(e.body);
		entities.Add(e);

		viewController.Show(e);
	}

	public void BringOutTheDead()
	{
		//clean dead entities in single pass without linq or extra alloc
		if (entities.Count > 0)
		{
			int lastAlive = entities.Count;
			for (int b = 0; b < lastAlive; b++)
			{
				if (entities[b].dead || entities[b].health < 1)
				{
					DestroyEntity(entities[b]);

					for (int e = lastAlive - 1; e >= b; e--)
					{
						lastAlive = e;

						if (!entities[e].dead)
						{
							entities.Swap(b, e);
							break;
						}
					}
				}
			}

			if (lastAlive < entities.Count)
				entities.RemoveRange(lastAlive, entities.Count - lastAlive);
		}
	}

	public void DestroyEntity(Entity e)
	{
		collisionController.RemoveCollider(e.body);
		viewController.RecycleDelayed(e);
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
}
