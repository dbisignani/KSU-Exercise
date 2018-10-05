using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Munging_Kata
{
    class LoadData
    {
        public DataTable Data { get; } = new DataTable();
        public LoadData()
        {
            string path = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            bool Saturday = true;
            bool Tuesday = false;
            FileStream FS = new FileStream(path + @"\Data.txt", FileMode.Open, FileAccess.Read);
            StreamReader SR = new StreamReader(FS);

            //The first line is the column names.
            List<string> ColumnNames = SR.ReadLine().ToString().Trim().Split(' ').ToList();
            //Now create a column for each field name.
            foreach (string s in ColumnNames)
            {
                if (s.StartsWith("DayOfWeek"))
                {
                    Data.Columns.Add(new DataColumn(s, Type.GetType("System.String")));
                }
                else
                {
                    Data.Columns.Add(new DataColumn(s, Type.GetType("System.Double")));
                }
            }

            //Read and load the data             
            //The strategy here is to find the run of characters that are not alpha-numerics and replace then with a comma
            while (!SR.EndOfStream)
            {
                int index = 0;
                List<RunLength> RunLengths = new List<RunLength>();

                StringBuilder sb = new StringBuilder(SR.ReadLine());
                while (index <= sb.Length - 1)
                {
                    int Count = sb.RunLength(index); //return length of run of non alpha-numeric chars.  This alueis zero for alpha-numeric chars
                    if (Count > 0)
                    {
                        RunLengths.Add(new RunLength { StartingIndex = index, Length = Count });
                        index = index + Count;
                    }
                    else
                    {
                        index++;
                    }
                }
                //Replace the runlengths of non-alpha-numerics from right to left so tha indices point to the correct location in the string.
                for (int rlPtr = RunLengths.Count - 1; rlPtr >= 0; rlPtr--)
                {
                    RunLength rl = RunLengths[rlPtr];
                    sb.ReplaceRun(rl.StartingIndex, rl.Length);
                }
                //Clip the last comma
                Console.WriteLine(sb.Remove(sb.Length - 1, 1));
                DataRow r = Data.NewRow();
                r.ItemArray = sb.ToString().Split(',');
                Data.Rows.Add(r);
            }
            //Clean up the s values for the day of week;  first s is saturday, second is sunday
            foreach (DataRow dr in Data.Rows)
            {
                if (dr["DayOfWeek(firstletter)"].ToString() == "s")
                {
                    if (Saturday)
                    {
                        dr["DayOfWeek(firstletter)"] = "Sa";
                        Saturday = false;
                    }
                    else
                    {
                        dr["DayOfWeek(firstletter)"] = "Su";
                        Saturday = true;
                    }
                }

                if (dr["DayOfWeek(firstletter)"].ToString() == "t")
                {
                    if (Tuesday)
                    {
                        dr["DayOfWeek(firstletter)"] = "Tu";
                        Tuesday = false;
                    }
                    else
                    {
                        dr["DayOfWeek(firstletter)"] = "Th";
                        Tuesday = true;
                    }
                }
            }


        }

    }
}
