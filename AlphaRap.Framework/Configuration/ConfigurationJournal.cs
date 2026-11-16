using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace AlphaRap.Configuration
{
    /// <summary>多文件提交的持久记录，进程异常退出后恢复完整旧版本或确认已提交新版本。</summary>
    public sealed class ConfigurationJournal
    {
        public sealed class Manifest
        {
            public int Version = 1;
            public string State = "Prepared";
            public List<Item> Files = new List<Item>();
        }
        public sealed class Item
        {
            public string Target, OriginalHash, PreparedHash, Backup;
            public bool Existed;
        }
        private static readonly byte[] entropy = Encoding.UTF8.GetBytes("AlphaRap.ConfigurationJournal.v1");
        private readonly string directory;
        private readonly Manifest manifest;
        internal bool IsCommitted { get; private set; }
        internal static Action<string> Checkpoint;
        public static string DirectoryPath { get { return Path.Combine(Directory.GetCurrentDirectory(), ".np-transactions"); } }

        /// <summary>绑定一份已落盘或将要建立的事务记录。</summary>
        private ConfigurationJournal(string directory, Manifest manifest) { this.directory = directory; this.manifest = manifest; IsCommitted = manifest.State != "Prepared"; }

        /// <summary>正式替换前持久化全部原始备份与准备内容摘要。</summary>
        internal static ConfigurationJournal Prepare(IList<ProjectConfiguration.PendingFile> files)
        {
            string directory = Path.Combine(DirectoryPath, Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(directory);
            var state = new Manifest();
            for (int i = 0; i < files.Count; i++)
            {
                ProjectConfiguration.PendingFile file = files[i];
                var item = new Item { Target = Path.GetFullPath(file.Path), Existed = file.Original != null,
                    OriginalHash = file.Original == null ? "" : Hash(file.Original), PreparedHash = HashFile(file.Temporary), Backup = i + ".bin" };
                if (item.Existed) WriteDurable(Path.Combine(directory, item.Backup), ProtectedData.Protect(file.Original, entropy, DataProtectionScope.CurrentUser));
                state.Files.Add(item);
            }
            var journal = new ConfigurationJournal(directory, state);
            journal.Persist();
            Checkpoint?.Invoke("Prepared");
            return journal;
        }

        /// <summary>替换前拒绝任何准备之后的外部编辑，不覆盖未知第三版本。</summary>
        internal void BeforeReplace(int index)
        {
            Item item = manifest.Files[index];
            string actual = File.Exists(item.Target) ? HashFile(item.Target) : "";
            if (actual != item.OriginalHash) throw new IOException(ConfigurationText.RecoveryConflict + " " + item.Target);
        }
        /// <summary>提供阶段观察入口，生产代码不依赖观察回调。</summary>
        internal void AfterReplace(int index) { Checkpoint?.Invoke("File:" + index); }
        /// <summary>所有正式文件已替换后持久记录提交完成，随后才允许发布内存版本。</summary>
        internal void MarkCommitted() { manifest.State = "Committed"; Persist(); IsCommitted = true; Checkpoint?.Invoke("Committed"); }
        /// <summary>内存发布完成后移除记录和加密备份。</summary>
        internal void MarkPublished() { manifest.State = "Published"; Persist(); Remove(); }

        /// <summary>中途失败时恢复原文件，未知外部变更会保留现场并阻断继续运行。</summary>
        internal void Rollback()
        {
            manifest.State = "Prepared"; Persist();
            foreach (Item item in manifest.Files.AsEnumerable().Reverse()) Restore(item);
            Remove();
        }

        /// <summary>启动读取配置前恢复未完成事务，提交标记存在时只接受完整的新文件集合。</summary>
        public static void RecoverPending()
        {
            if (!System.IO.Directory.Exists(DirectoryPath)) return;
            foreach (string directory in System.IO.Directory.GetDirectories(DirectoryPath))
            {
                string path = Path.Combine(directory, "transaction.xml");
                if (!File.Exists(path)) continue;
                Manifest manifest;
                using (var reader = XmlReader.Create(path, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }))
                    manifest = (Manifest)new XmlSerializer(typeof(Manifest)).Deserialize(reader);
                if (manifest.Version != 1 || manifest.Files.Any(item => !Path.IsPathRooted(item.Target) || Path.GetFileName(item.Backup) != item.Backup)) throw new InvalidDataException(ConfigurationText.RecoveryConflict);
                var journal = new ConfigurationJournal(directory, manifest);
                if (manifest.State == "Prepared") journal.Rollback();
                else if (manifest.State == "Committed" || manifest.State == "Published")
                {
                    foreach (Item item in manifest.Files)
                        if (!File.Exists(item.Target) || HashFile(item.Target) != item.PreparedHash) throw new IOException(ConfigurationText.RecoveryConflict + " " + item.Target);
                    journal.Remove();
                }
                else throw new InvalidDataException(ConfigurationText.RecoveryConflict);
            }
        }

        /// <summary>只覆盖可识别为本事务新版本的文件；已回滚条目可重复恢复。</summary>
        private void Restore(Item item)
        {
            string current = File.Exists(item.Target) ? HashFile(item.Target) : "";
            if (current == item.OriginalHash) return;
            if (current != item.PreparedHash) throw new IOException(ConfigurationText.RecoveryConflict + " " + item.Target);
            if (!item.Existed) { File.Delete(item.Target); return; }
            byte[] original = ProtectedData.Unprotect(File.ReadAllBytes(Path.Combine(directory, item.Backup)), entropy, DataProtectionScope.CurrentUser);
            if (Hash(original) != item.OriginalHash) throw new InvalidDataException(ConfigurationText.RecoveryConflict);
            string temporary = Path.Combine(Path.GetDirectoryName(item.Target), Guid.NewGuid().ToString("N") + ".restore");
            WriteDurable(temporary, original);
            try { if (File.Exists(item.Target)) File.Replace(temporary, item.Target, null); else File.Move(temporary, item.Target); }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }

        /// <summary>记录使用写穿和显式刷新，替换后才认为对应阶段已持久化。</summary>
        private void Persist()
        {
            using (var content = new MemoryStream())
            {
                new XmlSerializer(typeof(Manifest)).Serialize(content, manifest);
                string temporary = Path.Combine(directory, "transaction.tmp"), target = Path.Combine(directory, "transaction.xml");
                WriteDurable(temporary, content.ToArray());
                if (File.Exists(target)) File.Replace(temporary, target, null); else File.Move(temporary, target);
            }
        }
        /// <summary>只删除本事务已知文件，不递归删除目录。</summary>
        private void Remove()
        {
            foreach (Item item in manifest.Files)
            {
                string backup = Path.Combine(directory, item.Backup);
                if (File.Exists(backup)) File.Delete(backup);
            }
            string marker = Path.Combine(directory, "transaction.xml");
            if (File.Exists(marker)) File.Delete(marker);
            string temporary = Path.Combine(directory, "transaction.tmp");
            if (File.Exists(temporary)) File.Delete(temporary);
            System.IO.Directory.Delete(directory, false);
        }
        /// <summary>持久化一份完整文件内容。</summary>
        private static void WriteDurable(string path, byte[] contents)
        {
            using (var output = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough)) { output.Write(contents, 0, contents.Length); output.Flush(true); }
        }
        /// <summary>按实际文件字节计算摘要。</summary>
        private static string HashFile(string path) { using (var source = File.OpenRead(path)) using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(source)).Replace("-", ""); }
        /// <summary>对原始备份字节计算摘要。</summary>
        private static string Hash(byte[] content) { using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(content)).Replace("-", ""); }
    }
}
