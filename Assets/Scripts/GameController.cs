using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Logic;
using Utils;
using Physics;
using UnityEngine;

using FPRandom = Extensions;

public class GameController : MonoBehaviour
{
	public EntityRenderer viewController;
	public CollisionProcessor collisionProcessor;

	public FixedPoint boundarySize = 2;

	public FixedPoint sceneWidth = 18, sceneHeight = 10;
	public List<Entity> entities = new List<Entity>();

	public int maxAsteroids = 10;
	public FixedPoint asteroidSpawnInterval = (FixedPoint)0.5f;

	void Start ()
	{
		collisionProcessor = new CollisionProcessor(8, sceneWidth, sceneHeight);

		for (int i = 0; i < 10; i++) {
			SpawnUfo();
		}
	}

	private FixedPoint timeSinceAsteroidSpawned = 0;

	void Update () {
		FixedPoint dt = (FixedPoint)Time.deltaTime;

		foreach (var e in entities)
		{
			if (e.position.X < -boundarySize || e.position.X > sceneWidth + boundarySize ||
			   e.position.Y < -boundarySize || e.position.Y > sceneHeight + boundarySize)
				e.deathEvent.Send();
		}

		if (timeSinceAsteroidSpawned >= asteroidSpawnInterval && entities.Count(e => e.type == EntityType.Asteroid) < 10) {
			SpawnAsteroid();
			timeSinceAsteroidSpawned = 0;
		}

		foreach (var entity in entities) {
			entity.DoLogics(dt);
		}

		collisionProcessor.UpdateCollisions();
		viewController.UpdatePositions();

		timeSinceAsteroidSpawned += dt;

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
		var asteroid = new Asteroid(position, scale, this);

		asteroid.direction = (new FixedPointVector3(sceneWidth / 2, sceneHeight / 2, 0) - position).Normalized;
		asteroid.speed = sceneWidth / FPRandom.Range(5, 10);

		RegisterEntity(asteroid);
	}

	public void SpawnUfo()
	{
		var position = new FixedPointVector3(FPRandom.Range(0f, sceneWidth), FPRandom.Range(0.0f, sceneHeight), 0);
		var ufo = new Ufo(position, (FixedPoint)0.5f, this);

		RegisterEntity(ufo);
	}

	private void RegisterEntity(Entity e)
	{
		e.deathEvent.Listen(() =>
		{
			collisionProcessor.RemoveCollider(e.body);
			entities.Remove(e);
		});

		collisionProcessor.AddCollider(e.body);
		entities.Add(e);

		viewController.Show(e);
	}
}
