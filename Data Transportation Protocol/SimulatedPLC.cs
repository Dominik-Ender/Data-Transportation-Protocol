
namespace Data_Transportation_Protocol {
    public class SimulatedPLC : IDevice {

        private readonly IProtocol _communicationProtocol;
        private readonly Timer _timer;
        private double _lastTemperature = 20.0;

        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public DeviceType DeviceType => DeviceType.PLC;
        public string IPAddress { get; set; }
        public bool IsRunning { get; set; }

        public double SetPoint { get; set; }
        public bool AutoMode { get; set; }
        public string CurrentProgramm { get; set; } = "TemperatureControl";

        public event EventHandler<PLCStatusEventArgs> StatusChanged;

        public SimulatedPLC(string deviceId, IProtocol protocol) {
            DeviceId = deviceId;
            DeviceName = $"PLC CONTROLLER {deviceId}";
            _communicationProtocol = protocol;

            _timer = new Timer(ExecuteControlLogic, null, Timeout.Infinite, 1000);

            _communicationProtocol.MessageReceived += OnMessageReceived;
        }

        public async Task StartAsync() {
            IsRunning = true;
            AutoMode = true;
            _timer.Change(0, 1000);

            Console.WriteLine($"{DeviceName} started in {(AutoMode ? "AUTO" : "MANUAL")} mode");
        }

        public async Task StopAsync() {
            IsRunning = false;
            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            Console.WriteLine($"{DeviceName} stopped");
        }

        public async Task ProcessDataAsync(byte[] data) {
            var command = System.Text.Encoding.UTF8.GetString(data);

            if (command.StartsWith("SET_SETPOINT:")) {
                var value = command.Substring("SET_SETPOINT:".Length);

                if (double.TryParse(value, out double setpoint)) {
                    SetPoint = setpoint;

                    Console.WriteLine($"{DeviceName} Setpoint changed to {SetPoint}°C");
                }
            }
        }

        private async void OnMessageReceived(object sender, MessageReceivedEventArgs e) {

            Console.WriteLine($"{DeviceName}| e.Message.Source: {e.Message.Source}, e.Message.Destination: {e.Message.Destination}");

            // Console.WriteLine($"{DeviceName}| (e.Message is EthernetMessage ethernetMessage): {temp}");

            Console.WriteLine("=== MESSAGE RECEIVED DEBUG ===");
            Console.WriteLine($"{DeviceName}| Sender: {sender?.GetType().Name}");
            Console.WriteLine($"{DeviceName}| Message Type: {e.Message?.GetType().Name}");
            Console.WriteLine($"{DeviceName}| Message Source: '{e.Message?.Source}'");
            Console.WriteLine($"{DeviceName}| Message Destination: '{e.Message?.Destination}'");
            Console.WriteLine($"{DeviceName}| SenderId: '{e.SenderId}'");


            if (e.Message is EthernetMessage ethernetMessage && ethernetMessage.Source.Contains("SENSOR_01")) {
                Console.WriteLine($"{DeviceName}| ethernetMessage.TextData: {ethernetMessage.TextData}");
                if (ethernetMessage.TextData.Contains("TEMP:")) {
                    var parts = ethernetMessage.TextData.Split(';');

                    Console.WriteLine("|" + parts[0] + "|");
                    var tempString = parts[0].Substring(" TEMP:".Length);
                    // tempString = tempString.Replace(',', '.');



                    Console.WriteLine($"1: {tempString}");

                    Console.WriteLine($"{DeviceName}| _lastTemperature: {_lastTemperature}");

                    // if (double.TryParse(tempString, NumberStyles.Float, CultureInfo.InvariantCulture, out double temperature)) {
                    _lastTemperature = Convert.ToDouble(tempString); ;
                        Console.WriteLine($"{DeviceName}| _lastTemperature: {_lastTemperature}");

                    // }
                }
            }
        }

        private async void ExecuteControlLogic(object state) {
            if (!IsRunning || !AutoMode) {
                Console.WriteLine("33333333333333333333333333333333333333!");
                Console.WriteLine($"IsRunning: {IsRunning}");
                Console.WriteLine($"AutoMode: {AutoMode}");

                return;
            }

            Console.WriteLine("-----------------------------------");
            // TODO AutoMode auf True gesetzt
            // du willst das diese Funktion ausgelöst wird und dann der SimulatedActor eine Ausgabe hat/ Funktion ausführt
            // TODO letzte Fehlermeldung war hier
            // Console.WriteLine($"{DeviceName}| state: {state.ToString}");



            try {
                var deviation = SetPoint - _lastTemperature;

                if (deviation > 1.0) {
                    await SendCommandToActuator("HEATER_ON");
                } else if (deviation < -0.5) {
                    await SendCommandToActuator("HEATER_OFF");
                } else if (Math.Abs(deviation) <= 1.0) {
                    var power = Math.Max(0, Math.Min(100, 50 + (deviation * 20)));
                    await SendCommandToActuator($"SET_POWER:{power:F0}");
                }

                StatusChanged?.Invoke(this, new PLCStatusEventArgs(_lastTemperature, SetPoint, deviation));
            } catch (Exception exception) {
                Console.WriteLine($"{DeviceName} control error: {exception.Message}");
            }
        }

        private async Task SendCommandToActuator(string command) {
            var message = new EthernetMessage(DeviceId, "ACTUATOR_01", command);
            await _communicationProtocol.SendMessageAsync(message);
        }


    }
}
