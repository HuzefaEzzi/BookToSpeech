using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookToSpeech.FunctionsApp.Utils
{
    internal class FileUtils
    {
        public static string TrimFileName(string fileName, int length = 5)
        {
            string clean = MakeValidFileName(fileName);
            return clean[..Math.Min(length, clean.Length)].ToLower();
        }

        public static string MakeValidFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }
    }
}
