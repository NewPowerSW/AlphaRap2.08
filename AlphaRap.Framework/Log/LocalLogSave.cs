using System;
using System.IO;
using System.Text;

namespace AlphaRap
{

    /// <summary>
    /// 静态类：本地日志保存。将记录追加到指定目录下的运行/错误日志文件
    /// </summary>
    public static class LocalLogSave {
                
        #region 2. 静态构造函数

        // 静态构造函数：首次被调用静态类时，自动调用此方法
        static LocalLogSave() {
            Type staticType = typeof(LocalLogSave); //本静态类的类型

          
        }

        #endregion


        #region 3. 可配置的属性

        /// <summary>
        /// 可配置的属性：运行日志的目录，使用相对路径。默认值为 .\LogRun\
        /// </summary>
   
        public static string 运行日志目录 {
            get {
                return _runLogDirectory;
            }
            set {
                //创建赋值进来的目录
                if (!Directory.Exists(value)) {
                    try {
                        Directory.CreateDirectory(value);
                    }
                    catch {
                        return;
                    }
                }
                _runLogDirectory = value;
            }
        }
        private static string _runLogDirectory = @".\LogRun\";


        /// <summary>
        /// 可配置的属性：错误日志的目录，使用相对路径。默认值为 .\LogError\
        /// </summary>
   
        public static string 错误日志目录 {
            get {
                return _errorLogDirectory;
            }
            set {
                //创建赋值进来的目录
                if (!Directory.Exists(value)) {
                    try {
                        Directory.CreateDirectory(value);
                    }
                    catch {
                        return;
                    }
                }
                _errorLogDirectory = value;
            }
        }
        private static string _errorLogDirectory = @".\LogError\";

        #endregion


        #region 4. 主要功能：保存日志

        /// <summary>
        /// 主要功能：静态方法：向运行日志追加一行记录。日志文件按天生成
        /// </summary>
        public static void WriteRunLog(string text) {

            lock (_lockObj1) {
                //按天生成文件地址
                string filePath = _runLogDirectory + DateTime.Now.ToString("yyyy-MM-dd") + " 运行日志.log";
                try {
                    AppendLogToFile(filePath, text);
                }
                catch {
                }
            }//lock
        }
        private static object _lockObj1 = new object(); //用于锁线程


        /// <summary>
        /// 主要功能：静态方法：向错误日志追加一行记录。日志文件按天生成
        /// </summary>        
        public static void WriteErrorLog(string text) {

            lock (_lockObj2) {
                //按天生成文件地址
                string filePath = _errorLogDirectory + DateTime.Now.ToString("yyyy-MM-dd") + " 错误日志.log";
				FileInfo fi1 = new FileInfo(filePath);
				if (!fi1.Directory.Exists)   //如果文件不存在则创建文件
				{
					fi1.Directory.Create();
				}
				try {
                    AppendLogToFile(filePath, text); 
                }
                catch (Exception) {
                    
                }
            }//lock
        }
        private static object _lockObj2 = new object(); //用于锁线程

        #endregion


        #region 6. 私有方法：内部调用

        // 私有方法：将文本添加到指定文件末尾
        private static void AppendLogToFile(string filePath, string text) {

            //将待写的入数据从字符串转换为字节数组
            StringBuilder sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString("HH:mm:ss fff"));
            sb.Append("ms > ");
            sb.Append(text);
            sb.Append("\r\n\r\n");

            var charData = sb.ToString().ToCharArray();
            byte[] bytes = Encoding.GetEncoding(0).GetBytes(charData); //使用默认编码

            //创建或打开文件，使用using及时释放流对象
            using (FileStream fs = File.OpenWrite(filePath)) {
                try {
                    fs.Position = fs.Length; //设定书写的开始位置为文件的末尾 
                    fs.Write(bytes, 0, bytes.Length); //将待写入内容追加到文件末尾  
                }
                catch {
                }
            }
        }

        #endregion

    }// class

}// namespace
