using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Threading.Tasks;

namespace Hipot.Core.Services
{
    public class SerialPortService : IDisposable
    {
        private static readonly ConcurrentDictionary<string, SerialPort> _serialPorts = [];

        public ConcurrentDictionary<string, SerialPort> GetSerialPorts()
        {
            return _serialPorts;
        }

        public void AddPort(string portKey, string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            var serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            _serialPorts.TryAdd(portKey, serialPort);
        }

        public void OpenPort(string portKey)
        {
            if (_serialPorts.TryGetValue(portKey, out var serialPort) && !serialPort.IsOpen)
            {
                serialPort.Open();
            }
        }

        public void ClosePort(string portKey)
        {
            if (_serialPorts.TryGetValue(portKey, out var serialPort) && serialPort.IsOpen)
            {
                serialPort.Close();
            }
        }

        public void CloseAllPorts()
        {
            foreach (var port in _serialPorts.Values)
            {
                if (port.IsOpen)
                {
                    port.Close();
                }
            }
        }

        public void WriteToSrp(string portKey, string data)
        {
            if (_serialPorts.TryGetValue(portKey, out var serialPort) && serialPort.IsOpen)
            {
                serialPort.Write(data);
            }
        }

        public string ReadFromSrp(string portKey)
        {
            if (_serialPorts.TryGetValue(portKey, out var serialPort) && serialPort.IsOpen)
            {
                return serialPort.ReadExisting();
            }
            return string.Empty;
        }

        public async Task<string> SrpWriteAndRead(string portKey, string data, string expectedResponse, int timeoutMs)
        {
            if (_serialPorts.TryGetValue(portKey, out var serialPort) && serialPort.IsOpen)
            {
                serialPort.DiscardInBuffer();
                serialPort.Write(data);

                var startTime = DateTime.Now;
                var response = string.Empty;

                while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
                {
                    response += serialPort.ReadExisting();
                    if (response.Contains(expectedResponse))
                    {
                        return response;
                    }
                    await Task.Delay(100);
                }

                return response; // Or throw a timeout exception
            }

            return string.Empty;
        }

        public void Dispose()
        {
            foreach (var port in _serialPorts.Values)
            {
                port.Dispose();
            }
        }
    }
}