using System;

namespace Data_Transportation_Protocol {
    public class PLCStatusEventArgs : EventArgs {

        public double ActualValue { get; set; }
        public double SetPoint { get; set; }
        public double Deviation { get; set; }

        public PLCStatusEventArgs(double actualValue, double setPoint, double deviation) {
            ActualValue = actualValue;
            SetPoint = setPoint;
            Deviation = deviation;
        }
    }
}
