using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Transportation_Protocol {
    public interface IProtocol {

        string Name { get; }
        int Port { get; set; }
        bool IsConnected { get; }
        string Status { get; }

        Task<bool> ConnectAsync();
        Task DisconnectAsync();
        Task<bool> SendMessageAsync(MessageBase message);
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
        event EventHandler<string> StatusChanged;
    }
}
