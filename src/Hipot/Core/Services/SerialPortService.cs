
using System.IO.Ports;
using System.Collections.Generic;

namespace Hipot.Core.Services;

public class SerialPortService : IDisposable
{
    private readonly Dictionary<string, SerialPort> _srpPorts = new(); // Channel-specific ports
    private readonly Dictionary<string, SerialPort> _srcPorts = new(); // Common resource ports

    public void InitializePorts(List<ChannelConfig> channelConfigs, List<string> commonPortSettings)
    {
        // The settings strings are like "COM1,9600,N,8,1"
        foreach (var config in channelConfigs)
        {
            foreach (var portSetting in config.SerialPorts)
            {
                var settings = portSetting.Split(',');
                var portName = settings[0];
                var port = new SerialPort(portName,
                                          int.Parse(settings[1]),
                                          (Parity)Enum.Parse(typeof(Parity), settings[2]),
                                          int.Parse(settings[3]),
                                          (StopBits)Enum.Parse(typeof(StopBits), settings[4]));
                // The key could be a combination of channel and port name
                _srpPorts.Add($"{config.Name}_{portName}", port);
            }
        }

        foreach (var portSetting in commonPortSettings)
        {
             var settings = portSetting.Split(',');
             var portName = settings[0];
             var port = new SerialPort(portName,
                                       int.Parse(settings[1]),
                                       (Parity)Enum.Parse(typeof(Parity), settings[2]),
                                       int.Parse(settings[3]),
                                       (StopBits)Enum.Parse(typeof(StopBits), settings[4]));
            _srcPorts.Add(portName, port);
        }
    }

    public void WriteToSrp(string portKey, string data)
    {
        if (_srpPorts.TryGetValue(portKey, out var port) && port.IsOpen)
        {
            port.Write(data);
        }
    }

    public string ReadFromSrp(string portKey)
    {
        if (_srpPorts.TryGetValue(portKey, out var port) && port.IsOpen)
        {
            return port.ReadExisting();
        }
        return string.Empty;
    }

    public void WriteToSrc(string portName, string data)
    {
        if (_srcPorts.TryGetValue(portName, out var port) && port.IsOpen)
        {
            port.Write(data);
        }
    }

    public string ReadFromSrc(string portName)
    {
        if (_srcPorts.TryGetValue(portName, out var port) && port.IsOpen)
        {
            return port.ReadExisting();
        }
        return string.Empty;
    }


    public void Dispose()
    {
        foreach (var port in _srpPorts.Values)
        {
            if (port.IsOpen) port.Close();
            port.Dispose();
        }
        foreach (var port in _srcPorts.Values)
        {
            if (port.IsOpen) port.Close();
            port.Dispose();
        }
    }
}
