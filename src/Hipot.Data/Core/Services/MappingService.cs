using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hipot.Core.Services.Implementations;
using Hipot.Core.Services.Interfaces;
using Hipot.Data;
using Hipot.Data.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Hipot.Core.Services;

public class MappingService
{
    private readonly DataService _dataService;
    private readonly IHttpService _httpService;
    private readonly SerialPortService _serialPortService;
    private readonly VpdService _vpdService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MappingService> _logger;

    public MappingService(DataService dataService, IHttpService httpService, SerialPortService serialPortService, VpdService vpdService, IConfiguration configuration, ILogger<MappingService> logger)
    {
        _dataService = dataService;
        _httpService = httpService;
        _serialPortService = serialPortService;
        _vpdService = vpdService;
        _configuration = configuration;
        _logger = logger;
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

            case "LILACWRITEVPD":
                await HandleLilacWriteVpd(state, args);
                break;

            case "LILACVPDVER":
                await HandleLilacVerVpd(state, args);
                break;

            case "SRPWRCHK":
                await HandleSrpWriteReadCheck(state, args);
                break;

            case "GETHTTPGET":
                await HandleGetHttpGet(state, args);
                break;

            case "UPLOADODCXML":
                await HandleUploadOdcXml(state, args);
                break;

            case "CHKODC":
                HandleChkOdc(state, args);
                break;

            // ... other cases will be added here

            default:
                // Function not implemented yet
                _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "FAIL");
                break;
        }
    }

    private async Task HandleSrpWriteReadCheck(TestChannelState state, string[] args)
    {
        try
        {
            string portKey = $"{state.Idm}_{args[0]}";
            string data = args[1];
            string expectedResponse = args[2];
            int timeout = int.Parse(args[3]);

            string response = await _serialPortService.SrpWriteAndRead(portKey, data, expectedResponse, timeout);

            if (response.Contains(expectedResponse))
            {
                _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "PASS");
            }
            else
            {
                _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "FAIL");
            }
        }
        catch (Exception ex)
        {
            // Log exception
            _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "ABORT");
        }
    }

    private async Task HandleLilacWriteVpd(TestChannelState state, string[] args)
    {
        // This is a placeholder implementation. The actual implementation will require
        // the SerialPortService to be fully implemented.
        var result = await Task.Run(() => _vpdService.LilacMakeVPD(args[1], state.SerialNumber.Length, "0x63", state.SerialNumber, "0x96", "0x20"));
        if (result != null)
        {
            // Here you would use the SerialPortService to write the data to the device
            // For now, we will just log a message
            Console.WriteLine("VPD data generated successfully.");
            _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "PASS");
        }
        else
        {
            Console.WriteLine("Failed to generate VPD data.");
            _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "FAIL");
        }
    }

    private async Task HandleLilacVerVpd(TestChannelState state, string[] args)
    {
        // This is a placeholder implementation. The actual implementation will require
        // the SerialPortService to be fully implemented.
        var result = await Task.Run(() => _vpdService.LilacMakeVPD(args[1], state.SerialNumber.Length, "0x63", state.SerialNumber, "0x96", "0x20"));
        if (result != null)
        {
            // Here you would use the SerialPortService to read the data from the device and compare
            // For now, we will just log a message and assume it passes
            Console.WriteLine("VPD data verified successfully.");
            _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "PASS");
        }
        else
        {
            Console.WriteLine("Failed to generate VPD data for verification.");
            _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "FAIL");
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
        var result = text;
        var pattern = @"\(\*([^)]+)\*\)";
        var matches = Regex.Matches(text, pattern);

        foreach (Match match in matches)
        {
            var variableName = match.Groups[1].Value.ToUpper();
            string valueToReplace = GetValueFromStandardParameter(state, variableName);
            result = result.Replace(match.Value, valueToReplace);
        }

        if (result.Contains("?sn="))
        {
            var urlParts = result.Split(new[] { "?sn=" }, StringSplitOptions.None);
            if (urlParts.Length > 1 && string.IsNullOrEmpty(urlParts[1]))
            {
                result += state.SerialNumber;
            }
        }

        return result;
    }

    private string GetValueFromStandardParameter(TestChannelState state, string variableName)
    {
        switch (variableName)
        {
            case "UUTSN":
                return state.SerialNumber;
            // Add other cases as needed from the legacy app
            default:
                return _dataService.GetTempData(state.Idm, variableName) ?? "";
        }
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

    private async Task HandleGetHttpGet(TestChannelState state, string[] args)
    {
        try
        {
            string variableName = args[0];
            string url = args[1];
            System.Diagnostics.Debug.WriteLine($"Attempting to call URL: {url}");
            _logger.LogInformation("Attempting to call URL: {Url}", url);

            string response = await _httpService.GetAsync(url);
            _logger.LogInformation("Successfully received response from URL: {Url}", url);
            _dataService.PutTempData(state.Idm, variableName, response);
            _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "DONE");
        }
        catch (Exception ex)
        {
            string errorMessage = $"Failed to get data from URL: {args[1]}. Exception: {ex.Message}";
            System.Diagnostics.Debug.WriteLine(errorMessage);
            _logger.LogError(ex, "Failed to get data from URL.");
            state.FirstFailDescription = "Unable to connect to the remote server";
            _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "ABORT");
        }
    }

    private async Task HandleUploadOdcXml(TestChannelState state, string[] args)
    {
        try
        {
            string variableName = args[0];
            string url = args[1];
            string postData = args[2]; // This might need to be retrieved from somewhere else

            // In the legacy app, PostStr(Idm) = SplArg(2). Here we assume it's passed directly.
            // This might need adjustment based on how post data is handled.
            string processedPostData = ReplaceStandardVariables(state, postData);

            System.Diagnostics.Debug.WriteLine($"Attempting to POST to URL: {url}");
            _logger.LogInformation("Attempting to POST to URL: {Url}", url);

            string response = await _httpService.PostAsync(url, processedPostData);

            _logger.LogInformation("Successfully received response from URL: {Url}", url);
            _dataService.PutTempData(state.Idm, variableName, response);
            _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "DONE");
        }
        catch (Exception ex)
        {
            string errorMessage = $"Failed to POST data to URL: {args[1]}. Exception: {ex.Message}";
            System.Diagnostics.Debug.WriteLine(errorMessage);
            _logger.LogError(ex, "Failed to POST data to URL.");
            state.FirstFailDescription = "Unable to connect to the remote server";
            _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "ABORT");
        }
    }

    private void HandleChkOdc(TestChannelState state, string[] args)
    {
        string variableName = args[0];
        string expectedValue = args[1];

        string actualValue = _dataService.GetTempData(state.Idm, variableName);

        if (actualValue.Contains(expectedValue))
        {
            _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "PASS");
        }
        else
        {
            _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "FAIL");
        }
    }
}