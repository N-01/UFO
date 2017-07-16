using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Logic;
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

    public class Connection
    {
        public Body other;
        public bool confirmed;

        public Connection(Body _other)
        {
            other = _other;
            confirmed = true;
        }
    }

    public class Body
    {
        public FixedPointVector3 position;
        public Entity owner;

        public int layer = 0; //number up to 32
        public BitVector32 collidesWithLayers;
        public Stream<Body> collisionStream = new Stream<Body>();

        private List<Connection> contacts = new List<Connection>(8);
        public Point occupiedTile;

        public Body(FixedPointVector3 pos) { position = pos; }

        public bool AddContact(Body other)
        {
            var existing = contacts.FirstOrDefault(c => c.other == other);
            if (existing != null)
            {
                existing.confirmed = true;
                return false;
            }

            collisionStream.Send(other);
            contacts.Add(new Connection(other));
            return true;
        }

        public void MakeContactsDirty()
        {
            foreach (var c in contacts)
            {
                c.confirmed = false;
            }
        }

        public void CleanupContacts()
        {
            contacts.RemoveAll(c => c.confirmed == false);
        }

        public bool IsColliding()
        {
            return contacts.Count > 0;
        }

        public bool CanCollideWith(Body other)
        {
            return collidesWithLayers[other.layer];
        }
    }

    public class CircleBody : Body
    {
        public FixedPoint radius;
        public CircleBody(FixedPointVector3 pos, FixedPoint r) : base(pos) { radius = r; }
    }

    public class OrientedBoxBody : Body
    {
        public FixedPoint angle;
        public FixedPoint width, height;

        public OrientedBoxBody(FixedPointVector3 pos, FixedPoint w, FixedPoint h, FixedPoint a) : base(pos)
        {
            angle = a;
            width = w;
            height = h;
        }
    }
}