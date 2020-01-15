using System;
using System.Linq;

namespace Soulful.Core.Model
{
    /// <summary>
    /// A readonly, circular queue that marks dequeued items as not-for-use until they are re-enqueued
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class CyclicCardQueue<T> : IDisposable
    {
        private CyclicCardQueueItem<T>[] _items;
        private int _index;
        private bool _isDisposed;

        #region Properties

        /// <summary>
        /// Gets the number of items in this <see cref="CyclicCardQueue{T}"/>
        /// </summary>
        public int Count => _items.Length;

        #endregion

        public CyclicCardQueue(T[] items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (items.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(items), "Base array cannot be empty");
            _items = items.Select(t => new CyclicCardQueueItem<T>(t)).ToArray();
        }

        public bool Contains(T item) => _items.Select(i => i.Item).Contains(item);

        public T Dequeue(bool takeOutOfLoop = true)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(CyclicCardQueue<T>));

            if (_index == Count)
                _index = 0;

            int cycleCount = 0;
            while (!_items[_index].InLoop)
            {
                cycleCount++;
                _index++;
                if (cycleCount == Count)
                    throw new InvalidOperationException("All items have been dequeued");
            }

            if (takeOutOfLoop)
                _items[_index].InLoop = false;
            return _items[_index++].Item;
        }

        public void Enqueue(T item)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(CyclicCardQueue<T>));

            if (Contains(item))
            {
                for (int i = 0; i < Count; i++)
                {
                    if (_items[i].Item.Equals(item))
                        _items[i].InLoop = true;
                }
            }
            else
            {
                throw new InvalidOperationException("Item does not exist in queue");
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _items = null;
                _isDisposed = true;
            }
        }
    }

    internal struct CyclicCardQueueItem<T>
    {
        public T Item;
        public bool InLoop;

        public CyclicCardQueueItem(T item)
        {
            Item = item;
            InLoop = true;
        }

        public override string ToString() => Item.ToString();
    }
}
