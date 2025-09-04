using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Data_Transportation_Protocol {
    public class EthernetClient : IProtocol {
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private bool _isConnected;
        private int _messageCounter;
        private int _sequenceNumber;

        public string Name => "ETHERNET CLIENT";
        public int Port { get; set; }
        public bool IsConnected => _isConnected;
        public string Status { get; private set; }
        public string ServerIP { get; set; }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event EventHandler<string> StatusChanged;

        public EthernetClient(string serverIP = "127.0.0.1", int port = 502) {
            ServerIP = serverIP;
            Port = port;
            Status = "Disconnected";
        }

        public async Task<bool> ConnectAsync() {
            try {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(ServerIP, Port);
                _networkStream = _tcpClient.GetStream();
                _isConnected = true;

                UpdateStatus($"Connected to {ServerIP}:{Port}");

                // Starte Empfangs-Loop
                _ = Task.Run(ReceiveMessagesAsync);

                return true;
            } catch (Exception ex) {
                UpdateStatus($"Connection failed: {ex.Message}");
                return false;
            }
        }

        public async Task DisconnectAsync() {
            _isConnected = false;
            _networkStream?.Close();
            _tcpClient?.Close();
            UpdateStatus("Disconnected");
        }

        public async Task<bool> SendMessageAsync(MessageBase message) {

            Console.WriteLine($"{Name}| ");

            if (!_isConnected || !(message is EthernetMessage ethMessage)) {
                return false;
            }

            ethMessage.MessageId = ++_messageCounter;
            ethMessage.SequenceNumber = ++_sequenceNumber;

            try {
                var data = ethMessage.Serialize();
                await _networkStream.WriteAsync(data, 0, data.Length);
                return true;
            } catch (Exception ex) {
                UpdateStatus($"Send failed: {ex.Message}");
                return false;
            }
        }

        private async Task ReceiveMessagesAsync() {
            var buffer = new byte[1024];

            try {
                while (_isConnected && _tcpClient.Connected) {
                    var bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length);
                    
                    if (bytesRead == 0) {
                        break;
                    }

                    var receivedData = new byte[bytesRead];
                    Array.Copy(buffer, receivedData, bytesRead);

                    var message = new EthernetMessage();
                    message.Deserialize(receivedData);

                    Console.WriteLine($"{Name}: ServerIP {ServerIP}, MessageType: {message.MessageType}, MessageTextData: {message.TextData}, messageDestination: {message.Destination}");


                    MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message, ServerIP));
                }
            } catch (Exception ex) {
                UpdateStatus($"Receive error: {ex.Message}");
            }
        }

        private void UpdateStatus(string status) {
            Status = status;
            StatusChanged?.Invoke(this, status);
        }
    }
}