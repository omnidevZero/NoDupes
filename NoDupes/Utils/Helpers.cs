using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NoDupes.Utils
{
    public class Helpers
    {
        public static void LogToDebug(string text)
        {
            System.Diagnostics.Trace.WriteLine(text);
        }
        public static string GetMD5(string filePath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);
            var md5Hash = md5.ComputeHash(stream);
            var md5String = BitConverter.ToString(md5Hash).Replace("-", "").ToLowerInvariant();
            return md5String;
        }
    }
}
