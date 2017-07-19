using Newtonsoft.Json;

namespace WpfCrowdDetection.Model
{
    public class DeviceNotification
    {
        #region Properties

        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        [JsonProperty("persons")]
        public int Persons { get; set; }

        #endregion Properties

        #region Constructors

        public DeviceNotification(string deviceId, int persons)
        {
            DeviceId = deviceId;
            Persons = persons;
        }

        public DeviceNotification()
        {
        }

        #endregion Constructors
    }
}