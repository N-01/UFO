using System;
using Assets.Scripts.Logic.Math;
using UnityEngine;


namespace Logic{

    /// <summary>
    /// Логические координаты объекта.
    /// Аналог Vector3
    /// </summary>
    public struct FixedPointVector3 : IEquatable<FixedPointVector3>, IComparable<FixedPointVector3>{

        public FixedPoint X;
        public FixedPoint Y;
        public FixedPoint Z;

        /// <summary>
        /// Сумма параметров
        /// </summary>
        public FixedPoint Point{
            get { return X + Y + Z; }
        }

        #region ctr

        public FixedPointVector3(float x, float y, float z){
            X = (FixedPoint) x;
            Y = (FixedPoint) y;
            Z = (FixedPoint) z;
        }

        public FixedPointVector3(FixedPoint x, FixedPoint y, FixedPoint z){
            X = x;
            Y = y;
            Z = z;
        }

        public FixedPointVector3(Vector3 position){
            X = (FixedPoint) position.x;
            Y = (FixedPoint) position.y;
            Z = (FixedPoint) position.z;
        }

        public FixedPointVector3(FixedPointVector3 position){
            X = FixedPoint.CustomRaw(position.X.Raw);
            Y = FixedPoint.CustomRaw(position.Y.Raw);
            Z = FixedPoint.CustomRaw(position.Z.Raw);
        }

        #endregion

        #region Операции над векторами

        /// <summary>
        /// Скалярное произведение векторов
        /// </summary>
        /// <param name="a">Вектор A</param>
        /// <param name="b">Вектор B</param>
        /// <returns></returns>
        public static FixedPoint Dot(FixedPointVector3 a, FixedPointVector3 b){
            return a.X*b.X + a.Y*b.Y + a.Z*b.Z;
        }

        #endregion

        #region Перегрузка математических операций

        public static FixedPointVector3 operator *(FixedPointVector3 a, FixedPoint b){
            FixedPoint x = a.X*b;
            FixedPoint y = a.Y*b;
            FixedPoint z = a.Z*b;
            return new FixedPointVector3(x, y, z);
        }

        public static FixedPointVector3 operator /(FixedPointVector3 a, FixedPoint b){
            FixedPoint x = a.X/b;
            FixedPoint y = a.Y/b;
            FixedPoint z = a.Z/b;
            return new FixedPointVector3(x, y, z);
        }

        public static FixedPointVector3 operator +(FixedPointVector3 a, FixedPointVector3 b){
            FixedPoint x = a.X + b.X;
            FixedPoint y = a.Y + b.Y;
            FixedPoint z = a.Z + b.Z;
            return new FixedPointVector3(x, y, z);
        }

        public static FixedPointVector3 operator -(FixedPointVector3 a, FixedPointVector3 b){
            FixedPoint x = a.X - b.X;
            FixedPoint y = a.Y - b.Y;
            FixedPoint z = a.Z - b.Z;
            return new FixedPointVector3(x, y, z);
        }

        public static FixedPointVector3 operator *(FixedPointVector3 a, FixedPointVector3 b){
            FixedPoint x = a.X*b.X;
            FixedPoint y = a.Y*b.Y;
            FixedPoint z = a.Z*b.Z;
            return new FixedPointVector3(x, y, z);
        }

        public static FixedPointVector3 operator /(FixedPointVector3 a, FixedPointVector3 b){
            FixedPoint x = a.X/b.X;
            FixedPoint y = a.Y/b.Y;
            FixedPoint z = a.Z/b.Z;
            return new FixedPointVector3(x, y, z);
        }

        public static bool operator ==(FixedPointVector3 a, FixedPointVector3 b){
            if (a.X == b.X && a.Y == b.Y && a.Z == b.Z){
                return true;
            }
            else{
                return false;
            }
        }

        public static bool operator !=(FixedPointVector3 a, FixedPointVector3 b){
            if (a.X != b.X || a.Y != b.Y || a.Z != b.Z){
                return true;
            }
            else{
                return false;
            }
        }

        public static bool operator >(FixedPointVector3 a, FixedPointVector3 b){
            FixedPoint zero = (FixedPoint) 0;
            FixedPoint deltaX = a.X - b.X;
            FixedPoint deltaY = a.Y - b.Y;
            FixedPoint deltaZ = a.Z - b.Z;

            FixedPoint delta = deltaX + deltaY + deltaZ;

            if (delta > zero){
                return true;
            }

            return false;
        }

        public static bool operator <(FixedPointVector3 a, FixedPointVector3 b){
            FixedPoint zero = (FixedPoint) 0;
            FixedPoint deltaX = b.X - a.X;
            FixedPoint deltaY = b.Y - a.Y;
            FixedPoint deltaZ = b.Z - a.Z;

            FixedPoint delta = deltaX + deltaY + deltaZ;

            if (delta > zero){
                return true;
            }

            return false;
        }

        #endregion

        public bool Equals(FixedPointVector3 other){
            return this == other;
        }

        public int CompareTo(FixedPointVector3 other){
            return Point.CompareTo(other.Point);
        }

        /// <summary>
        /// Длина вектора
        /// </summary>
        public FixedPoint Magnitude{
            get{
                var sum = X*X + Y*Y + Z*Z;
                if (sum == 0){
                    return sum;
                }
                return sum.Sqrt();
            }
        }

        /// <summary>
        /// Нормализованный вектор
        /// </summary>
        public FixedPointVector3 Normalized{
            get{
                var magnitude = Magnitude;
                if (magnitude == 0){
                    return new FixedPointVector3(0, 0, 0);
                }
                
                FixedPoint x = X/magnitude;
                FixedPoint y = Y/magnitude;
                FixedPoint z = Z/magnitude;
                return new FixedPointVector3(x, y, z);
            }
        }

        public FixedPointVector3 Abs{
            get{
                FixedPoint x = Mathfp.Abs(X);
                FixedPoint y = Mathfp.Abs(Y);
                FixedPoint z = Mathfp.Abs(Z);
                return new FixedPointVector3(x, y, z);
            }
        }

        public FixedPointVector3 Turn{
            get{
                FixedPoint x = X * -1;
                FixedPoint y = Y * -1;
                FixedPoint z = Z * -1;
                return new FixedPointVector3(x, y, z);
            }
        }


		#region cast operator overloading

		public static implicit operator FixedPointVector3(Vector2 v) { return new FixedPointVector3(v.x, v.y, 0); }
		public static implicit operator FixedPointVector3(Vector3 v) { return new FixedPointVector3(v.x, v.y, v.z);	}
		public static implicit operator Vector3(FixedPointVector3 v) { return new Vector3(v.X, v.Y, v.Z); }

		public static explicit operator Vector2(FixedPointVector3 v) { return new Vector2(v.X, v.Y); }

		#endregion

	}

}