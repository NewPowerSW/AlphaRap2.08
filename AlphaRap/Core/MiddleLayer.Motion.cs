namespace AlphaRap
{
    partial class MiddleLayer
    {
        /// <summary>打开全部模块的伺服，模块失败时报警并停止生产。</summary>
        public static void ServoOn()
        {
            ExecuteModules(module => module.ServoOn(), "ServoOn", "2011", true);
        }

        /// <summary>关闭全部模块的伺服，模块失败时报警并停止生产。</summary>
        public static void ServoOff()
        {
            ExecuteModules(module => module.ServoOff(), "ServoOff", "2012", true);
        }

    }
}
