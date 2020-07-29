using System;
using System.Text;
using System.Threading;
using UsbLibrary;

namespace MegatecUpsController
{
    static class UsbOps
    {

        public static UsbHidPort usb = new UsbHidPort();
        private static Timer timerUSB;
        private static string rawDataDecoded = "";

        private static readonly byte[] Q1request = ASCIIEncoding.ASCII.GetBytes("0Q1\r00000");
        private static readonly byte[] Qrequest = ASCIIEncoding.ASCII.GetBytes("0Q\r000000");

        static UsbOps()
        {
            usb.OnSpecifiedDeviceArrived += new System.EventHandler(Usb_OnSpecifiedDeviceArrived);
            usb.OnSpecifiedDeviceRemoved += new System.EventHandler(Usb_OnSpecifiedDeviceRemoved);
            usb.OnDataSend += new System.EventHandler(Usb_OnDataSend);
            usb.OnDataRecieved += new UsbLibrary.DataRecievedEventHandler(Usb_OnDataRecieved);

            TimerCallback tm = new TimerCallback(TimerActionSendQ1);
            timerUSB = new Timer(tm, null, 0, 1000);
        }

        public static bool SetupUsbDevice(Int32 vid, Int32 pid)
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

        private static void TimerActionSendQ1(object obj)
        {
            if (usb.SpecifiedDevice != null)
            {
                usb.SpecifiedDevice.SendData(Q1request);
            }
        }

        public static void sendSwitchBeeper()
        {
            usb.SpecifiedDevice.SendData(Qrequest);
        }


        private static void Usb_OnSpecifiedDeviceArrived(object sender, EventArgs e)
        {
            //device found
            UpsData.ConnectStatus = true;
        }

        private static void Usb_OnSpecifiedDeviceRemoved(object sender, EventArgs e)
        {
            //device lost
            UpsData.ConnectStatus = false;
        }

        private static void Usb_OnDataSend(object sender, EventArgs e)
        {
            //data was sent
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
