using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Transportation_Protocol {
    public class MessageReceivedEventArgs : EventArgs {

        public MessageBase Message { get; }
        public string SenderId { get; }

        public MessageReceivedEventArgs(MessageBase message, string senderId) {
            Message = message;
            SenderId = senderId;
        }
    }
}
