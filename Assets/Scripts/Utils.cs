using System;
using System.Collections;
using System.Collections.Generic;
using Logic;
using UnityEngine;

namespace Utils
{
    public static class MathExt
    {
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        public static FixedPointVector3 RotatePoint(FixedPointVector3 p, FixedPointVector3 o, FixedPoint angle)
        {
            FixedPointVector3 r = new FixedPointVector3(p.X - o.X, p.Y - o.Y, 0);
            FixedPoint s = angle.Sin();
            FixedPoint c = angle.Cos();

            r.X = r.X * c - r.Y * s;
            r.Y = r.Y * c + r.X * s;

            r.X += o.X;
            r.Y += o.Y;

            return r;
        }
    }

}

public static class Extensions
{
    public static FixedPoint Range(FixedPoint from, FixedPoint to)
    {
        return (FixedPoint)UnityEngine.Random.Range(from, to);
    }

    public static FixedPoint Range(float from, float to)
    {
        return (FixedPoint)UnityEngine.Random.Range(from, to);
    }

    public static int IndexOf<T>(this T[] array, T value) where T : class
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == value) {
                return i;
            }
        }

        return -1;
    }

    public static bool PutIntoFreeSlot<T>(this T[] array, T value) where T : class
    {
        int index = array.IndexOf(null);
        if (index != -1)
        {
            array[index] = value;
            return true;
        }

        return false;
    }

    public static bool FreeSlot<T>(this T[] array, T value) where T : class
    {
        int index = array.IndexOf(value);
        if (index != -1)
        {
            array[index] = null;
            return true;
        }

        return false;
    }

    public static void FreeSlots<T>(this T[] array, Func<T, bool> pattern) where T : class
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (pattern(array[i]))
            {
                array[i] = null;
            }
        }
    }

    public static void Clear<T>(this T[] array) where T : class
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = null;
        }
    }

    public static IList<T> Swap<T>(this IList<T> list, int indexA, int indexB)
    {
        T tmp = list[indexA];
        list[indexA] = list[indexB];
        list[indexB] = tmp;
        return list;
    }
}
