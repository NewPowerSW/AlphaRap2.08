using AlphaRap.Configuration;
using AlphaRapLibrary;

namespace AlphaRap
{
    /// <summary>项目手动页面的公共基类，支持接收框架的动态 IO。</summary>
    public class ManualPage : ModuleBaseForm
    {
        /// <summary>将同一批 IO 实例绑定到项目页面的硬件面板。</summary>
        public virtual void BindResources(HardwareResources resources) { }
    }
}
