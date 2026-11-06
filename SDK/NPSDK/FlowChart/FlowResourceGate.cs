using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NPSDK
{
    public static class FlowResourceGate
    {
        private sealed class Slot
        {
            public readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);
            public volatile string Owner;
            public long Order;
        }

        private static readonly ConditionalWeakTable<object, Slot> slots = new ConditionalWeakTable<object, Slot>();
        private static readonly AsyncLocal<string> owner = new AsyncLocal<string>();
        private static long nextOrder;

        /// <summary>判断当前执行上下文是否可以操作资源；停止命令不受此限制。</summary>
        public static bool CanAccess(object resource)
        {
            Slot slot;
            if (!slots.TryGetValue(resource, out slot)) return true;
            return slot.Owner == null || slot.Owner == owner.Value;
        }

        /// <summary>立即取得单个手动资源；资源忙时返回 false，不阻塞界面。</summary>
        public static bool TryAcquire(object resource, out Lease lease)
        {
            lease = null;
            if (resource == null) return false;
            Slot slot = slots.GetValue(resource, key => new Slot { Order = Interlocked.Increment(ref nextOrder) });
            if (!slot.Semaphore.Wait(0)) return false;
            string id = Guid.NewGuid().ToString("N");
            slot.Owner = id;
            lease = new Lease(id, () => Release(new List<Slot> { slot }));
            return true;
        }

        /// <summary>按固定顺序取得资源，避免多个轴组反向加锁；等待可以取消。</summary>
        public static async Task<Lease> Acquire(IEnumerable<object> resources, CancellationToken token)
        {
            List<Slot> wanted = resources.Where(item => item != null).Distinct().Select(item =>
                slots.GetValue(item, key => new Slot { Order = Interlocked.Increment(ref nextOrder) })).OrderBy(item => item.Order).ToList();
            List<Slot> held = new List<Slot>();
            string id = Guid.NewGuid().ToString("N");
            try
            {
                foreach (Slot slot in wanted)
                {
                    await slot.Semaphore.WaitAsync(token).ConfigureAwait(false);
                    slot.Owner = id;
                    held.Add(slot);
                }
                return new Lease(id, () => Release(held));
            }
            catch { Release(held); throw; }
        }

        /// <summary>按相反顺序释放已取得的资源。</summary>
        private static void Release(List<Slot> held)
        {
            for (int index = held.Count - 1; index >= 0; index--)
            {
                held[index].Owner = null;
                held[index].Semaphore.Release();
            }
        }

        public sealed class Lease : IDisposable
        {
            private readonly string id;
            private Action release;
            /// <summary>保存资源租约释放方法。</summary>
            internal Lease(string value, Action cleanup) { id = value; release = cleanup; }
            /// <summary>在持有的手动资源上下文中执行同步指令，退出后恢复调用方身份。</summary>
            public T RunManual<T>(Func<T> action)
            {
                string previous = owner.Value;
                owner.Value = id;
                try { return action(); }
                finally { owner.Value = previous; }
            }
            /// <summary>在同一异步执行上下文内运行资源动作。</summary>
            public async Task<T> Run<T>(Func<Task<T>> action)
            {
                string previous = owner.Value;
                owner.Value = id;
                try { return await action().ConfigureAwait(false); }
                finally { owner.Value = previous; }
            }
            /// <summary>动作及停止确认结束后释放资源，只执行一次。</summary>
            public void Dispose() { Action cleanup = Interlocked.Exchange(ref release, null); if (cleanup != null) cleanup(); }
        }
    }
}
