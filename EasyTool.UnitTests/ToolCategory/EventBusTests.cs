using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyTool.ToolCategory;
using Xunit;

namespace EasyTool.Tests
{
    public class EventBusTests : IDisposable
    {
        public void Dispose()
        {
            EventBus.Clear<string>();
            EventBus.Clear<int>();
            EventBus.Clear<bool>();
        }

        [Fact]
        public void Subscribe_and_Publish_basic()
        {
            string? received = null;
            using var token = EventBus.Subscribe<string>(data => received = data);

            EventBus.Publish("hello");

            Assert.Equal("hello", received);
        }

        [Fact]
        public void Multiple_handlers_all_receive_event()
        {
            var results = new List<string>();
            using var t1 = EventBus.Subscribe<string>(data => results.Add("h1:" + data));
            using var t2 = EventBus.Subscribe<string>(data => results.Add("h2:" + data));
            using var t3 = EventBus.Subscribe<string>(data => results.Add("h3:" + data));

            EventBus.Publish("test");

            Assert.Equal(3, results.Count);
            Assert.Contains("h1:test", results);
            Assert.Contains("h2:test", results);
            Assert.Contains("h3:test", results);
        }

        [Fact]
        public void Unsubscribe_prevents_receiving()
        {
            int callCount = 0;
            var token = EventBus.Subscribe<int>(data => callCount++);

            EventBus.Publish(1);
            Assert.Equal(1, callCount);

            token.Unsubscribe();
            EventBus.Publish(2);
            Assert.Equal(1, callCount);
        }

        [Fact]
        public void Publish_with_no_subscribers_does_not_throw()
        {
            var ex = Record.Exception(() => EventBus.Publish(42));
            Assert.Null(ex);
        }

        [Fact]
        public void Exception_isolation()
        {
            var secondCalled = false;
            using var t1 = EventBus.Subscribe<string>(data => throw new InvalidOperationException("boom"));
            using var t2 = EventBus.Subscribe<string>(data => secondCalled = true);

            var ex = Assert.Throws<AggregateException>(() => EventBus.Publish("test"));
            Assert.Single(ex.InnerExceptions);
            Assert.IsType<InvalidOperationException>(ex.InnerExceptions[0]);
            Assert.True(secondCalled);
        }

        [Fact]
        public async Task PublishAsync_exception_isolation()
        {
            var secondCalled = false;
            using var t1 = EventBus.Subscribe<string>(data => throw new InvalidOperationException("async-boom"));
            using var t2 = EventBus.Subscribe<string>(data => secondCalled = true);

            var ex = await Assert.ThrowsAsync<AggregateException>(() => EventBus.PublishAsync("test"));
            Assert.Single(ex.InnerExceptions);
            Assert.IsType<InvalidOperationException>(ex.InnerExceptions[0]);
            Assert.True(secondCalled);
        }

        [Fact]
        public void Clear_removes_all_handlers()
        {
            int callCount = 0;
            using var t1 = EventBus.Subscribe<string>(data => callCount++);
            using var t2 = EventBus.Subscribe<string>(data => callCount++);

            EventBus.Publish("before");
            Assert.Equal(2, callCount);

            EventBus.Clear<string>();
            callCount = 0;

            EventBus.Publish("after");
            Assert.Equal(0, callCount);
        }

        [Fact]
        public void AddWord_concurrent()
        {
            const int threadCount = 10;
            const int handlersPerThread = 100;
            var tokens = new ConcurrentBag<SubscriptionToken>();
            var callCount = 0;

            Parallel.For(0, threadCount, _ =>
            {
                for (int i = 0; i < handlersPerThread; i++)
                {
                    var token = EventBus.Subscribe<bool>(data =>
                    {
                        global::System.Threading.Interlocked.Increment(ref callCount);
                    });
                    tokens.Add(token);
                }
            });

            EventBus.Publish(true);

            Assert.Equal(threadCount * handlersPerThread, callCount);

            foreach (var token in tokens)
            {
                token.Dispose();
            }
        }
    }
}
