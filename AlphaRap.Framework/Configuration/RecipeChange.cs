using System;
using System.Collections.Generic;
using System.Linq;

namespace AlphaRap.Configuration
{
    /// <summary>一份已准备的配方改变，应用失败时按相反顺序恢复旧状态。</summary>
    public sealed class RecipeChange : IDisposable
    {
        private readonly Action apply, rollback, release;
        private bool disposed;
        /// <summary>声明独立准备的内存改变及其补偿动作。</summary>
        public RecipeChange(Action apply, Action rollback, Action release = null)
        {
            this.apply = apply ?? throw new ArgumentNullException(nameof(apply));
            this.rollback = rollback ?? throw new ArgumentNullException(nameof(rollback));
            this.release = release;
        }
        /// <summary>应用本次已校验的模型。</summary>
        internal void Apply() { apply(); }
        /// <summary>恢复应用前模型，保留原数据源身份。</summary>
        internal void Rollback() { rollback(); }
        /// <summary>释放准备期间持有的临时资源。</summary>
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            try { release?.Invoke(); }
            catch { ProjectConfiguration.RequireRecovery(); throw; }
        }

        /// <summary>尝试释放所有准备对象，单个失败不跳过其他资源，失败后保持恢复封锁。</summary>
        internal static void Release(IEnumerable<RecipeChange> changes)
        {
            var errors = new List<Exception>();
            foreach (RecipeChange change in changes) try { change.Dispose(); } catch (Exception error) { errors.Add(error); }
            if (errors.Count > 0) throw new AggregateException(errors);
        }

        /// <summary>所有准备成功后才执行；失败的动作也纳入补偿，不能留下半次应用。</summary>
        public static void Commit(IEnumerable<RecipeChange> changes, Action publish)
        {
            RecipeChange[] all = changes.ToArray();
            var applied = new List<RecipeChange>();
            try
            {
                foreach (RecipeChange change in all) { applied.Add(change); change.Apply(); }
                publish();
            }
            catch (Exception error)
            {
                var errors = new List<Exception> { error };
                foreach (RecipeChange change in applied.AsEnumerable().Reverse())
                    try { change.Rollback(); } catch (Exception restore) { errors.Add(restore); }
                if (errors.Count > 1) { ProjectConfiguration.RequireRecovery(); throw new AggregateException(errors); }
                throw;
            }
            finally { Release(all); }
        }
    }
}
