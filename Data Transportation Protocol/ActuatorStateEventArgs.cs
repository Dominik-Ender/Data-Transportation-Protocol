using System;

namespace Data_Transportation_Protocol {
    public class ActuatorStateEventArgs : EventArgs {

        public bool IsActive { get; set; }
        public double PowerLevel { get; set; }
        public double TargetValue { get; set; }

        public ActuatorStateEventArgs(bool isActive, double powerLevel, double targetValue) {
            IsActive = isActive;
            PowerLevel = powerLevel;
            TargetValue = targetValue;
        }
    }
}
