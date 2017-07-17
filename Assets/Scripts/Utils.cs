using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class ActionDisposable : IDisposable
    {
        Action onDispose;

        public ActionDisposable(Action act)
        {
            onDispose = act;
        }

        public void Dispose()
        {
            onDispose();
        }
    }

    public interface IStream<T>
    {
        IDisposable Listen(Action<T> callback);
    }

    public class AnonymousStream<T> : IStream<T>
    {
        private readonly Func<Action<T>, IDisposable> _transformSubscription;

        public AnonymousStream(Func<Action<T>, IDisposable> subscriber)
        {
            _transformSubscription = subscriber;
        }

        public IDisposable Listen(Action<T> act)
        {
            return _transformSubscription(act);
        }
    }

    public class Stream<T> : IStream<T>
    {
        private List<Action<T>> _callbacks;

        public IDisposable Listen(Action<T> callback)
        {
            if (_callbacks == null)
                _callbacks = new List<Action<T>> {callback};
            else
                _callbacks.Add(callback);

            //not using Remove directly to avoid removing callback during foreach
            return new ActionDisposable(() => _callbacks[_callbacks.IndexOf(callback)] = null);
        }

        public void Send(T val)
        {
            if (_callbacks != null)
            {
                _callbacks.RemoveAll(c => c == null);

                foreach (var c in _callbacks)
                {
                    c(val);
                }
            }
        }

    
        public IStream<T2> Select<T2>(Func<T, T2> map)
        {
            return new AnonymousStream<T2>(action =>
            {
                return Listen(v => action(map(v)));
            });
        }
    }

    public class OnceEmptyStream
    {
        private List<Action> _callbacks;

        public IDisposable Listen(Action callback)
        {
            if (_callbacks == null)
                _callbacks = new List<Action> { callback };
            else
                _callbacks.Add(callback);

            return new ActionDisposable(() => _callbacks[_callbacks.IndexOf(callback)] = null);
        }

        public void Send()
        {
            if (_callbacks != null)
            {
                _callbacks.RemoveAll(c => c == null);

                foreach (var c in _callbacks)
                {
                    c();
                }

                _callbacks.Clear();
            }
        }
    }

    public static class Math {
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }
    }

}

public static class Extensions
{
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
}
