using System;
using System.Threading;
using System.Threading.Tasks;

namespace Data_Transportation_Protocol {
    public class SimulatedSensor : IDevice {

        private readonly Timer _timer;
        private readonly Random _random;
        private readonly IProtocol _communicationProtocol;

        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public DeviceType DeviceType => DeviceType.Sensor;
        public string IPAddress { get; set; }
        public bool IsRunning { get; set; }

        public double CurrentTemperature { get; set; }
        public double MinTemperature { get; set; } = 18.0;
        public double MaxTemperature { get; set; } = 35.0;
        public int UpdateIntervalMilliseconds { get; set; } = 2000;

        public event EventHandler<SensorDataEventArgs> DataGenerated;

        public SimulatedSensor(string deviceId, IProtocol protocol) {
            DeviceId = deviceId;
            DeviceName = $"TEMPERATURE SENSOR {deviceId}";
            _communicationProtocol = protocol;
            _random = new Random();

            _timer = new Timer(GenerateSensorData, null, Timeout.Infinite, UpdateIntervalMilliseconds);
            CurrentTemperature = 20.0;
        }

        public async Task StartAsync() {
            IsRunning = true;
            _timer.Change(0, UpdateIntervalMilliseconds);
            Console.WriteLine($"{DeviceName} started");
        }

        public async Task StopAsync() {
            IsRunning = false;
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            Console.WriteLine($"{DeviceName} stopped");
        }

        public async Task ProcessDataAsync(byte[] data) {
            var command = System.Text.Encoding.UTF8.GetString(data);

            if (command.StartsWith("CALIBRATE")) {
                Console.WriteLine($"{DeviceName} received calibration command");
                // calibration here
            }
        }

        private async void GenerateSensorData(object data) {
            if (!IsRunning) {
                return;
            }

            var change = (_random.NextDouble() - 0.5) * 0.5;
            CurrentTemperature += change;

            CurrentTemperature = Math.Max(MinTemperature, Math.Min(MaxTemperature, CurrentTemperature));

            Console.WriteLine($"{DeviceName}| Temperature: {CurrentTemperature}°");
            var sensorData = $"TEMP:{CurrentTemperature:F2}; UNIT:°C; TIME:{DateTime.Now:HH:mm:ss}";
            var message = new EthernetMessage(DeviceId, "PLC_01", sensorData);

            await _communicationProtocol.SendMessageAsync(message);

            DataGenerated?.Invoke(this, new SensorDataEventArgs(CurrentTemperature, DateTime.Now));
        }
    }
}
