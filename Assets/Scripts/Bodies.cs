using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Logic;
using Utils;

namespace Physics
{
	public class Body
	{
		public FixedPointVector3 position;
		public Entity owner;

		public int layer = 0; //number up to 32
		public BitVector32 collidesWithLayers;

		public Point occupiedTile;

		public Body(FixedPointVector3 pos) { position = pos; }

		public void SetLayer(EntityType t)
		{
			layer = (int) t;
		}

		public void SetCollidesWith(EntityType t, bool state)
		{
			collidesWithLayers[1 << (int)t] = state;
		}

		public bool CanCollideWith(Body other)
		{
			return collidesWithLayers[1 << other.layer];
		}
	}

	public class CircleBody : Body
	{
		public FixedPoint radius;

		public CircleBody(FixedPointVector3 pos, FixedPoint r, Entity _owner) : base(pos)
		{
			radius = r;
			owner = _owner;
		}
	}

	public class OrientedBoxBody : Body
	{
		public FixedPoint angle;
		public FixedPoint width, height;

		public OrientedBoxBody(FixedPointVector3 pos, FixedPoint w, FixedPoint h, FixedPoint a, Entity _owner) : base(pos)
		{
			angle = a;
			width = w;
			height = h;
			owner = _owner;
		}
	}
}