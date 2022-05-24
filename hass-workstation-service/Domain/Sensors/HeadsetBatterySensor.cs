using hass_workstation_service.Communication;
using HIDReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hass_workstation_service.Domain.Sensors
{
    public class HeadsetBatterySensor : AbstractSensor
    {
        G733 G733;
        int level;

        public HeadsetBatterySensor(MqttPublisher publisher, int? updateInterval = null, string name = "HeadsetBattery", Guid id = default(Guid)) : base(publisher, name ?? "HeadsetBattery", updateInterval ?? 10, id)
        {
            try
            {
                G733 = new G733(2000);
                G733.OnBatteryNotification += G733_OnBatteryNotification;
            }
            catch
            {
                G733 = null;
            }
        }

        private void G733_OnBatteryNotification(object sender, BatteryEventArgs e)
        {
            level = G733.BatteryLevel;
        }

        public override DiscoveryConfigModel GetAutoDiscoveryConfig()
        {
            return this._autoDiscoveryConfigModel ?? SetAutoDiscoveryConfigModel(new SensorDiscoveryConfigModel()
            {
                Name = this.Name,
                NamePrefix = Publisher.NamePrefix,
                Unique_id = this.Id.ToString(),
                Device = this.Publisher.DeviceConfigModel,
                State_topic = $"homeassistant/{this.Domain}/{Publisher.DeviceConfigModel.Name}/{DiscoveryConfigModel.GetNameWithPrefix(Publisher.NamePrefix, this.ObjectId)}/state",
                Device_class = "battery",
                Unit_of_measurement = "%",
                Availability_topic = $"homeassistant/{this.Domain}/{Publisher.DeviceConfigModel.Name}/availability"
            });
        }

        public override string GetState()
        {
            if (G733 == null)
                return "unsupported";

            return level.ToString(CultureInfo.InvariantCulture);
        }
    }
}
