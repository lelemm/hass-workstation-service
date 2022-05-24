using HidLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIDReader
{
    public class HidGeneric : IDisposable
    {
        HidDevice _device;

        public HidGeneric(int vendor, int product, int inArray)
        {
            var devices = HidDevices.Enumerate(vendor, product);

            if (devices.Count() > inArray)
            {
                _device = devices.ToArray()[inArray];

                _device.OpenDevice();
                _device.MonitorDeviceEvents = true;

            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }

        public HidGeneric(int vendor, int product, int capability1, int capability2)
        {
            var HidDeviceList = HidDevices.Enumerate(vendor, product).ToArray();

            for (int i = 0; i < HidDeviceList.Length; i++)
            {
                Console.WriteLine(HidDeviceList[i]);
                HidDeviceList[i].OpenDevice();
                Console.WriteLine("Connected: " + HidDeviceList[i].IsConnected.ToString());
                Console.WriteLine("InputReportByteLength: " + HidDeviceList[i].Capabilities.InputReportByteLength);
                Console.WriteLine("OutputReportByteLength: " + HidDeviceList[i].Capabilities.OutputReportByteLength);
                Console.WriteLine("FeatureReportByteLength: " + HidDeviceList[i].Capabilities.FeatureReportByteLength);
                Console.WriteLine("DevicePath: " + HidDeviceList[i].DevicePath);
                Console.WriteLine("Description: " + HidDeviceList[i].Description);
                Console.WriteLine("ReadHandle: " + HidDeviceList[i].ReadHandle);
                Console.WriteLine("");
                if (HidDeviceList[i].Capabilities.OutputReportByteLength == capability1 && HidDeviceList[i].Capabilities.InputReportByteLength == capability2)
                {
                    _device = HidDeviceList[i];
                    break;
                }

                HidDeviceList[i].CloseDevice();
            }

            _device.MonitorDeviceEvents = true;
        }

        Func<HidReport, bool> _func;
        public void GetInfo(byte[] payload, Func<HidReport, bool> func)
        {
            _func = func;
            var report = new HidReport(payload.Length, new HidDeviceData(payload, HidDeviceData.ReadStatus.Success));

            _device.WriteReport(report, (success) =>
            {
                if (success)
                {
                    _device.ReadReport(OnResult);
                }
                else
                {
                    func(null);
                }
            });
        }

        private void OnResult(HidReport report)
        {
            if (_func(report))
                _device.ReadReport(OnResult);
        }

        public void Dispose()
        {
            if (_device != null)
            {
                _device.CloseDevice();
            }
        }
    }
}
