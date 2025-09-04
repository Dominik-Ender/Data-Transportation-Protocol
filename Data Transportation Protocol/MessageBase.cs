using System;

namespace Data_Transportation_Protocol {
    public abstract class MessageBase {

        public int MessageId { get; set; }
        public DateTime TimeStamp { get; set; }
        public byte[] Data { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public int DataLength => Data?.Length ?? 0;

        public MessageBase() {
            TimeStamp = DateTime.Now;
            Data = new byte[0];
        }

        public MessageBase(string source, string destination, byte[] data) : this() {
            Source = source;
            Destination = destination;
            Data = data ?? new byte[0];
        }

        public abstract byte[] Serialize();
        public abstract void Deserialize(byte[] data);
        public abstract bool Validate();

        public override string ToString() {
            return $"Message {MessageId}: {Source} -> {Destination} ({DataLength} bytes)";
        }
    }
}
