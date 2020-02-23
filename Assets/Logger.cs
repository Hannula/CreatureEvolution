using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Logger : MonoBehaviour
{
    public Text textLog;

    private List<LogEntry> entries;

    public void Start()
    {
        entries = new List<LogEntry>();
    }

    public void Log(string message)
    {
        LogEntry newEntry = new LogEntry(message);
        entries.Add(newEntry);
        string text = "";
        for(int i = 0; i < Mathf.Min(20, entries.Count);  ++i)
        {
            var entry = entries[entries.Count - 1 - i];
            text += entry.ToString() + "\n";
        }
        textLog.text = text; 
    }
}

public struct LogEntry
{
    public System.DateTime time;

    public string message;

    public LogEntry(string message)
    {
        time = System.DateTime.Now;

        this.message = message;
    }

    public override string ToString()
    {
        return time.ToString() + ": " + message;
    }
}
