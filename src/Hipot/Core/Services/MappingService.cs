using System;
using System.Threading.Tasks;
using Hipot.Core.Services.Implementations;
using Pekasuz3.Maui.Data;

namespace Hipot.Core.Services;

public class MappingService
{
    private readonly DataService _dataService;
    private readonly HttpService _httpService;
    private readonly SerialPortService _serialPortService;

    public MappingService(DataService dataService, HttpService httpService, SerialPortService serialPortService)
    {
        _dataService = dataService;
        _httpService = httpService;
        _serialPortService = serialPortService;
    }

    public async Task MapFunction(TestChannelState state, string functionName, string arguments)
    {
        // First, replace any standard variables in the arguments string
        string processedArgs = ReplaceStandardVariables(state, arguments);
        string[] args = processedArgs.Split(',');

        switch (functionName.ToUpper())
        {
            case "DELAY":
                await HandleDelay(state, args);
                break;

            case "DISPLAY":
                HandleDisplay(state, args);
                break;

            case "MESSAGE":
                HandleMessage(state, args);
                break;

            case "CHKVALUE":
                HandleCheckValue(state, args);
                break;

            case "CHKINWORD":
                HandleCheckInWord(state, args);
                break;

            case "SRPWONLY":
                HandleSrpWriteOnly(state, args);
                break;

            case "SRPRONLY":
                HandleSrpReadOnly(state, args);
                break;

            // ... other cases will be added here

            default:
                // Function not implemented yet
                _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "FAIL");
                break;
        }
    }

    private void HandleSrpWriteOnly(TestChannelState state, string[] args)
    {
        try
        {
            string portKey = $"{state.Idm}_{args[0]}"; // Construct the port key
            string data = args[1];
            string suffix = args.Length > 2 ? GetSuffix(args[2]) : "\r";

            _serialPortService.WriteToSrp(portKey, data + suffix);
            _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "DONE");
        }
        catch (Exception ex)
        {
            // Log exception
            _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "ABORT");
        }
    }

    private string GetSuffix(string suffixArg)
    {
        return suffixArg.ToUpper() switch
        {
            "[*CR*]" => "\r",
            "[*CRLF*]" => "\r\n",
            "[*TAB*]" => "\t",
            "[*NA*]" => "",
            _ => suffixArg,
        };
    }

    private string ReplaceStandardVariables(TestChannelState state, string text)
    {
        // This is a simplified implementation of ReplaceStdVar
        // In a real app, a more robust parsing mechanism would be better.
        if (!text.Contains("(*")) return text;

        // Example: replace "(*UUTSN*)" with the serial number
        return text.Replace("(*UUTSN*)", state.SerialNumber);
        // This needs to be expanded to handle all standard variables from GetValFromStdParam
    }

    private async Task HandleDelay(TestChannelState state, string[] args)
    {
        if (int.TryParse(args[0], out int delayMs))
        {
            await Task.Delay(delayMs);
            _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "DONE");
        }
        else
        {
            _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "FAIL");
        }
    }

    private void HandleDisplay(TestChannelState state, string[] args)
    {
        // This would raise an event to be handled by the UI
        Console.WriteLine($"DISPLAY for {state.Idm}: {args[0]}");
        _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "DONE");
    }

    private void HandleMessage(TestChannelState state, string[] args)
    {
        // This would raise an event for a modal dialog in the UI
        Console.WriteLine($"MESSAGE for {state.Idm}: {args[0]}");
        // The original code waits for user input. This needs a more complex UI interaction mechanism.
        _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "DONE");
    }

    private void HandleCheckValue(TestChannelState state, string[] args)
    {
        string inputValue = _dataService.GetTempData(state.Idm, args[0]);
        string[] minMax = args[1].Split('_');

        if (float.TryParse(inputValue, out float val) &&
            float.TryParse(minMax[0], out float min) &&
            float.TryParse(minMax[1], out float max))
        {
            if (val >= min && val <= max)
            {
                _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "PASS");
            }
            else
            {
                _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "FAIL");
            }
        }
        else
        {
            _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "FAIL");
        }
    }

    private void HandleCheckInWord(TestChannelState state, string[] args)
    {
        string inputValue = _dataService.GetTempData(state.Idm, args[0]);
        string wordToFind = args[1];

        if (inputValue.Contains(wordToFind))
        {
            _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "PASS");
        }
        else
        {
            _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "FAIL");
        }
    }

    private void HandleSrpReadOnly(TestChannelState state, string[] args)
    {
        try
        {
            string portKey = $"{state.Idm}_{args[0]}"; // Construct the port key
            string portName = args[0];

            string newData = _serialPortService.ReadFromSrp(portKey);

            if (!state.PortData.ContainsKey(portName))
            {
                state.PortData[portName] = string.Empty;
            }

            state.PortData[portName] += newData;

            // For compatibility with functions that use GetValFromStdParam
            _dataService.PutTempData(state.Idm, portName, state.PortData[portName]);

            _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "DONE");
        }
        catch (Exception ex)
        {
            // Log exception
            _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "ABORT");
        }
    }
}
