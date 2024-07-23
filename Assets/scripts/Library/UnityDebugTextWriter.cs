using System.IO;
using System.Text;
using UnityEngine;

public class UnityDebugTextWriter : TextWriter
{
    StringBuilder buffer = new StringBuilder();

    public override void Write(char value)
    {
        if (value == '\n')
        {
            Debug.Log(buffer.ToString());
            buffer.Clear();
        }
        else
        {
            buffer.Append(value);
        }
    }

    public override Encoding Encoding
    {
        get { return Encoding.UTF8; }
    }
}