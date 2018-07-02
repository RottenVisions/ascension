using System.Collections.Generic;
using Ascension.Networking;

class NetObjectPool<T> where T : NetObject, new()
{
    readonly Stack<T> pool = new Stack<T>();

    public T Acquire()
    {
        T obj;

        if (pool.Count > 0)
        {
            obj = pool.Pop();
        }
        else
        {
            obj = new T();
        }

#if DEBUG
        NetAssert.True(obj.pooled);
        obj.pooled = false;
#endif

        return obj;
    }

    public void Release(T obj)
    {
#if DEBUG
        NetAssert.False(obj.pooled);
        obj.pooled = true;
#endif

        pool.Push(obj);
    }
}
