using System;
using System.Threading;
using System.Threading.Tasks;

namespace HIDReader
{
    public class SteelseriesRival650_Wireless : WirelessDevice, IDisposable
    {
        const int VENDOR_STEELSERIES = 0x1038;
        const int PRODUCT_ID = 0x1726;
        const int PRODUCT_ID_CABLE = 0x172b;

        HidGeneric hidGeneric;
        byte[] payload = new byte[64];
        CancellationTokenSource source = new CancellationTokenSource();

        public SteelseriesRival650_Wireless(int recurrenceInMili)
        {
            hidGeneric = new HidGeneric(VENDOR_STEELSERIES, PRODUCT_ID, 2);

            payload[1] = 0xaa;
            payload[2] = 0x01;

            Task.Run(async () =>
            {
                while (!source.Token.IsCancellationRequested)
                {
                    try
                    {
                        var onCable = new HidGeneric(VENDOR_STEELSERIES, PRODUCT_ID_CABLE, 1);
                        if (onCable != null)
                        {
                            BatteryLevel = 9999;
                            NotifyBattery();
                            onCable.Dispose();
                        }
                    }
                    catch
                    {
                        GetMouseInfo();
                    }

                    if (recurrenceInMili == -1)
                    {
                        source.Cancel();
                        return;
                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(recurrenceInMili), source.Token);
                }
            }, source.Token);
        }

        public CancellationToken Token
        {
            get
            {
                return source.Token;
            }
        }

        public void Dispose()
        {
            source.Cancel();

            if (hidGeneric != null)
            {
                hidGeneric.Dispose();
            }
        }

        private void GetMouseInfo()
        {
            hidGeneric.GetInfo(payload, (report) =>
            {
                if (report != null)
                {
                    BatteryLevel = report.Data[0];
                    NotifyBattery();
                }

                return false;
            });
        }
    }
}
