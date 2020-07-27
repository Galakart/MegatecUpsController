﻿using System;
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
        }

        public static void SetupUsbDevice(Int32 vid, Int32 pid)
        {
            StopUsbTimer();

            usb.VendorId = vid;
            usb.ProductId = pid;

            usb.Open(true);

            TimerCallback tm = new TimerCallback(TimerActionSendQ1);
            timerUSB = new Timer(tm, null, 0, 1000);

            if (usb.SpecifiedDevice != null)
            {
                UpsData.StatusLine = "ИБП подключён";
            }
            else
            {
                UpsData.StatusLine = "ИБП недоступен";
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
            else
            {
                UpsData.StatusLine = "потеряно соединение";
            }
        }

        public static void sendSwitchBeeper()
        {
            usb.SpecifiedDevice.SendData(Qrequest);
        }


        private static void Usb_OnSpecifiedDeviceArrived(object sender, EventArgs e)
        {
            //device found
            UpsData.StatusLine = "ИБП подключён";
        }

        private static void Usb_OnSpecifiedDeviceRemoved(object sender, EventArgs e)
        {
            //device lost
            UpsData.StatusLine = "ИБП недоступен";
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
