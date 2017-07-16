using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Assets.Scripts.Logic.Math;
using Logic;
using UnityEngine;

namespace Physics
{
    public class CollisionProcessor
    {
        List<Body> bodies = new List<Body>();

        public int gridSize = 8;
        public int gridCapacity = 8;
        private FixedPoint gridCellWidth, gridCellHeight;

        Body[][][] colliderGrid;

        public CollisionProcessor(int _gridSize, FixedPoint w, FixedPoint h)
        {
            gridSize = _gridSize;
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
        }

        public void AddCollider(Body c)
        {
            bodies.Add(c as CircleBody);
        }

        public void RemoveCollider<T>(T c) where T : Body
        {
            bodies.Remove(c as CircleBody);
        }

        public void Step()
        {
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

                //put into grid including adjacent tiles
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
            foreach (var c in bodies) {
                c.MakeContactsDirty();
            }

            foreach (var body in bodies)
            {
                int x = body.occupiedTile.x;
                int y = body.occupiedTile.y;

                //ignore objects outside grid
                if (!(x >= 0 && x < gridSize && y > 0 && y < gridSize))
                    continue;

                foreach (var other in colliderGrid[x][y].Where(o => o != null))
                {
                    if (other != body)
                    {
                        if (body.CanCollideWith(other))
                        {
                            if (CheckCollision(body, other))
                            {
                                body.AddContact(other);
                                other.AddContact(body);
                            }
                        }
                    }
                }
            }

            foreach (var c in bodies)
            {
                c.CleanupContacts();
            }
        }

        public bool CheckCollision(Body first, Body second)
        {
            if (first is CircleBody && second is CircleBody)
                return Circle2Circle((CircleBody)first, (CircleBody)second);


            return false;
        }

        private bool Circle2Circle(CircleBody first, CircleBody second)
        {
            FixedPoint rSum = (first.radius + second.radius);
            //lets imagine lib provides FixedPointVector2 and we don't do extra muls, but it looks better
            return (first.position - second.position).Magnitude <= rSum * rSum;
        }
    }
}