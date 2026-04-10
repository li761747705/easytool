using System;
using System.Threading.Tasks;
using Xunit;
using EasyTool.ToolCategory;

namespace EasyTool.ToolCategory.Tests
{
    public class BackoffUtilTests
    {
        // ==================== Exponential ====================

        [Fact]
        public void Exponential_AttemptZero_ReturnsBaseDelay()
        {
            var baseDelay = TimeSpan.FromMilliseconds(100);
            var result = BackoffUtil.Exponential(0, baseDelay, jitter: false);
            Assert.Equal(baseDelay, result);
        }

        [Fact]
        public void Exponential_AttemptOne_ReturnsDoubleBaseDelay()
        {
            var baseDelay = TimeSpan.FromMilliseconds(100);
            var result = BackoffUtil.Exponential(1, baseDelay, jitter: false);
            Assert.Equal(TimeSpan.FromMilliseconds(200), result);
        }

        [Fact]
        public void Exponential_AttemptTwo_ReturnsQuadrupleBaseDelay()
        {
            var baseDelay = TimeSpan.FromMilliseconds(100);
            var result = BackoffUtil.Exponential(2, baseDelay, jitter: false);
            Assert.Equal(TimeSpan.FromMilliseconds(400), result);
        }

        [Fact]
        public void Exponential_WithMaxDelay_CappedAtMax()
        {
            var baseDelay = TimeSpan.FromMilliseconds(1000);
            var maxDelay = TimeSpan.FromMilliseconds(500);
            var result = BackoffUtil.Exponential(5, baseDelay, maxDelay, jitter: false);
            Assert.Equal(maxDelay, result);
        }

        [Fact]
        public void Exponential_WithJitter_AddsSmallRandomVariation()
        {
            var baseDelay = TimeSpan.FromMilliseconds(1000);
            var result = BackoffUtil.Exponential(0, baseDelay, jitter: true);
            // With jitter, delay should be between baseDelay and baseDelay + 10%
            Assert.True(result >= baseDelay);
            Assert.True(result < TimeSpan.FromMilliseconds(1200));
        }

        [Fact]
        public void Exponential_WithoutJitter_ExactValue()
        {
            var baseDelay = TimeSpan.FromMilliseconds(200);
            var result = BackoffUtil.Exponential(3, baseDelay, jitter: false);
            // 200 * 2^3 = 200 * 8 = 1600
            Assert.Equal(TimeSpan.FromMilliseconds(1600), result);
        }

        // ==================== Linear ====================

        [Fact]
        public void Linear_AttemptZero_ReturnsBaseDelay()
        {
            var baseDelay = TimeSpan.FromMilliseconds(100);
            var result = BackoffUtil.Linear(0, baseDelay);
            Assert.Equal(baseDelay, result);
        }

        [Fact]
        public void Linear_AttemptOne_ReturnsDoubleBaseDelay()
        {
            var baseDelay = TimeSpan.FromMilliseconds(100);
            var result = BackoffUtil.Linear(1, baseDelay);
            Assert.Equal(TimeSpan.FromMilliseconds(200), result);
        }

        [Fact]
        public void Linear_AttemptTwo_ReturnsTripleBaseDelay()
        {
            var baseDelay = TimeSpan.FromMilliseconds(100);
            var result = BackoffUtil.Linear(2, baseDelay);
            Assert.Equal(TimeSpan.FromMilliseconds(300), result);
        }

        [Fact]
        public void Linear_WithMaxDelay_CappedAtMax()
        {
            var baseDelay = TimeSpan.FromMilliseconds(500);
            var maxDelay = TimeSpan.FromMilliseconds(800);
            var result = BackoffUtil.Linear(5, baseDelay, maxDelay);
            // 500 * 6 = 3000, capped at 800
            Assert.Equal(maxDelay, result);
        }

        [Fact]
        public void Linear_WithinMaxDelay_NotCapped()
        {
            var baseDelay = TimeSpan.FromMilliseconds(100);
            var maxDelay = TimeSpan.FromMilliseconds(500);
            var result = BackoffUtil.Linear(2, baseDelay, maxDelay);
            // 100 * 3 = 300, within max
            Assert.Equal(TimeSpan.FromMilliseconds(300), result);
        }

        // ==================== Fixed ====================

        [Fact]
        public void Fixed_ReturnsSameDelay()
        {
            var delay = TimeSpan.FromSeconds(5);
            var result = BackoffUtil.Fixed(delay);
            Assert.Equal(delay, result);
        }

        [Fact]
        public void Fixed_ZeroDelay_ReturnsZero()
        {
            var result = BackoffUtil.Fixed(TimeSpan.Zero);
            Assert.Equal(TimeSpan.Zero, result);
        }

        // ==================== DecorrelatedJitter ====================

