using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Transportation_Protocol {
    public interface IDevice {

        string DeviceId { get; }
        string DeviceName { get; set; }
        DeviceType DeviceType { get; }
        string IPAddress { get; set; }
        bool IsRunning { get; }

        Task StartAsync();
        Task StopAsync();
        Task ProcessDataAsync(byte[] data);
    }
}
