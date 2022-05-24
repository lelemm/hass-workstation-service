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
    public class MouseBatterySensor : AbstractSensor
    {
        SteelseriesRival650_Wireless rival650_Wireless;
        int level;
        public MouseBatterySensor(MqttPublisher publisher, int? updateInterval = null, string name = "MouseBattery", Guid id = default(Guid)) : base(publisher, name ?? "MouseBattery", updateInterval ?? 10, id)
        {
            try
            {
                rival650_Wireless = new SteelseriesRival650_Wireless(2000);
                rival650_Wireless.OnBatteryNotification += Rival650_Wireless_OnBatteryNotification;
            }
            catch
            {
                rival650_Wireless = null;
            }
        }

        private void Rival650_Wireless_OnBatteryNotification(object sender, BatteryEventArgs e)
        {
            level = rival650_Wireless.BatteryLevel;
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
            if(rival650_Wireless == null)
                return "unsupported";

            return level.ToString(CultureInfo.InvariantCulture);
        }
    }
}
