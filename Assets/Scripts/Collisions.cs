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
    public class CollisionProcessor
    {
        List<Body> bodies = new List<Body>(32);

        public int gridSize = 8;
        public int gridCapacity = 8;

        private FixedPoint gridCellWidth, gridCellHeight;
        private FixedPoint sceneWidth, sceneHeight, boundarySize;

        Body[][][] colliderGrid;

        public List<Connection> contacts = new List<Connection>(32);

        public GameController gameController;
        public BehaviorController behaviorController;

        public CollisionProcessor(int _gridSize, 
                                  FixedPoint w, FixedPoint h, FixedPoint bounds,
                                  BehaviorController bc, GameController gc)
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
            gameController = gc;
        }

        public void AddCollider(Body c)
        {
            bodies.Add(c);
        }

        public void RemoveCollider<T>(T c) where T : Body
        {
            bodies.Remove(c);
        }

        public Connection CreateContact(Body first, Body second)
        {
            var existing = contacts.FirstOrDefault(c => c.first == first && c.second == second);

            if (existing != null)
            {
                existing.confirmed = true;
                return null;
            }

            var contact = new Connection(first, second);
            contacts.Add(contact);
            return contact;
        }

        public void UpdateCollisions()
        {
            //destroy objects otside of bounds
            bodies.Where(b => (b.position.X < -boundarySize || b.position.X > sceneWidth + boundarySize ||
                               b.position.Y < -boundarySize || b.position.Y > sceneHeight + boundarySize) && b.owner is Placeholder == false)
                              .ToList()
                              .ForEach(val => gameController.DestroyEntity(val.owner));

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
            foreach (var c in contacts) {
                c.confirmed = false;
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
                            if (CheckCollision(body, other)) {
                                behaviorController.SendCollisionEnter(CreateContact(body, other));
                            }
                        }
                    }
                }
            }

            foreach (var c in contacts.Where(c => !c.confirmed)) {
                behaviorController.SendCollisionExit(c);
            }
            contacts.RemoveAll(c => !c.confirmed);
        }

        public bool CheckCollision(Body first, Body second)
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