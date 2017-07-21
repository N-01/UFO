using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Assets.Scripts.Logic.Math;
using Logic;
using UnityEngine;
using Utils;

namespace Physics
{
	public struct Point
	{
		public int x, y;

		public Point(int _x, int _y)
		{
			x = _x;
			y = _y;
		}
	}

	public struct Connection
	{
		public Body first;
		public Body second;
		public bool confirmed;

		public bool alive;

		public Connection(Body _first, Body _second)
		{
			first = _first;
			second = _second;
			confirmed = true;
			alive = true;
		}
	}

	public class CollisionProcessor
	{
		List<Body> bodies = new List<Body>(32);

		public int gridSize = 10;
		public int gridCapacity = 4;
		public const int contactCapacity = 32;

		private FixedPoint gridCellWidth, gridCellHeight;
		private FixedPoint sceneWidth, sceneHeight, boundarySize;

		Body[][][] colliderGrid;

		//use of struct and array are very ugly, but also fastest i could get
		public Connection[] contacts = new Connection[contactCapacity];

		public BehaviorController behaviorController;

		public CollisionProcessor(int _gridSize, 
								  FixedPoint w, FixedPoint h, FixedPoint bounds,
								  BehaviorController bc)
		{
			gridSize = _gridSize;

			sceneWidth = w;
			sceneHeight = h;

			boundarySize = bounds;

			gridCellWidth  = w / (FixedPoint)_gridSize;
			gridCellHeight = h / (FixedPoint)_gridSize;

			colliderGrid = new Body[gridSize][][];

			for (int i = 0; i < _gridSize; i++)
			{
				colliderGrid[i] = new Body[gridSize][];

				for (int j = 0; j < _gridSize; j++)
				{
					colliderGrid[i][j] = new Body[gridCapacity];
				}
			}

			behaviorController = bc;
		}

		public void AddCollider(Body c)
		{
			bodies.Add(c);
		}

		public void RemoveCollider<T>(T c) where T : Body
		{
			bodies.Remove(c);
		}

		public bool AddContact(Connection c)
		{
			for (int i = 0; i < contactCapacity; i++)
			{
				if (!contacts[i].alive)
				{
					contacts[i] = c;
					return true;
				}
			}
			return false;
		}

		public bool ContactConfirmExists(Body first, Body second)
		{
			for (int i = 0; i < contactCapacity; i++)
			{
				if (contacts[i].alive && contacts[i].first == first && contacts[i].second == second)
				{
					contacts[i].confirmed = true;
					return true;
				}
			}
			return false;
		}

		public void UpdateCollisions()
		{
			//destroy objects outside of bounds
			foreach (var b in bodies)
			{
				if ((b.position.X < -boundarySize || b.position.X > sceneWidth + boundarySize ||
					 b.position.Y < -boundarySize || b.position.Y > sceneHeight + boundarySize) &&
					 b.owner is Placeholder == false)
					 b.owner.dead = true;
			}

			//clean grid
			for (int x = 0; x < gridSize; x++)
			{
				for (int y = 0; y < gridSize; y++)
				{
					colliderGrid[x][y].Clear();
				}
			}

			//place objects into 2D grid
			foreach (var body in bodies)
			{
				int tileX = Mathfp.Floor(body.position.X / gridCellWidth);
				int tileY = Mathfp.Floor(body.position.Y / gridCellHeight);

				body.occupiedTile = new Point(tileX, tileY);

				//inject into grid, including adjacent tiles
				for (int x = tileX - 1; x <= tileX + 1; x++)
				{
					for (int y = tileY - 1; y <= tileY + 1; y++)
					{
						if (x >= 0 && x < gridSize && y >= 0 && y < gridSize)
							colliderGrid[x][y].PutIntoFreeSlot(body);
					}
				}

			}

			//check collisions
			for (int i = 0; i < contactCapacity; i++)
			{
				contacts[i].confirmed = false;
			}

			foreach (var body in bodies)
			{
				int x = body.occupiedTile.x;
				int y = body.occupiedTile.y;

				//ignore objects outside grid
				if (!(x >= 0 && x < gridSize && y > 0 && y < gridSize))
					continue;

				foreach (var other in colliderGrid[x][y])
				{
					if (other != null && other != body)
					{
						if (body.CanCollideWith(other) && PerformCollision(body, other) && !ContactConfirmExists(body, other))
						{
							var contact = new Connection(body, other);

							if (AddContact(contact))
								behaviorController.SendCollisionEnter(contact);
						}
					}
				}
			}

			for (int i = 0; i < contactCapacity; i++)
			{
				if (contacts[i].alive && contacts[i].confirmed == false)
				{
					behaviorController.SendCollisionExit(contacts[i]);
					contacts[i].alive = false;
				}
			}
		}

		public bool PerformCollision(Body first, Body second)
		{
			if (first is CircleBody && second is CircleBody)
				return CircleVsCircle((CircleBody)first, (CircleBody)second);
			if (first is OrientedBoxBody && second is CircleBody)
				return OOBvsCircle((OrientedBoxBody)first, (CircleBody)second);
			if (first is CircleBody && second is OrientedBoxBody)
				return OOBvsCircle((OrientedBoxBody)second, (CircleBody)first);


			return false;
		}

		//lets imagine lib provides FixedPointVector2
		private bool CircleVsCircle(CircleBody first, CircleBody second)
		{
			FixedPoint rSum = (first.radius + second.radius);
			return (first.position - second.position).Magnitude <= rSum * rSum;
		}

		private bool OOBvsCircle(OrientedBoxBody box, CircleBody circle)
		{
			FixedPointVector3 rotated = MathExt.RotatePoint(circle.position, box.position, 0);
			FixedPointVector3 relative = rotated - box.position;

			FixedPoint halfW = box.width * FixedPoint.Float05;
			FixedPoint halfH = box.height * FixedPoint.Float05;
			FixedPointVector3 clamped = new FixedPointVector3(
										relative.X.Clamp(-halfW, halfW),
										relative.Y.Clamp(-halfH, halfH),
										0);

			//rotate back
			FixedPointVector3 transformedBack = MathExt.RotatePoint(clamped, box.position, 0);
			transformedBack += box.position;

			return (circle.position - transformedBack).Magnitude <= circle.radius;
		}
	}
}