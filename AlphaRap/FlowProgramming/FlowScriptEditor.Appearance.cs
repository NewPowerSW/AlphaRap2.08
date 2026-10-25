using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AlphaRap.FlowProgramming
{
    public partial class FlowScriptEditor
    {
        private readonly List<int> sourceLineStarts = new List<int> { 0 };
        private bool lineMapDirty = true;
        private bool appearanceInitialized;
        private const int GetSourceScrollPosition = 0x04DD;
        private const int SetSourceScrollPosition = 0x04DE;
        private const int SetSourceTargetDevice = 0x0448;

        [StructLayout(LayoutKind.Sequential)]
        private struct SourceScrollPoint { internal int X, Y; }

        /// <summary>发送原生编辑器消息，不重建窗口句柄或替换文本。</summary>
        [DllImport("user32.dll", EntryPoint = "SendMessageW")]
        private static extern IntPtr SourceMessage(IntPtr window, int message, IntPtr parameter, IntPtr value);

        /// <summary>读取或恢复编辑器像素滚动位置。</summary>
        [DllImport("user32.dll", EntryPoint = "SendMessageW")]
        private static extern IntPtr SourcePointMessage(IntPtr window, int message, IntPtr parameter, ref SourceScrollPoint value);

        /// <summary>连接行号和换行显示事件；不修改文本、选择区或撤销栈。</summary>
        private void InitializeAppearance()
        {
            if (appearanceInitialized || UiTheme.IsDesignTime) return;
            appearanceInitialized = true;
            lineNumbers.Paint += PaintSourceLines;
            source.TextChanged += SourceLinesChanged;
            source.VScroll += SourceViewportChanged;
            source.HScroll += SourceViewportChanged;
            source.Resize += SourceViewportChanged;
            source.SelectionChanged += SourceViewportChanged;
            source.FontChanged += SourceViewportChanged;
            source.HandleCreated += SourceHandleCreated;
            sourceWrap.CheckedChanged += SourceWrapChanged;
            if (source.IsHandleCreated) ApplySourceWrap();
        }

        /// <summary>源码改变时只失效逻辑行索引，实际重建合并到下一次绘制。</summary>
        private void SourceLinesChanged(object sender, EventArgs args)
        {
            lineMapDirty = true;
            lineNumbers.Invalidate();
        }

        /// <summary>滚动、选择或尺寸改变后按真实字符位置重绘行号。</summary>
        private void SourceViewportChanged(object sender, EventArgs args) { lineNumbers.Invalidate(); }

        /// <summary>原生编辑器句柄建立后应用当前换行选项。</summary>
        private void SourceHandleCreated(object sender, EventArgs args) { ApplySourceWrap(); }

        /// <summary>切换视觉换行，不写入源码换行符或清空编辑撤销记录。</summary>
        private void SourceWrapChanged(object sender, EventArgs args) { ApplySourceWrap(); }

        /// <summary>直接切换 RichEdit 客户区换行，避免托管属性重建句柄损失撤销历史。</summary>
        private void ApplySourceWrap()
        {
            if (!source.IsHandleCreated || source.IsDisposed) return;
            SourceScrollPoint scroll = new SourceScrollPoint();
            SourcePointMessage(source.Handle, GetSourceScrollPosition, IntPtr.Zero, ref scroll);
            SourceMessage(source.Handle, SetSourceTargetDevice, IntPtr.Zero, sourceWrap.Checked ? IntPtr.Zero : new IntPtr(1));
            SourcePointMessage(source.Handle, SetSourceScrollPosition, IntPtr.Zero, ref scroll);
            source.Invalidate(); lineNumbers.Invalidate();
        }

        /// <summary>按真实源码的 LF 建立逻辑行索引，自动折行仍属于同一行。</summary>
        private void RefreshSourceLineMap()
        {
            if (!lineMapDirty) return;
            string text = source.Text;
            sourceLineStarts.Clear(); sourceLineStarts.Add(0);
            for (int index = 0; index < text.Length; index++)
                if (text[index] == '\n') sourceLineStarts.Add(index + 1);
            lineMapDirty = false;
            int digits = sourceLineStarts.Count.ToString().Length;
            float width = TextRenderer.MeasureText(new string('0', digits), source.Font, Size.Empty, TextFormatFlags.NoPadding).Width + 16;
            sourceLayout.ColumnStyles[0].Width = Math.Max(50, width);
        }

        /// <summary>仅绘制可见逻辑行首，行号与编辑器真实滚动及自动折行保持一致。</summary>
        private void PaintSourceLines(object sender, PaintEventArgs args)
        {
            if (!source.IsHandleCreated || source.IsDisposed) return;
            RefreshSourceLineMap();
            int firstCharacter = source.GetCharIndexFromPosition(new Point(1, 1));
            int firstLine = sourceLineStarts.BinarySearch(firstCharacter);
            if (firstLine < 0) firstLine = Math.Max(0, ~firstLine - 1);
            int currentLine = sourceLineStarts.BinarySearch(source.SelectionStart);
            if (currentLine < 0) currentLine = Math.Max(0, ~currentLine - 1);
            for (int index = firstLine; index < sourceLineStarts.Count; index++)
            {
                Point position = source.GetPositionFromCharIndex(sourceLineStarts[index]);
                if (position.Y > source.ClientSize.Height) break;
                if (position.Y + source.Font.Height < 0) continue;
                Rectangle bounds = new Rectangle(0, position.Y, Math.Max(1, lineNumbers.ClientSize.Width - 7), source.Font.Height);
                TextRenderer.DrawText(args.Graphics, (index + 1).ToString(), source.Font, bounds,
                    index == currentLine ? Color.FromArgb(0, 95, 155) : Color.FromArgb(112, 119, 128),
                    TextFormatFlags.Right | TextFormatFlags.Top | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            }
            using (Pen border = new Pen(Color.FromArgb(218, 224, 231)))
                args.Graphics.DrawLine(border, lineNumbers.Width - 1, 0, lineNumbers.Width - 1, lineNumbers.Height);
        }

        /// <summary>解除阅读显示事件，不拥有任何硬件、脚本或运行任务。</summary>
        private void DisposeAppearance()
        {
            if (!appearanceInitialized) return;
            appearanceInitialized = false;
            lineNumbers.Paint -= PaintSourceLines;
            source.TextChanged -= SourceLinesChanged;
            source.VScroll -= SourceViewportChanged;
            source.HScroll -= SourceViewportChanged;
            source.Resize -= SourceViewportChanged;
            source.SelectionChanged -= SourceViewportChanged;
            source.FontChanged -= SourceViewportChanged;
            source.HandleCreated -= SourceHandleCreated;
            sourceWrap.CheckedChanged -= SourceWrapChanged;
        }
    }
}
