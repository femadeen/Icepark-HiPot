using System.Collections.Concurrent;
using System.Data;
using System.Text;
using System.Xml;

namespace Hipot.Data;

public class DataService
{
    private readonly ConcurrentDictionary<int, DataTable> _tempData = new();
    private readonly ConcurrentDictionary<int, DataTable> _mainScanData = new();

    // Methods for temporary data

    public void CreateTempDataTable(int idm)
    {
        var dt = new DataTable();
        dt.Columns.Add("vIdx", typeof(int));
        dt.Columns.Add("vName", typeof(string));
        dt.Columns.Add("vValue", typeof(string));
        _tempData[idm] = dt;
    }

    public void PutTempData(int idm, string name, string value)
    {
        if (TempDataExists(idm, name))
        {
            UpdateTempDataRow(idm, name, value);
        }
        else
        {
            AddTempDataRow(idm, name, value);
        }
    }

    public string GetAllTempDataAsString(int idm)
    {
        if (!_tempData.ContainsKey(idm)) return "";

        var sb = new StringBuilder();
        foreach (DataRow row in _tempData[idm].Rows)
        {
            var vName = row["vName"].ToString();
            if (!ShouldHideParameter(vName))
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }
                sb.Append($"{vName} : {row["vValue"]}");
            }
        }
        sb.AppendLine("#____________________________________________#");
        return sb.ToString();
    }

    private bool ShouldHideParameter(string pName)
    {
        return pName.ToUpper() switch
        {
            "BXML" or "XML" or "TICKET" or "ODCCURST" => true,
            _ => false,
        };
    }

    private void AddTempDataRow(int idm, string name, string value)
    {
        var dt = _tempData.GetOrAdd(idm, (_) => {
            var newDt = new DataTable();
            newDt.Columns.Add("vIdx", typeof(int));
            newDt.Columns.Add("vName", typeof(string));
            newDt.Columns.Add("vValue", typeof(string));
            return newDt;
        });

        var vIdx = dt.Rows.Count;
        dt.Rows.Add(vIdx, name, value);
    }

    private void UpdateTempDataRow(int idm, string name, string value)
    {
        if (!_tempData.ContainsKey(idm)) return;

        var rows = _tempData[idm].Select($"vName = '{name}'");
        if (rows.Length > 0)
        {
            rows[0]["vValue"] = value;
        }
    }

    public string GetTempData(int idm, string name)
    {
        if (!_tempData.ContainsKey(idm)) return "";

        var rows = _tempData[idm].Select($"vName = '{name}'");
        return rows.Length > 0 ? rows[0]["vValue"].ToString() : "";
    }

    public bool TempDataExists(int idm, string name)
    {
        return _tempData.ContainsKey(idm) && _tempData[idm].Select($"vName = '{name}'").Length > 0;
    }

    public void DestroyTempData(int idm)
    {
        _tempData.TryRemove(idm, out _);
    }

    // Methods for main sequence data

    public bool LoadMainSequence(int idm, string xmlPath, Action<int, int> initProgressBar, Action<int, string> appMain, Func<string, string> getTestHeader)
    {
        try
        {
            DestroyMainScanData(idm);
            CreateMainScanTable(idm);

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);

            ProcessSequenceNodes(idm, xmlDoc, "maintest/pretest", "PRETEST");
            ProcessSequenceNodes(idm, xmlDoc, "maintest/test", "TEST");
            ProcessSequenceNodes(idm, xmlDoc, "maintest/posttest", "POSTTEST");

            initProgressBar(idm, _mainScanData[idm].Rows.Count);
            appMain(idm, getTestHeader(xmlPath));
            var uSN = GetTempData(idm, "UUT_SN"); // Assuming UUT_SN is stored in temp data
            appMain(idm, $"UUT's Serial number : {uSN}");
            appMain(idm, GetAllTempDataAsString(idm));

            return true;
        }
        catch (Exception ex)
        {
            // In a real app, use a proper logging framework and show a user-friendly error.
            Console.WriteLine($"Load Main Sequence Error: {ex.Message}");
            return false;
        }
    }

    private void ProcessSequenceNodes(int idm, XmlDocument xmlDoc, string xpath, string testType)
    {
        var nodeList = xmlDoc.SelectNodes(xpath);
        if (nodeList == null) return;

        foreach (XmlNode node in nodeList)
        {
            AddMainScanRow(idm, "IDLE", testType, node.Attributes?["tname"]?.Value ?? "", "NA");
            foreach (XmlNode childNode in node.ChildNodes)
            {
                var status = childNode.Name.ToUpper() != "#COMMENT" ? "IDLE" : "DONE";
                AddMainScanRow(idm, status, "INNER", childNode.Name.ToUpper(), childNode.InnerText);
            }
        }
    }

    public void CreateMainScanTable(int idm)
    {
        var dt = new DataTable();
        dt.Columns.Add("vIdm", typeof(int));
        dt.Columns.Add("vStatus", typeof(string));
        dt.Columns.Add("vType", typeof(string));
        dt.Columns.Add("vFunction", typeof(string));
        dt.Columns.Add("vVariable", typeof(string));
        _mainScanData[idm] = dt;
    }

    public void AddMainScanRow(int idm, string status, string type, string function, string variable)
    {
        if (!_mainScanData.ContainsKey(idm)) CreateMainScanTable(idm);
        var vIdm = _mainScanData[idm].Rows.Count;
        _mainScanData[idm].Rows.Add(vIdm, status, type, function, variable);
    }

    public void UpdateMainScanRow(int idm, int pointer, string status)
    {
        if (_mainScanData.ContainsKey(idm) && pointer < _mainScanData[idm].Rows.Count)
        {
            _mainScanData[idm].Rows[pointer]["vStatus"] = status;
        }
    }

    public DataRow GetMainScanRow(int idm, int pointer)
    {
        if (_mainScanData.ContainsKey(idm) && pointer < _mainScanData[idm].Rows.Count)
        {
            return _mainScanData[idm].Rows[pointer];
        }
        return null;
    }

    public void DestroyMainScanData(int idm)
    {
        _mainScanData.TryRemove(idm, out _);
    }

    public int GetMainScanRowCount(int idm)
    {
        if (_mainScanData.TryGetValue(idm, out var table))
        {
            return table.Rows.Count;
        }
        return 0;
    }
}