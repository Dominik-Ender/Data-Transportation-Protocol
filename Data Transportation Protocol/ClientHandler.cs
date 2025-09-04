using System.Net.Sockets;
using System.Xml.Linq;

namespace Data_Transportation_Protocol {
    internal class ClientHandler {

        private readonly TcpClient _tcpClient;
        private readonly NetworkStream _networkStream;
        private readonly EthernetServer _server;
        private readonly string _clientId;

        public ClientHandler(TcpClient tcpClient, EthernetServer server) {
            _tcpClient = tcpClient;
            _networkStream = tcpClient.GetStream();
            _server = server;
            _clientId = tcpClient.Client.RemoteEndPoint?.ToString() ?? "Unknown";
        }

        public async Task HandleClientAsync() {
            var buffer = new byte[1024];

            Console.WriteLine($"CLIENTHANDLER|");

            try {
                while (_tcpClient.Connected) {
                    var bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    var receivedData = new byte[bytesRead];
                    Array.Copy(buffer, receivedData, bytesRead);

                    var message = new EthernetMessage();
                    message.Deserialize(receivedData);

                    _server.OnMessageReceived(message, _clientId);

                    SendDataAsync(receivedData);
                    // SendDataAsync(buffer);



                    // Console.WriteLine("ClientHandler: HandleClientAsycn()");
                }
            } catch (Exception ex) {
                // Client getrennt oder Fehler
            } finally {
                Disconnect();
            }
        }
        
        public async Task SendDataAsync(byte[] data) {

            // Console.WriteLine($"CLIENTHANDLER| _tcpClient.Connected: {_tcpClient.Connected}, data: {data}");
            if (_tcpClient.Connected) {
                await _networkStream.WriteAsync(data, 0, data.Length);
            }
        }

        public void Disconnect() {
            _networkStream?.Close();
            _tcpClient?.Close();
            _server.OnClientDisconnected(this);
        }
    }
}