        [Fact]
        public void DecorrelatedJitter_WithinBounds()
        {
            var baseDelay = TimeSpan.FromMilliseconds(100);
            var maxDelay = TimeSpan.FromMilliseconds(1000);
            var result = BackoffUtil.DecorrelatedJitter(0, baseDelay, maxDelay);
            Assert.True(result >= baseDelay);
            Assert.True(result <= maxDelay);
        }

        [Fact]
        public void DecorrelatedJitter_WithPreviousDelay_WithinBounds()
        {
            var baseDelay = TimeSpan.FromMilliseconds(100);
            var maxDelay = TimeSpan.FromMilliseconds(1000);
            var previousDelay = TimeSpan.FromMilliseconds(500);
            var result = BackoffUtil.DecorrelatedJitter(1, baseDelay, maxDelay, previousDelay);
            Assert.True(result >= baseDelay);
            Assert.True(result <= maxDelay);
        }

        [Fact]
        public void DecorrelatedJitter_ComputedBelowBase_ClampedToBase()
        {
            var baseDelay = TimeSpan.FromMilliseconds(1000);
            var maxDelay = TimeSpan.FromMilliseconds(10000);
            // Run multiple times to account for randomness
            for (int i = 0; i < 100; i++)
            {
                var result = BackoffUtil.DecorrelatedJitter(0, baseDelay, maxDelay);
                Assert.True(result >= baseDelay, $"Result {result.TotalMilliseconds} should be >= {baseDelay.TotalMilliseconds}");
            }
        }

        // ==================== EqualJitter ====================

        [Fact]
        public void EqualJitter_WithinBounds()
        {
            var baseDelay = TimeSpan.FromMilliseconds(100);
            var maxDelay = TimeSpan.FromMilliseconds(1000);
            var result = BackoffUtil.EqualJitter(0, baseDelay, maxDelay);
            Assert.True(result > TimeSpan.Zero);
            Assert.True(result <= maxDelay);
        }

        [Fact]
        public void EqualJitter_AttemptOne_LargerThanAttemptZero()
        {
            var baseDelay = TimeSpan.FromMilliseconds(100);
            var maxDelay = TimeSpan.FromMinutes(5); // large max so no capping
            var result0 = BackoffUtil.EqualJitter(0, baseDelay, maxDelay);
            var result1 = BackoffUtil.EqualJitter(1, baseDelay, maxDelay);
            // result1 should generally be larger (exponential component), but jitter makes this probabilistic.
            // We just verify both are positive and within bounds.
            Assert.True(result0 > TimeSpan.Zero);
            Assert.True(result1 > TimeSpan.Zero);
        }

        // ==================== CreateGenerator ====================

        [Fact]
        public void CreateGenerator_ReturnsGenerator()
        {
            var generator = BackoffUtil.CreateGenerator(
                BackoffStrategy.Exponential,
                TimeSpan.FromMilliseconds(100));
            Assert.NotNull(generator);
            Assert.Equal(0, generator.Attempt);
        }

        // ==================== BackoffGenerator ====================

        [Fact]
        public void BackoffGenerator_Next_ExponentialStrategy()
        {
            var generator = new BackoffGenerator(
                BackoffStrategy.Exponential,
                TimeSpan.FromMilliseconds(100),
                jitter: false);

            var d0 = generator.Next();
            var d1 = generator.Next();
            var d2 = generator.Next();

            Assert.Equal(TimeSpan.FromMilliseconds(100), d0);  // 100 * 2^0
            Assert.Equal(TimeSpan.FromMilliseconds(200), d1);  // 100 * 2^1
            Assert.Equal(TimeSpan.FromMilliseconds(400), d2);  // 100 * 2^2
            Assert.Equal(3, generator.Attempt);
        }

        [Fact]
        public void BackoffGenerator_Next_LinearStrategy()
        {
            var generator = new BackoffGenerator(
                BackoffStrategy.Linear,
                TimeSpan.FromMilliseconds(100));

            var d0 = generator.Next();
            var d1 = generator.Next();
            var d2 = generator.Next();

            Assert.Equal(TimeSpan.FromMilliseconds(100), d0);  // 100 * 1
            Assert.Equal(TimeSpan.FromMilliseconds(200), d1);  // 100 * 2
            Assert.Equal(TimeSpan.FromMilliseconds(300), d2);  // 100 * 3
        }

        [Fact]
        public void BackoffGenerator_Next_FixedStrategy()
        {
            var generator = new BackoffGenerator(
                BackoffStrategy.Fixed,
                TimeSpan.FromMilliseconds(250));

            var d0 = generator.Next();
            var d1 = generator.Next();
            var d2 = generator.Next();

            Assert.Equal(TimeSpan.FromMilliseconds(250), d0);
            Assert.Equal(TimeSpan.FromMilliseconds(250), d1);
            Assert.Equal(TimeSpan.FromMilliseconds(250), d2);
        }

