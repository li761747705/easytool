using System;
using System.Threading.Tasks;
using Xunit;
using EasyTool.ToolCategory;

namespace EasyTool.ToolCategory.Tests
{
    public class CircuitBreakerUtilTests
    {
        // ==================== ExecuteAsync success ====================

        [Fact]
        public async Task ExecuteAsync_success_action_executes()
        {
            var cb = CircuitBreakerUtil.Create(new CircuitBreakerOptions
            {
                FailureThreshold = 3,
                BreakDuration = TimeSpan.FromSeconds(1)
            });

            var result = await cb.ExecuteAsync(() => Task.FromResult(42));
            Assert.Equal(42, result);
        }

        // ==================== ExecuteAsync increments failure count ====================

        [Fact]
        public async Task ExecuteAsync_failure_increments_failure_count()
        {
            var cb = CircuitBreakerUtil.Create(new CircuitBreakerOptions
            {
                FailureThreshold = 3,
                BreakDuration = TimeSpan.FromSeconds(1)
            });

            for (int i = 0; i < 3; i++)
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    cb.ExecuteAsync<int>(() => throw new InvalidOperationException("fail")));
            }

            Assert.Equal(3, cb.FailureCount);
        }

        // ==================== Circuit opens after threshold failures ====================

        [Fact]
        public async Task Circuit_opens_after_threshold_failures()
        {
            var cb = CircuitBreakerUtil.Create(new CircuitBreakerOptions
            {
                FailureThreshold = 3,
                BreakDuration = TimeSpan.FromSeconds(10)
            });

            for (int i = 0; i < 3; i++)
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    cb.ExecuteAsync<int>(() => throw new InvalidOperationException("fail")));
            }

            Assert.Equal(CircuitState.Open, cb.State);

            await Assert.ThrowsAsync<CircuitBreakerOpenException>(() =>
                cb.ExecuteAsync<int>(() => Task.FromResult(0)));
        }

        // ==================== Circuit transitions to half-open after timeout ====================

        [Fact]
        public async Task Circuit_transitions_to_half_open_after_timeout()
        {
            var cb = CircuitBreakerUtil.Create(new CircuitBreakerOptions
            {
                FailureThreshold = 2,
                SuccessThreshold = 1,
                BreakDuration = TimeSpan.FromMilliseconds(100)
            });

            // Trip the circuit open
            for (int i = 0; i < 2; i++)
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    cb.ExecuteAsync<int>(() => throw new InvalidOperationException("fail")));
            }

            Assert.Equal(CircuitState.Open, cb.State);

            // Wait for break duration to elapse
            await Task.Delay(TimeSpan.FromMilliseconds(200));

            // Accessing State should transition to HalfOpen
            Assert.Equal(CircuitState.HalfOpen, cb.State);

            // In half-open state, a call should be allowed
            var result = await cb.ExecuteAsync(() => Task.FromResult("recovered"));
            Assert.Equal("recovered", result);
        }

        // ==================== ExecuteAsync success resets failure count ====================

        [Fact]
        public async Task ExecuteAsync_success_resets_failure_count()
        {
            var cb = CircuitBreakerUtil.Create(new CircuitBreakerOptions
            {
                FailureThreshold = 5,
                BreakDuration = TimeSpan.FromSeconds(10)
            });

            // Accumulate 3 failures
            for (int i = 0; i < 3; i++)
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    cb.ExecuteAsync<int>(() => throw new InvalidOperationException("fail")));
            }

            Assert.Equal(3, cb.FailureCount);

            // A single success should reset the failure count
            await cb.ExecuteAsync(() => Task.FromResult(99));

            Assert.Equal(0, cb.FailureCount);
        }

        // ==================== ExecuteAsync propagates original exception ====================

        [Fact]
        public async Task ExecuteAsync_propagates_original_exception()
        {
            var cb = CircuitBreakerUtil.Create(new CircuitBreakerOptions
            {
                FailureThreshold = 5,
                BreakDuration = TimeSpan.FromSeconds(10)
            });

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                cb.ExecuteAsync<int>(() => throw new ArgumentNullException("param", "arg was null")));

            Assert.Equal("param", ex.ParamName);
        }
    }
}
