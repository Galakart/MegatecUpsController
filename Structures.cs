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
                this.FixedCapacity = fixedCapacity;
            }

            public new T Enqueue(T item)
            {
                base.Enqueue(item);
                if (base.Count > FixedCapacity)
                {
                    return base.Dequeue();
                }
                return default;
            }
        }

    }
}
