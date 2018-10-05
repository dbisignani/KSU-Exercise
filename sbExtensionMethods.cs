using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Munging_Kata
{
    public static class SBExtensions
    {
        public static int RunLength(this StringBuilder str, int startingLocation)
        {
            int runLength = 0;
            for (int i = startingLocation; i <= str.Length - 1; i++)
            {
                int AsciiValue = Microsoft.VisualBasic.Strings.Asc(str[i]);
                //Keep upper and lower case characters and digits;
                if ((AsciiValue >= 40 && AsciiValue <= 41) || (AsciiValue >= 48 && AsciiValue <= 57) || (AsciiValue >= 65 && AsciiValue <= 90) || (AsciiValue >= 97 && AsciiValue <= 122))
                {
                    break;
                }
                else
                {
                    runLength++;
                }
            }
            return runLength;
        }

        public static void ReplaceRun(this StringBuilder str, int StartIndex, int Length)
        {
            bool FirstTime = true;
            for (int ptr = StartIndex; ptr <= StartIndex + Length - 1; ptr++)
            {
                //Set the first charcater to a delimiter character
                if (FirstTime)
                {
                    str[ptr] = ',';
                    FirstTime = false;
                }
                else
                {
                    str[ptr] = '\t';
                }
            }
            str.Replace("\t", "", StartIndex, Length);
        }
    }

    public class RunLength
    {
        public int StartingIndex { get; set; }
        public int Length { get; set; }
    }
}
