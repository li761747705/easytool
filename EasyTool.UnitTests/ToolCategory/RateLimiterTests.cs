using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using EasyTool.ToolCategory;

namespace EasyTool.ToolCategory.Tests
{
    public class RateLimiterTests
    {
        // ==================== TryAcquire returns true when tokens available ====================

        [Fact]
        public void TryAcquire_returns_true_when_tokens_available()
        {
            var limiter = new TokenBucketRateLimiter(capacity: 10, refillRate: 1);
            Assert.True(limiter.TryAcquire());
        }

        // ==================== TryAcquire returns false when exhausted ====================

        [Fact]
        public void TryAcquire_returns_false_when_exhausted()
        {
            var limiter = new TokenBucketRateLimiter(capacity: 2, refillRate: 1);

            Assert.True(limiter.TryAcquire());
            Assert.True(limiter.TryAcquire());
            Assert.False(limiter.TryAcquire());
        }

        // ==================== Token refill over time ====================

        [Fact]
        public async Task Token_refill_over_time()
        {
            // capacity=2, refillRate=100 tokens/sec => ~1 token per 10ms
            var limiter = new TokenBucketRateLimiter(capacity: 2, refillRate: 100);

            // Exhaust tokens
            Assert.True(limiter.TryAcquire());
            Assert.True(limiter.TryAcquire());
            Assert.False(limiter.TryAcquire());

            // Wait long enough for at least one token to refill
            await Task.Delay(100);

            Assert.True(limiter.TryAcquire());
        }

        // ==================== Concurrent TryAcquire is thread-safe ====================

        [Fact]
        public void Concurrent_TryAcquire_thread_safe()
        {
            var capacity = 50;
            var limiter = new TokenBucketRateLimiter(capacity: capacity, refillRate: 0);
            var acquiredCount = 0;
            var threadCount = 8;
            var tasks = new List<Task>();

            for (int t = 0; t < threadCount; t++)
            {
                tasks.Add(Task.Run(() =>
                {
                    for (int i = 0; i < capacity; i++)
                    {
                        if (limiter.TryAcquire())
                        {
                            global::System.Threading.Interlocked.Increment(ref acquiredCount);
                        }
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            Assert.Equal(capacity, acquiredCount);
        }
    }
}
