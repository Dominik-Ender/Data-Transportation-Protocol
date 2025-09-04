// See https://aka.ms/new-console-template for more information
using Data_Transportation_Protocol;

Console.WriteLine("Hello, World!");

var server = new EthernetServer(502);
await server.ConnectAsync();

var client = new EthernetClient("127.0.0.1", 502);
await client.ConnectAsync();

var sensor = new SimulatedSensor("SENSOR_01", client);
var actuator = new SimulatedActuator("ACTUATOR_01", client);
var plc = new SimulatedPLC("PLC_01", server);

await sensor.StartAsync();
await actuator.StartAsync();
await plc.StartAsync();

while (true) {
    await Task.Delay(1000);
}


await Task.WhenAll(
    sensor.StartAsync(),
    actuator.StartAsync(),
    plc.StartAsync()
);

