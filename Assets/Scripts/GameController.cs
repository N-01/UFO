using System.Collections;
using System.Collections.Generic;
using Logic;
using Physics;
using UnityEngine;

public class GameController : MonoBehaviour
{
	public EntityViewSpawner viewSpawner;
	public CollisionProcessor collisionProcessor;

	public FixedPoint sceneWidth = 18, sceneHeight = 10;

	List<Entity> entities = new List<Entity>();

	void Start ()
	{
		collisionProcessor = new CollisionProcessor(8, sceneWidth, sceneHeight);


		for (int i = 0; i < 15; i++)
		{
			SpawnAsteroid();
		}

		foreach (var e in entities)
		{
			viewSpawner.Create(e);
		}
	}

	void Update () {
		foreach (var entity in entities) {
			entity.Step((FixedPoint)Time.deltaTime);
		}
		collisionProcessor.Step();
		viewSpawner.UpdatePositions();
	}

	public void SpawnAsteroid()
	{
		var position = new FixedPointVector3(Random.Range(0f, sceneWidth), Random.Range(0.0f, sceneHeight), 0);
		var scale = (FixedPoint)Random.Range(1.0f, 1.5f);
		var asteroid = new Asteroid(position, scale);



		collisionProcessor.AddCollider(asteroid.body);
		entities.Add(asteroid);
	}
}
