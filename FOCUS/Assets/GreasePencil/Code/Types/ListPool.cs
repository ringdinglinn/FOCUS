using System;
using System.Collections.Generic;

namespace GreasePencil
{
	public static class ListPool<T>
	{
		// Object pool to avoid allocations.
		private static readonly ObjectPool<List<T>> s_ListPool = new ObjectPool<List<T>>(null, Clear);

        private static void Clear(List<T> list)
        {
            list.Clear();
        }

        public static List<T> Get()
		{
			return s_ListPool.Get();
		}

		public static void Release(List<T> toRelease)
		{
			s_ListPool.Release(toRelease);
		}

        public static bool TestAllocations ()
        {
            return s_ListPool.countActive > 0;
        }
    }

    public class PooledList<T> : System.IDisposable
    {
        public static PooledList<T> Get(out List<T> list)
        {
            var pool = new PooledList<T>();
            list = pool.list;
            return pool;
        }

        public static PooledList<T> Get(out List<T> list, List<T> prefillList)
        {
            var pool = new PooledList<T>();
            pool.list.AddRange(prefillList);
            list = pool.list;
            return pool;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        public List<T> list;

        public PooledList()
        {
            list = ListPool<T>.Get();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ListPool<T>.Release(list);
                    list = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
