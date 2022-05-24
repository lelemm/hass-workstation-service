using HidLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HIDReader
{
    public class G533 : WirelessDevice, IDisposable
    {
        const int VENDOR_LOGITECH = 0x046d;
        const int PRODUCT_ID = 0x0a66;
        const int HIDPP_LONG_MESSAGE_LENGTH = 20;
        const byte HIDPP_LONG_MESSAGE = 0x11;
        const byte HIDPP_DEVICE_RECEIVER = 0xff;

        List<HidGeneric> hidGenerics = new List<HidGeneric>();
        byte[] payload = new byte[HIDPP_LONG_MESSAGE_LENGTH];
        CancellationTokenSource source = new CancellationTokenSource();

        public G533(int recurrenceInMili)
        {
            hidGenerics.Add(new HidGeneric(VENDOR_LOGITECH, PRODUCT_ID, 0));
            hidGenerics.Add(new HidGeneric(VENDOR_LOGITECH, PRODUCT_ID, 1));
            hidGenerics.Add(new HidGeneric(VENDOR_LOGITECH, PRODUCT_ID, 2));
            hidGenerics.Add(new HidGeneric(VENDOR_LOGITECH, PRODUCT_ID, 3));

            payload[0] = HIDPP_LONG_MESSAGE;
            payload[1] = HIDPP_DEVICE_RECEIVER;
            payload[2] = 0x07;
            payload[3] = 0x01;

            Task.Run(async () =>
            {
                while (!source.Token.IsCancellationRequested)
                {
                    GetHeadsetInfo();

                    if (recurrenceInMili == -1)
                        break;

                    await Task.Delay(TimeSpan.FromMilliseconds(recurrenceInMili), source.Token);
                }
            }, source.Token);

        }

        private void GetHeadsetInfo()
        {
            hidGenerics.ForEach(f => f.GetInfo(payload, HeadsetOnReport));
            //hidGeneric.GetInfo(payload, HeadsetOnReport);
        }

        private bool HeadsetOnReport(HidReport report)
        {
            if (report != null && report.Exists && report.ReportId == 17)
            {
                if (report.Data[5] == 3)
                {
                    BatteryLevel = 9999;
                }
                else
                {
                    int voltage = (report.Data[3] << 8) | report.Data[4];
                    var bat = estimate_battery_level((double)voltage);

                    BatteryLevel = bat;
                }

                NotifyBattery();
            }

            return false;
        }

        int estimate_battery_level(double voltage)
        {
            if (voltage <= 3618)
                return (int)((0.017547 * voltage) - 53.258578);
            if (voltage > 4011)
                return 100;

            return map((int)(Math.Round(-0.0000010876 * Math.Pow(voltage, 3) + 0.0122392434 * Math.Pow(voltage, 2) - 45.6420832787 * voltage + 56445.8517589238)),
                25, 100, 20, 100);
        }

        int map(int x, int in_min, int in_max, int out_min, int out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }

        public void Dispose()
        {
            source.Cancel();

            hidGenerics.ForEach(f => f.Dispose());

            //if (hidGeneric != null)
            //{
            //    hidGeneric.Dispose();
            //}
        }
    }
}
