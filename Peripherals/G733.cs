using HidLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HIDReader
{
    public class G733 : WirelessDevice, IDisposable
    {
        const int VENDOR_LOGITECH = 0x046d;
        const int PRODUCT_ID = 0x0ab5;
        const int HIDPP_LONG_MESSAGE_LENGTH = 20;
        const byte HIDPP_LONG_MESSAGE = 0x11;
        const byte HIDPP_DEVICE_RECEIVER = 0xff;

        List<HidGeneric> hidGenerics = new List<HidGeneric>();
        byte[] payload = new byte[HIDPP_LONG_MESSAGE_LENGTH];
        CancellationTokenSource source = new CancellationTokenSource();

        public G733(int recurrenceInMili)
        {
            hidGenerics.Add(new HidGeneric(VENDOR_LOGITECH, PRODUCT_ID, 20, 20));

            payload[0] = HIDPP_LONG_MESSAGE;
            payload[1] = HIDPP_DEVICE_RECEIVER;
            payload[2] = 0x08;
            payload[3] = 0x0f;

            Task.Run(async () =>
            {
                while (!source.Token.IsCancellationRequested)
                {
                    try
                    {
                        GetHeadsetInfo();
                    }
                    catch(Exception ex)
                    {
                        ex = ex;
                    }

                    if (recurrenceInMili == -1)
                        break;

                    await Task.Delay(TimeSpan.FromMilliseconds(recurrenceInMili), source.Token);
                }
            }, source.Token);

        }

        private void GetHeadsetInfo()
        {
            hidGenerics.ForEach(f => f.GetInfo(payload, HeadsetOnReport));
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
                    var bat = (int)(0.1667 * voltage - 608.33);

                    BatteryLevel = bat;
                }

                NotifyBattery();
            }

            return false;
        }

        public void Dispose()
        {
            source.Cancel();

            hidGenerics.ForEach(f => f.Dispose());
        }
    }
}
