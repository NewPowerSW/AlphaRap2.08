using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>
    /// 抽象类：设备抽象类，定义了设备通用的属性和方法，包括设备名、参数配置、打开、关闭、运行、停止等。
    /// 带参实例构造函数需提供设备名。
    ///  版本1.0 初始版本
    /// 修改时间2023-3-27----------------WZF
    /// </summary>
    public abstract class AbstractDevice
    {

        #region 1. 普通属性

        /// <summary>
        /// 抽象类的只读属性：设备名，作为 Ini 配置文件的 section。仅可在构造函数中初始化
        /// </summary>
        public string DeviceName { get; }


        /// <summary>
        /// 抽象类的只读属性：是否连接并打开，可在子类中重写。仅可在构造函数中初始化
        /// </summary>
        public virtual bool IsConnected { get; }


        /// <summary>
        /// 抽象类的只读属性：是否正在运行，可在子类中重写。仅可在构造函数中初始化
        /// </summary>
        public virtual bool IsRunning { get; }

        #endregion


        #region 2. 构造函数

        /// <summary>
        /// 实例构造函数：提供设备名，作为 Ini 配置文件的 section
        /// </summary>
        /// <param name="deviceName">设备名</param>
        public AbstractDevice(string deviceName)
        {
            DeviceName = deviceName; // 初始化设备名
        }

        #endregion



        #region 3.主要功能：打开、关闭、运行、停止设备

        /// <summary>
        /// 抽象类的方法：打开并连接设备，可在子类中重写
        /// </summary>
        public virtual void Open() { }


        /// <summary>
        /// 抽象类的方法：断开连接并关闭设备，可在子类中重写
        /// </summary>
        public virtual void Close() { }


        /// <summary>
        /// 抽象类的方法：设备开始运行，可在子类中重写
        /// </summary>
        public virtual void StartRunning() { }


        /// <summary>
        /// 抽象类的方法：设备停止运行，可在子类中重写
        /// </summary>
        public virtual void StopRunning() { }

        #endregion


        #region 4. 功能：弹窗显示异常

        /// <summary>
        /// 功能：仅子类可调用的方法：开新线程，弹窗显示异常
        /// </summary>
        /// <param name="foreword">异常主题</param>
        /// <param name="ex">被显示的异常</param>
        protected void ShowException(string foreword, Exception ex)
        {
            // 开新线程，避免阻塞当前线程
            Task.Run(() =>
            {
                // 提示文案走语言包（MiddleLayer.LangMsg）；异常详情（ex）保持原样
                string title = string.Format(
                    MiddleLayer.LangMsg("Device", "msg_DeviceExceptionTitle", "{0} 类的 [{1}] 异常", "Exception of class {0} in [{1}]", "Excepción de la clase {0} en [{1}]"),
                    this.GetType().Name, DeviceName);
                string body = ">> " + foreword + "\r\n\r\n"
                            + MiddleLayer.LangMsg("Device", "msg_DeviceExceptionBody", "异常信息：", "Exception details:", "Información de la excepción:")
                            + "\r\n" + ex;
                MessageBox.Show(body, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
				MiddleLayer.DataF.AddLogError($">> {foreword}\r\n\r\n异常信息：\r\n{ex}" + $"{this.GetType().Name} 类的 [{DeviceName}] 异常");

			});
        }

        #endregion


    }// class

}// namespace
