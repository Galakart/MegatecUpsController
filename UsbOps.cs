using log4net;
using System;
using System.Reflection;
using System.Text;
using System.Threading;
using UsbLibrary;

namespace MegatecUpsController
{
    static class UsbOps
    {

        private static readonly ILog applog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ILog eventlog = LogManager.GetLogger("eventlog");

        public static UsbHidPort usb = new UsbHidPort();
        private static readonly Timer timerUSB;
        private static string rawDataDecoded = "";

        private static readonly byte[] comm_status = Encoding.ASCII.GetBytes("0Q1\r00000");
        private static readonly byte[] comm_beeper = Encoding.ASCII.GetBytes("0Q\r000000");

        static UsbOps()
        {
            usb.OnSpecifiedDeviceArrived += new EventHandler(Usb_OnSpecifiedDeviceArrived);
            usb.OnSpecifiedDeviceRemoved += new EventHandler(Usb_OnSpecifiedDeviceRemoved);
            usb.OnDataSend += new EventHandler(Usb_OnDataSend);
            usb.OnDataRecieved += new DataRecievedEventHandler(Usb_OnDataRecieved);

            TimerCallback tm = new TimerCallback(TimerActionDataRequest);
            timerUSB = new Timer(tm, null, 0, 1000);
        }

        public static bool SetupUsbDevice(int vid, int pid)
        {
            if (usb.Ready())
            {
                usb.Close();
            }

            usb.VendorId = vid;
            usb.ProductId = pid;

            usb.Open(true);

            if (usb.SpecifiedDevice != null)
            {
                UpsData.ConnectStatus = true;
                return true;
            }
            else
            {
                UpsData.ConnectStatus = false;
                return false;
            }
        }

        public static void StopUsbTimer()
        {
            if (timerUSB != null)
            {
                timerUSB.Dispose();
            }                
        }

        private static void TimerActionDataRequest(object obj)
        {
            if (usb.SpecifiedDevice != null)
            {
                usb.SpecifiedDevice.SendData(comm_status);
            }
        }

        public static void SwitchUpsBeeper()
        {
            if (usb.SpecifiedDevice != null)
            {
                usb.SpecifiedDevice.SendData(comm_beeper);
            }
        }

        private static void Usb_OnSpecifiedDeviceArrived(object sender, EventArgs e)
        {
            eventlog.Info("ИБП подключён");
            UpsData.ConnectStatus = true;
        }

        private static void Usb_OnSpecifiedDeviceRemoved(object sender, EventArgs e)
        {
            eventlog.Info("Потеряно соединение с ИБП");
            UpsData.ConnectStatus = false;
        }

        private static void Usb_OnDataSend(object sender, EventArgs e)
        {
            //data sent
        }

        private static void Usb_OnDataRecieved(object sender, DataRecievedEventArgs args)
        {
            foreach (byte myData in args.data)
            {
                if (myData != 0x00)
                {
                    char c = Convert.ToChar(myData);
                    rawDataDecoded += c.ToString();
                }

                if (myData == 0x0D)
                {
                    UpsData.UpdateData(rawDataDecoded);
                    rawDataDecoded = "";
                }
            }
        }

    }
}
