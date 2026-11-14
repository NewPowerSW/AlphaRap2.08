using System.Collections.Generic;

namespace AlphaRap
{
    internal static class StationData
    {
        public static bool UpConveyorInitialOk;
        public static bool DownConveyorInitialOk;
        public static bool GantryInitialOK;
    }

    public struct FastenData
    {
        public string Point;
        public double Torque;
        public double Angle;
        public ResultCode Result;
    }
    public class BoardInfo
    {
        public bool bBoardInNG;
        public string SerialNumber;
        public List<FastenData> Data = new List<FastenData>();
        public bool bResultNG;
    }

}
