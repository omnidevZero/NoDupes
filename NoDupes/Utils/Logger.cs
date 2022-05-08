using System;
using System.Collections.Generic;
using System.Text;

namespace NoDupes.Utils
{
    public class Logger
    {
        public static string Log(string text)
        {
            return DateTime.Now.ToString("HH:mm:ss") + $" - {text}\n";
        }
    }
}
