using System;

namespace HIDReader
{
    public class WirelessDevice
    {
        public int BatteryLevel { set; get; }

        public event EventHandler<BatteryEventArgs> OnBatteryNotification;

        public void NotifyBattery()
        {
            OnBatteryNotification?.Invoke(this, new BatteryEventArgs()
            {
                IsSuccessful = true
            });
        }

    }

    public class BatteryEventArgs : EventArgs
    {
        public bool IsSuccessful { get; set; }
    }
}