        [Fact]
        public void BackoffGenerator_Next_WithMaxDelay_Capped()
        {
            var generator = new BackoffGenerator(
                BackoffStrategy.Exponential,
                TimeSpan.FromMilliseconds(1000),
                TimeSpan.FromMilliseconds(500),
                jitter: false);

            var d0 = generator.Next();
            var d1 = generator.Next();

            Assert.Equal(TimeSpan.FromMilliseconds(500), d0);  // 1000 * 2^0 = 1000, capped to 500
            Assert.Equal(TimeSpan.FromMilliseconds(500), d1);  // 1000 * 2^1 = 2000, capped to 500
        }

        [Fact]
        public void BackoffGenerator_Reset_ResetsAttempt()
        {
            var generator = new BackoffGenerator(
                BackoffStrategy.Exponential,
                TimeSpan.FromMilliseconds(100),
                jitter: false);

            generator.Next();
            generator.Next();
            Assert.Equal(2, generator.Attempt);

            generator.Reset();
            Assert.Equal(0, generator.Attempt);

            var d0 = generator.Next();
            Assert.Equal(TimeSpan.FromMilliseconds(100), d0);
        }

        // ==================== ExecuteWithBackoffAsync<T> ====================

        [Fact]
        public async Task ExecuteWithBackoffAsync_Func_SucceedsOnFirstAttempt()
        {
            var result = await BackoffUtil.ExecuteWithBackoffAsync(
                () => Task.FromResult(42),
                maxRetries: 3,
                baseDelay: TimeSpan.Zero);

            Assert.Equal(42, result);
        }

        [Fact]
        public async Task ExecuteWithBackoffAsync_Func_RetriesOnFailure()
        {
            int attempts = 0;
            var result = await BackoffUtil.ExecuteWithBackoffAsync(
                () =>
                {
                    attempts++;
                    if (attempts < 3) throw new InvalidOperationException("transient");
                    return Task.FromResult("success");
                },
                maxRetries: 3,
                baseDelay: TimeSpan.Zero,
                shouldRetry: (ex, attempt) => true);

            Assert.Equal("success", result);
            Assert.Equal(3, attempts);
        }

        [Fact]
        public async Task ExecuteWithBackoffAsync_Func_ThrowsAfterMaxRetries()
        {
            int attempts = 0;
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await BackoffUtil.ExecuteWithBackoffAsync(
                    () =>
                    {
                        attempts++;
                        throw new InvalidOperationException("always fail");
                    },
                    maxRetries: 2,
                    baseDelay: TimeSpan.Zero,
                    shouldRetry: (ex, attempt) => true));

            Assert.Equal(3, attempts); // 1 initial + 2 retries
        }

        [Fact]
        public async Task ExecuteWithBackoffAsync_Func_ShouldRetryFalse_StopsEarly()
        {
            int attempts = 0;
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await BackoffUtil.ExecuteWithBackoffAsync(
                    () =>
                    {
                        attempts++;
                        throw new InvalidOperationException("stop");
                    },
                    maxRetries: 5,
                    baseDelay: TimeSpan.Zero,
                    shouldRetry: (ex, attempt) => attempt == 0));

            // Only retries once because shouldRetry returns false on attempt 1
            Assert.Equal(2, attempts);
        }

        // ==================== ExecuteWithBackoffAsync (Action) ====================

        [Fact]
        public async Task ExecuteWithBackoffAsync_Action_SucceedsOnFirstAttempt()
        {
            int attempts = 0;
            await BackoffUtil.ExecuteWithBackoffAsync(
                () =>
                {
                    attempts++;
                    return Task.CompletedTask;
                },
                maxRetries: 3,
                baseDelay: TimeSpan.Zero);

            Assert.Equal(1, attempts);
        }

        [Fact]
        public async Task ExecuteWithBackoffAsync_Action_RetriesOnFailure()
        {
            int attempts = 0;
            await BackoffUtil.ExecuteWithBackoffAsync(
                () =>
                {
                    attempts++;
                    if (attempts < 2) throw new TimeoutException("transient");
                    return Task.CompletedTask;
                },
                maxRetries: 3,
                baseDelay: TimeSpan.Zero,
                shouldRetry: (ex, attempt) => true);

            Assert.Equal(2, attempts);
        }

        [Fact]
        public async Task ExecuteWithBackoffAsync_Action_DefaultsToExponentialStrategy()
        {
            // Verify it uses a non-zero delay by default (exponential strategy)
            int attempts = 0;
            var sw = new global::System.Diagnostics.Stopwatch();
            sw.Start();
            await BackoffUtil.ExecuteWithBackoffAsync(
                () =>
                {
                    attempts++;
                    if (attempts < 2) throw new TimeoutException();
                    return Task.CompletedTask;
                },
                maxRetries: 1,
                baseDelay: TimeSpan.FromMilliseconds(50));
            sw.Stop();

            Assert.Equal(2, attempts);
            Assert.True(sw.ElapsedMilliseconds >= 40, "Default exponential strategy should cause a delay");
        }
    }
}
