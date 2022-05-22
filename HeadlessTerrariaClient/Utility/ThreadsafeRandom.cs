using System;

namespace HeadlessTerrariaClient.Utility
{
    /// <summary>
    /// A threadsafe random object
    /// </summary>
    public class ThreadSafeRandom
    {
        private static readonly Random _global = new Random();
        [ThreadStatic] private static Random _local;
        
        /// <returns>A random integer</returns>
        public int Next()
        {
            if (_local == null)
            {
                int seed;
                lock (_global)
                {
                    seed = _global.Next();
                }
                _local = new Random(seed);
            }

            return _local.Next();
        }

        /// <returns>A random integer from 0 to maxValue</returns>
        public int Next(int maxValue)
        {
            if (_local == null)
            {
                int seed;
                lock (_global)
                {
                    seed = _global.Next();
                }
                _local = new Random(seed);
            }

            return _local.Next(maxValue);
        }

        /// <returns>A random integer from minValue to maxValue</returns>
        public int Next(int minValue, int maxValue)
        {
            if (_local == null)
            {
                int seed;
                lock (_global)
                {
                    seed = _global.Next();
                }
                _local = new Random(seed);
            }

            return _local.Next(minValue, maxValue);
        }

        /// <returns>A random double between 0 and 1</returns>
        public double NextDouble()
        {
            if (_local == null)
            {
                int seed;
                lock (_global)
                {
                    seed = _global.Next();
                }
                _local = new Random(seed);
            }

            return _local.NextDouble();
        }

        /// <returns>A random float between 0 and 1</returns>
        public float NextFloat()
        {
            if (_local == null)
            {
                int seed;
                lock (_global)
                {
                    seed = _global.Next();
                }
                _local = new Random(seed);
            }

            return (float)_local.NextDouble();
        }

        /// <returns>A random arrat if bytes</returns>
        public void NextBytes(byte[] buffer)
        {
            if (_local == null)
            {
                int seed;
                lock (_global)
                {
                    seed = _global.Next();
                }
                _local = new Random(seed);
            }

            _local.NextBytes(buffer);
        }
    }
}