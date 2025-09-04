using System;

namespace Data_Transportation_Protocol {
    public class SensorDataEventArgs : EventArgs {

        public double Value { get; set; }
        public DateTime TimeStamp { get; set; }

        public SensorDataEventArgs(double value, DateTime timestamp) {
            Value = value;
            TimeStamp = timestamp;
        }
    }
}
