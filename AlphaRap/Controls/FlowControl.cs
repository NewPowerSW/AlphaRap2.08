using System;
using System.Threading;
using NPSDK;

namespace AlphaRap
{
    public class FlowControl
    {
        public bool bStopWork = false; 
        Thread FlowControlThread = null; 

        public void StartThread()
        {
            bStopWork = false;
            FlowControlThread = new Thread(DoWork);
            FlowControlThread.Start();
        }

        public void StopThread()
        {
            if (FlowControlThread != null)
            {
                bStopWork = true;
                FlowControlThread.Join();
            }
        }

        public void DoWork()
        {
            int ScanTick = 0; 
            Int64 LastSecond = 0;
            Int64 TempSecond = 0;
            GetTickCountEx tick = new GetTickCountEx();
            while (!bStopWork)
            {
                Execute();

                Thread.Sleep(5);
                ScanTick++;
                TempSecond = tick.Value;
                if ((TempSecond - LastSecond) >= 1000)
                {
                    LastSecond = TempSecond;
                    if (SysPara.SystemMode == RunMode.RUN)
                    {
                        SysPara.OperationSecond++;
                        if (SysPara.SystemRun)
                            SysPara.RunSecond++;
                        else
                            SysPara.StopSecond++;
                    }
                    SysPara.ScanTime = ScanTick;
                    ScanTick = 0;
                }
            }
        }

        private void Execute()
        {
            try { SDKKernal.RefreshIO(); }
            catch (Exception ex) { NPSDK.Alarm.Show("2018", ex.Message); }

            try { MiddleLayer.AlwaysRun(); }
            catch (Exception) { NPSDK.Alarm.Show("2019");}

            try { MiddleLayer.alTask.AlwaysRun(); }
            catch (Exception) { NPSDK.Alarm.Show("2019"); }

            try { MiddleLayer.CheckMotorProtected(); }
            catch (Exception) { NPSDK.Alarm.Show("2020"); }
        }

#region Initial
        public void InitialReset()
        {
            SysPara.SystemMode = RunMode.INITIAL;
            SysPara.UpConveyorInitialOk = false;
        }

#endregion
#region Run
        public void RunReset()
        {
            SysPara.SystemMode = RunMode.RUN;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               
        }
        #endregion
    }
}
