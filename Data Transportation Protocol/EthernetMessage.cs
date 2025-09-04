using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Data_Transportation_Protocol {
    public class EthernetMessage : MessageBase {

        public int SequenceNumber { get; set; }
        public EthernetMessageType MessageType { get; set; }
        public string TextData { get; set; }

        public EthernetMessage() : base() {
            MessageType = EthernetMessageType.Data;
        }

        public EthernetMessage(string source, string destination, string textData) : base(source, destination, Encoding.UTF8.GetBytes(textData)) {
            TextData = textData;
            MessageType = EthernetMessageType.Data;
        }

        public override byte[] Serialize() {
            var message = $"{MessageId} | {SequenceNumber} | {MessageType} | {Source} | {Destination} | {TextData}";
            return Encoding.UTF8.GetBytes(message);
        }

        public override void Deserialize(byte[] data) {
            var message = Encoding.UTF8.GetString(data);
            var parts = message.Split('|');

            if (parts.Length >= 6) {
                MessageId = int.Parse(parts[0]);
                SequenceNumber = int.Parse(parts[1]);
                MessageType = (EthernetMessageType)Enum.Parse(typeof(EthernetMessageType), parts[2]);
                Source = parts[3];
                Destination = parts[4];
                TextData = parts[5];
                Data = Encoding.UTF8.GetBytes(TextData);
            }
        }

        public override bool Validate() {
            return !string.IsNullOrEmpty(Source)
                    && !string.IsNullOrEmpty(Destination)
                    && !string.IsNullOrEmpty(TextData);
        }
    }

    public enum EthernetMessageType2 {
        Data,
        Command,
        Response,
        Heartbeat,
        Error
    }
}
