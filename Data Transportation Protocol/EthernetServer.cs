using System.Net;
using System.Net.Sockets;

namespace Data_Transportation_Protocol {
    public class EthernetServer : IProtocol {

        private TcpListener _tcpListener;
        private readonly List<ClientHandler> _connectedClients;
        private bool _isRunning;
        private int _messageCounter;

        public string Name => "ETHERNET SERVER";
        public int Port { get; set; }
        public bool IsConnected => _isRunning;
        public string Status { get; set; }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event EventHandler<string> StatusChanged;

        public EthernetServer(int port = 502) {
            Port = port;
            _connectedClients = new List<ClientHandler>();
            Status = "Disconnected";

            Console.WriteLine("Server running...");
        }

        public async Task<bool> ConnectAsync() {
            try {
                _tcpListener = new TcpListener(IPAddress.Any, Port);
                _tcpListener.Start();
                _isRunning = true;

                UpdateStatus("Server started, waiting for clients...");

                _ = Task.Run(AcceptClientsAsync);

                return true;
            } catch (Exception exception) {
                UpdateStatus($"Error starting server: {exception.Message}");
                return false;
            }
        }

        public async Task DisconnectAsync() {
            _isRunning = false;

            foreach (var clients in _connectedClients.ToArray()) {
                clients.Disconnect();
            }

            _connectedClients.Clear();

            _tcpListener.Stop();
            UpdateStatus("Server stopped");
        }

        public async Task<bool> SendMessageAsync(MessageBase message) {
            if (!(message is EthernetMessage ethMessage))
                return false;

            ethMessage.MessageId = ++_messageCounter;
            var data = ethMessage.Serialize();

            // An alle verbundenen Clients senden
            foreach (var client in _connectedClients.ToArray()) {
                await client.SendDataAsync(data);
            }

            return true;
        }

        private async Task AcceptClientsAsync() {
            while (_isRunning) {
                try {
                    var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                    var clientHandler = new ClientHandler(tcpClient, this);
                    _connectedClients.Add(clientHandler);

                    UpdateStatus($"Client connected. Total clients: {_connectedClients.Count}");
                    Console.WriteLine($"ETHERNET SERVER| Total clients: {_connectedClients.Count}");

                    _ = Task.Run(() => clientHandler.HandleClientAsync());
                } catch (ObjectDisposedException) {
                    break;
                } catch (Exception ex) {
                    UpdateStatus($"Error accepting client: {ex.Message}");
                }
            }
        }

        internal void OnMessageReceived(EthernetMessage message, string clientId) {

            // Console.WriteLine($"{Name}| messageType: {message.MessageType}, clientId: {clientId}, messageData: {message.TextData}, messageDestination: {message.Destination}");
            Console.WriteLine($"{Name}| messageData: {message.TextData}, messageDestination: {message.Destination}");

            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message, clientId));
        }

        internal void OnClientDisconnected(ClientHandler client) {
            _connectedClients.Remove(client);
            UpdateStatus($"Client disconnected. Total clients: {_connectedClients.Count}");
        }

        private void UpdateStatus(string status) {
            Status = status;
            StatusChanged?.Invoke(this, status);
        }
    }
}
