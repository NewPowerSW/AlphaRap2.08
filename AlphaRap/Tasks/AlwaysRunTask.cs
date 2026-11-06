namespace AlphaRap
{
    internal sealed class AlwaysRunTask
    {
        private readonly JTimer blinkTimer = new JTimer();
        private bool blinkIsOn;

        /// <summary>更新本站的闪烁时基，仿真时不执行外围任务。</summary>
        public void AlwaysRun()
        {
            if (SysPara.Simulation || !blinkTimer.IsOn(500)) return;
            blinkIsOn = !blinkIsOn;
            blinkTimer.Restart();
        }
    }
}
