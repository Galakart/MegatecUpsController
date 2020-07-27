using System;
using System.Globalization;
using static MegatecUpsController.Structures;

namespace MegatecUpsController
{
    static class UpsData
    {
        // Megatec data
        public static float InputVoltage { get; private set; }
        public static float InputVoltageLastFault { get; private set; }
        public static float OutputVoltage { get; private set; }
        public static int LoadPercent { get; private set; }
        public static float Hz { get; private set; }
        public static float BatteryVoltage { get; private set; }
        public static float Temperature { get; private set; }
        public static string BinaryStatus { get; private set; }

        // Decoded binary status data
        public static bool IsUtilityFail { get; private set; }
        public static bool IsBatteryLow { get; private set; }
        public static bool IsActiveAVR { get; private set; }
        public static bool IsUpsFailed { get; private set; }
        public static bool IsStandby { get; private set; }
        public static bool IsTestInProgress { get; private set; }
        public static bool IsShutdownActive { get; private set; }
        public static bool IsBeeperOn { get; private set; }

        // Calculated data
        public static int BatteryPercent { get; private set; }
        public static float CurVA { get; private set; } // KUURRRWWA!!!
        public static float CurWatt { get; private set; }
        public static float CurAmper { get; private set; }

        //History data
        public static SizedQueue<double> InputVoltageHistory = new SizedQueue<double>(60);
        public static SizedQueue<double> OutputVoltageHistory = new SizedQueue<double>(60);

        //Settings data
        public static int ShutdownAction { private get; set; }
        public static float ShutdownVoltage { private get; set; }
        public static float BatteryVoltageMax { private get; set; }
        public static float BatteryVoltageMin { private get; set; }
        public static float BatteryVoltageMaxOnLoad { private get; set; }
        public static float UpsVA { private get; set; }

        // Common data
        public static string RawInputData { get; private set; }
        public static DateTime LastUpdated { get; private set; }
        public static string StatusLine { get; set; }

        static UpsData()
        {
            for (int i = 0; i < 60; i++)
            {
                InputVoltageHistory.Enqueue(0);
                OutputVoltageHistory.Enqueue(0);
            }

        }

        public static void UpdateData(string RawData)
        {
            RawInputData = RawData;

            RawData = RawData.Replace("(", "");
            string[] arrayOfData = RawData.Split();

            InputVoltage = float.Parse(arrayOfData[0], CultureInfo.InvariantCulture.NumberFormat);
            InputVoltageLastFault = float.Parse(arrayOfData[1], CultureInfo.InvariantCulture.NumberFormat);
            OutputVoltage = float.Parse(arrayOfData[2], CultureInfo.InvariantCulture.NumberFormat);
            LoadPercent = Convert.ToInt32(arrayOfData[3]);
            Hz = float.Parse(arrayOfData[4], CultureInfo.InvariantCulture.NumberFormat);
            BatteryVoltage = float.Parse(arrayOfData[5], CultureInfo.InvariantCulture.NumberFormat);
            Temperature = float.Parse(arrayOfData[6], CultureInfo.InvariantCulture.NumberFormat);
            BinaryStatus = arrayOfData[7];

            if (BinaryStatus.Length==8)
            {
                if (BinaryStatus[0].Equals('1')) IsUtilityFail = true; else IsUtilityFail = false;
                if (BinaryStatus[1].Equals('1')) IsBatteryLow = true; else IsBatteryLow = false;
                if (BinaryStatus[2].Equals('1')) IsActiveAVR = true; else IsActiveAVR = false;
                if (BinaryStatus[3].Equals('1')) IsUpsFailed = true; else IsUpsFailed = false;
                if (BinaryStatus[4].Equals('1')) IsStandby = true; else IsStandby = false;
                if (BinaryStatus[5].Equals('1')) IsTestInProgress = true; else IsTestInProgress = false;
                if (BinaryStatus[6].Equals('1')) IsShutdownActive = true; else IsShutdownActive = false;
                if (BinaryStatus[7].Equals('1')) IsBeeperOn = true; else IsBeeperOn = false;
            }            

            if (IsUtilityFail)
            {
                BatteryPercent = Convert.ToInt32(100 - (100 / (BatteryVoltageMaxOnLoad - BatteryVoltageMin) * (BatteryVoltageMaxOnLoad - BatteryVoltage)));
            }
            else
            {
                BatteryPercent = Convert.ToInt32(100 - (100 / (BatteryVoltageMax - BatteryVoltageMin) * (BatteryVoltageMax - BatteryVoltage)));
            }

            CurVA = LoadPercent * UpsVA / 100;
            CurWatt = Convert.ToSingle(CurVA * 0.6);
            if (OutputVoltage > 0)
            {
                CurAmper = (float)Math.Round(CurWatt / OutputVoltage, 1);
            }
            else
            {
                CurAmper = 0;
            }
            

            InputVoltageHistory.Enqueue(UpsData.InputVoltage);
            OutputVoltageHistory.Enqueue(UpsData.OutputVoltage);

            LastUpdated = DateTime.Now;

            if (IsUtilityFail)
            {
                CheckShutdownAction();
            }

        }

        private static void CheckShutdownAction()
        {
            if (BatteryVoltage <= ShutdownVoltage)
            {
                if (ShutdownAction == 0)
                {
                    PowerOps.ShutdownComputer();
                }
                else if (ShutdownAction == 1)
                {
                    PowerOps.HibernateComputer();
                }
            }
        }

    }
}
