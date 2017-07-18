namespace WpfCrowdDetection.Model
{
    public class DeviceNotification
    {
        #region Properties

        public string deviceId { get; set; }

        public int persons { get; set; }

        #endregion Properties

        #region Constructors

        public DeviceNotification(string deviceId, int persons)
        {
            this.deviceId = deviceId;
            this.persons = persons;
        }

        public DeviceNotification()
        {
        }

        #endregion Constructors
    }
}