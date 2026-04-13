using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyTool.IdentifierCategory;
using Xunit;

namespace EasyTool.Tests
{
    public class IdUtilConcurrencyTests
    {
        private const int TotalIds = 10_000;
        private const int ParallelTasks = 10;
        private const int IdsPerTask = TotalIds / ParallelTasks;

        [Fact]
        public void SnowflakeId_concurrent_uniqueness()
        {
            var ids = new ConcurrentBag<long>();

            Parallel.For(0, ParallelTasks, _ =>
            {
                for (int i = 0; i < IdsPerTask; i++)
                {
                    ids.Add(IdUtil.SnowflakeId());
                }
            });

            Assert.Equal(TotalIds, ids.Count);
            Assert.Equal(TotalIds, new HashSet<long>(ids).Count);
        }

        [Fact]
        public void UUID_SequentialGUID_concurrent_uniqueness()
        {
            var ids = new ConcurrentBag<Guid>();

            Parallel.For(0, ParallelTasks, _ =>
            {
                for (int i = 0; i < IdsPerTask; i++)
                {
                    ids.Add(IdUtil.UUID(UUIDStyle.SequentialGUID));
                }
            });

            Assert.Equal(TotalIds, ids.Count);
            Assert.Equal(TotalIds, new HashSet<Guid>(ids).Count);
        }

        [Fact]
        public void UUID_Sequence_concurrent_uniqueness()
        {
            var ids = new ConcurrentBag<Guid>();

            Parallel.For(0, ParallelTasks, _ =>
            {
                for (int i = 0; i < IdsPerTask; i++)
                {
                    ids.Add(IdUtil.UUID(UUIDStyle.Sequence));
                }
            });

            Assert.Equal(TotalIds, ids.Count);
            Assert.Equal(TotalIds, new HashSet<Guid>(ids).Count);
        }

        [Fact]
        public void TSID_concurrent_uniqueness()
        {
            var ids = new ConcurrentBag<long>();

            Parallel.For(0, ParallelTasks, _ =>
            {
                for (int i = 0; i < IdsPerTask; i++)
                {
                    ids.Add(TsidUtil.Generate());
                }
            });

            Assert.Equal(TotalIds, ids.Count);
            Assert.Equal(TotalIds, new HashSet<long>(ids).Count);
        }

        [Fact]
        public void ULID_concurrent_uniqueness()
        {
            var ids = new ConcurrentBag<string>();

            Parallel.For(0, ParallelTasks, _ =>
            {
                for (int i = 0; i < IdsPerTask; i++)
                {
                    ids.Add(UlidUtil.GenerateString());
                }
            });

            Assert.Equal(TotalIds, ids.Count);
            Assert.Equal(TotalIds, new HashSet<string>(ids).Count);
        }

        [Fact]
        public void ObjectId_concurrent_uniqueness()
        {
            var ids = new ConcurrentBag<string>();

            Parallel.For(0, ParallelTasks, _ =>
            {
                for (int i = 0; i < IdsPerTask; i++)
                {
                    ids.Add(IdUtil.ObjectId());
                }
            });

            Assert.Equal(TotalIds, ids.Count);
            Assert.Equal(TotalIds, new HashSet<string>(ids).Count);
        }
    }
}
