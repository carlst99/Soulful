using Soulful.Core.Model;
using System;
using Xunit;

namespace Soulful.Core.Tests.Model
{
    public class CyclicQueueTests
    {
        /// <summary>
        /// Should always have a value of two or more, to support the tests in <see cref="TestDequeue"/>
        /// </summary>
        private const int TEST_QUEUE_COUNT = 5;

        [Fact]
        public void TestCtor()
        {
            Assert.Throws<ArgumentNullException>(() => new CyclicCardQueue<int>(null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CyclicCardQueue<int>(Array.Empty<int>()));
        }

        [Fact]
        public void TestCountProperty()
        {
            Assert.Equal(TEST_QUEUE_COUNT, GetIntCyclicQueue().Count);
        }

        [Fact]
        public void TestContains()
        {
            Assert.True(GetIntCyclicQueue().Contains(TEST_QUEUE_COUNT - 1));
        }

        [Fact]
        public void TestDequeue()
        {
            CyclicCardQueue<int> cyclicQueue = GetIntCyclicQueue();
            for (int i = 0; i < TEST_QUEUE_COUNT; i++)
                Assert.Equal(i, cyclicQueue.Dequeue());
            Assert.Throws<InvalidOperationException>(() => cyclicQueue.Dequeue());
            cyclicQueue.Enqueue(TEST_QUEUE_COUNT - 2);
            Assert.Equal(TEST_QUEUE_COUNT - 2, cyclicQueue.Dequeue());
        }

        [Fact]
        public void TestEnqueue()
        {
            CyclicCardQueue<int> cyclicQueue = GetIntCyclicQueue();
            Assert.Throws<InvalidOperationException>(() => cyclicQueue.Enqueue(TEST_QUEUE_COUNT));
            // Other enqueue operations covered by TestDequeue()
        }

        [Fact]
        public void TestDispose()
        {
            CyclicCardQueue<int> cyclicQueue = GetIntCyclicQueue();
            int number = cyclicQueue.Dequeue();
            cyclicQueue.Dispose();
            Assert.Throws<ObjectDisposedException>(() => cyclicQueue.Dequeue());
            Assert.Throws<ObjectDisposedException>(() => cyclicQueue.Enqueue(0));
        }

        private CyclicCardQueue<int> GetIntCyclicQueue()
        {
            int[] numbers = new int[TEST_QUEUE_COUNT];
            for (int i = 0; i < TEST_QUEUE_COUNT; i++)
                numbers[i] = i;
            return new CyclicCardQueue<int>(numbers);
        }
    }
}
