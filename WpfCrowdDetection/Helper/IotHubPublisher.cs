using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text;

namespace WpfCrowdDetection.Helper
{
    public class IotHubPublisher
    {
        private readonly DeviceClient _deviceClient;

        public IotHubPublisher(string iotHubHostName, string deviceId, string sharedAccessKey)
        {
            _deviceClient = DeviceClient.Create(iotHubHostName, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, sharedAccessKey), TransportType.Http1);
        }

        public async void SendDataAsync<T>(T data)
        {
            var messageString = JsonConvert.SerializeObject(data);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));
            await _deviceClient.SendEventAsync(message);
        }
    }
}