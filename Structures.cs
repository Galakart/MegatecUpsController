using System.Collections.Generic;

namespace MegatecUpsController
{
    static class Structures
    {
        public sealed class SizedQueue<T> : Queue<T>
        {
            public int FixedCapacity { get; }
            public SizedQueue(int fixedCapacity)
            {
                FixedCapacity = fixedCapacity;
            }

            public new T Enqueue(T item)
            {
                base.Enqueue(item);
                if (Count > FixedCapacity)
                {
                    return Dequeue();
                }
                return default;
            }
        }

    }
}
