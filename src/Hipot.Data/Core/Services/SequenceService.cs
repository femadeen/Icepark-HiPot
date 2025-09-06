using Hipot.Core.DTOs;
using System.Collections.Concurrent;
using System.Data;
using System.Threading.Tasks;
using Hipot.Core.Services.Implementations;
using Hipot.Core.Services.Interfaces;
using Hipot.Data;
using Hipot.Data.Core.Services;
using Hipot.Data.Core.Services.Implementations.XML;

namespace Hipot.Core.Services;

public class TestChannelState
{
    public int Idm { get; set; }
    public int SequencePointer { get; set; }
    public string CurrentStatus { get; set; } = "IDLE";
    public bool ShouldStop { get; set; }
    public bool FirstFailHit { get; set; }
    public bool IsPostTestReached { get; set; }
    public string TestResult { get; set; } = "PASSED";
    public string FirstFailDescription { get; set; }
    public string CurrentTestStepName { get; set; }
    public string SerialNumber { get; set; }
    public TestHeaderInfo HeaderInfo { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public Dictionary<string, string> PortData { get; set; } = new();
    public System.Text.StringBuilder MainLog { get; } = new();
    public System.Text.StringBuilder DetailLog { get; } = new();

    public void Reset()
    {
        SequencePointer = 0;
        CurrentStatus = "IDLE";
        ShouldStop = false;
        FirstFailHit = false;
        IsPostTestReached = false;
        TestResult = "PASSED";
        FirstFailDescription = null;
        CurrentTestStepName = null;
        PortData.Clear();
        MainLog.Clear();
        DetailLog.Clear();
    }
}

public class SequenceService
{
    private readonly DataService _dataService;
    private readonly XmlLogService _logService;
    private readonly MappingService _mappingService;

    public event Action<int, string> OnLogMessage;
    public event Action<int, string> OnDetailLogMessage;
    public event Action<int> OnTestCompleted;
    public event Action<int, int> OnProgressUpdate;

    private readonly ConcurrentDictionary<int, TestChannelState> _channelStates = new();

    public SequenceService(DataService dataService, XmlLogService logService, MappingService mappingService)
    {
        _dataService = dataService;
        _logService = logService;
        _mappingService = mappingService;
    }

    public void InitializeChannel(int idm, string serialNumber)
    {
        _channelStates[idm] = new TestChannelState { Idm = idm, SerialNumber = serialNumber };
    }

    public async Task ExecuteTestSequenceAsync(int idm)
    {
        if (!_channelStates.TryGetValue(idm, out var state)) return;

        state.Reset();
        state.CurrentStatus = "TESTING";

        var mainScanTable = _dataService.GetMainScanDataTable(idm);

        foreach (DataRow row in mainScanTable.Rows)
        {
            if (state.ShouldStop) break;

            state.SequencePointer = (int)row["vIdm"];
            OnProgressUpdate?.Invoke(idm, state.SequencePointer);

            string rowType = row["vType"].ToString();
            string rowStatus = row["vStatus"].ToString();
            string rowFunction = row["vFunction"].ToString();
            string rowVariable = row["vVariable"].ToString();

            if (rowType != "INNER" && rowStatus == "IDLE")
            {
                HandleMajorStep(state, rowType, rowFunction);
            }
            else
            {
                if (!await HandleInnerStep(state, rowStatus, rowFunction, rowVariable))
                {
                    break; // Test aborted or finished
                }
            }
        }

        if (!state.IsPostTestReached)
        {
            FinalizeTest(state);
        }

        EndTest(idm);
    }

    private void HandleMajorStep(TestChannelState state, string type, string function)
    {
        OnDetailLogMessage?.Invoke(state.Idm, $"\n{type} Step : {function}");
        state.CurrentTestStepName = function;
        state.CurrentStatus = type.ToUpper();
        if (type.ToUpper() == "POSTTEST")
        {
            state.IsPostTestReached = true;
            FinalizeTest(state);
        }
        _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "DONE");
    }

    private async Task<bool> HandleInnerStep(TestChannelState state, string status, string function, string variable)
    {
        switch (status.ToUpper())
        {
            case "IDLE":
                if (state.ShouldStop)
                {
                    _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "DONE");
                }
                else
                {
                    _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "WAIT");
                    await _mappingService.MapFunction(state, function, variable); // This will update the state
                }
                break;

            case "PASS":
                OnLogMessage?.Invoke(state.Idm, "***REPLACEPASS***");
                _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "DONE");
                break;

            case "FAIL":
                OnLogMessage?.Invoke(state.Idm, "***REPLACEFAIL***");
                _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "DONE");
                if (!state.FirstFailHit)
                {
                    state.FirstFailHit = true;
                    state.FirstFailDescription = state.CurrentTestStepName;
                    state.TestResult = "FAILED";
                }
                break;

            case "STOPTEST":
                 OnLogMessage?.Invoke(state.Idm, "***REPLACEFAILSKIP***");
                _dataService.UpdateMainScanRow(state.Idm, state.SequencePointer, "DONE");
                if (!state.FirstFailHit)
                {
                    state.FirstFailHit = true;
                    state.FirstFailDescription = state.CurrentTestStepName;
                    state.TestResult = "FAILED";
                }
                state.ShouldStop = true;
                break;

            case "ABORT":
            case "ABORTODC":
                FinalizeTest(state, isAbort: true);
                return false; // End execution

            case "DONE":
            default:
                // Do nothing, just move to the next step
                break;
        }
        return true; // Continue execution
    }

    private void FinalizeTest(TestChannelState state, bool isAbort = false)
    {
        OnLogMessage?.Invoke(state.Idm, $"Ending Test : {DateTime.Now:yyyy/MM/dd HH:mm:ss}\n");
        OnLogMessage?.Invoke(state.Idm, $"TEST RESULT : {state.TestResult}\n");

        if (!isAbort)
        {
            _logService.GenerateLog(state.SerialNumber, state.TestResult, state.MainLog.ToString(), state.DetailLog.ToString(), $"CH{state.Idm}");
        }
    }

    private void EndTest(int idm)
    {
        if (_channelStates.TryGetValue(idm, out var state))
        {
            OnTestCompleted?.Invoke(idm);
        }
    }
}