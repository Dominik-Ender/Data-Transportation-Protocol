
namespace Data_Transportation_Protocol {
    public class SimulatedActuator {

        private readonly IProtocol _communicationProtocol;

        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public DeviceType DeviceType => DeviceType.Actuator;
        public string IPAddress { get; set; }
        public bool IsRunning { get; set; }

        public bool HeaterOn { get; set; }
        public double PowerLevel { get; set; }
        public double TargetTemperature { get; set; } = 22.0;

        public event EventHandler<ActuatorStateEventArgs> StateChanged;

        public SimulatedActuator(string deviceId, IProtocol protocol) {
            DeviceId = deviceId;
            DeviceName = $"HEATER ACTOR {deviceId}";
            _communicationProtocol = protocol;

            _communicationProtocol.MessageReceived += OnMessageReceived;
        }

        public async Task StartAsync() {
            IsRunning = true;
            Console.WriteLine($"{DeviceName} started");
        }

        public async Task StopAsync() {
            IsRunning = false;
            HeaterOn = false;
            PowerLevel = 0;
            Console.WriteLine($"{DeviceName} stopped");
        }

        public async Task ProcessDataAsync(byte[] data) {
            var command = System.Text.Encoding.UTF8.GetString(data);

            Console.WriteLine($"{DeviceName}| Command: {command}");
            await ProcessCommand(command);
        }

        private async void OnMessageReceived(object sender, MessageReceivedEventArgs e) {
            if (e.Message is EthernetMessage ethernetMessage && ethernetMessage.Destination == DeviceId) {
                await ProcessCommand(ethernetMessage.TextData);
            }
        }

        private async Task ProcessCommand(string command) {
            if (!IsRunning) {
                return;
            }

            Console.WriteLine($"SimulatedActuator| command: {command}");

            try {
                if (command.StartsWith("HEATER_ON")) {
                    HeaterOn = true;
                    PowerLevel = 75.0;
                    await SendResponse("HEATER_ON_OK");
                } else if (command.StartsWith("HEATER_OFF")) {
                    HeaterOn = false;
                    PowerLevel = 0.0;
                    await SendResponse("HEATER_OFF_OK");
                } else if (command.StartsWith("SET_POWER:")) {
                    var powerString = command.Substring("SET_POWER:".Length);

                    if (double.TryParse(powerString, out double power)) {
                        PowerLevel = Math.Max(0, Math.Min(100, power));
                        HeaterOn = PowerLevel > 0;
                        await SendResponse($"POWER_SET:{PowerLevel:F1}%");
                    }
                } else if (command.StartsWith("SET_TARGET:")) {
                    var tempString = command.Substring("SET_TARGET:".Length);

                    if (double.TryParse(tempString, out double temp)) {
                        TargetTemperature = temp;
                        await SendResponse($"TARGET_SET:{TargetTemperature:F1}°C");
                    }
                }

                StateChanged?.Invoke(this, new ActuatorStateEventArgs(HeaterOn, PowerLevel, TargetTemperature));

                Console.WriteLine($"{DeviceName}: Heater={HeaterOn}, Power={PowerLevel:F1}%");
            } catch (Exception exception) {
                await SendResponse($"ERROR:{exception.Message}");
            }
        }

        private async Task SendResponse(string response) {
            var message = new EthernetMessage(DeviceId, "PLC_01", response);
            await _communicationProtocol.SendMessageAsync(message);
        }
    }
}
