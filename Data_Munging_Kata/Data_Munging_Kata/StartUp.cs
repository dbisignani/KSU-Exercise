using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Munging_Kata
{
    public class StartUp
    {
        public StartUp()
        {
            Temperatures t = new Temperatures(85,70);
            FormattingElements fe = new FormattingElements();
            int N = -1;
            string p = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            if (Environment.GetCommandLineArgs().Length > 1)
            {
                try
                {
                    N = Convert.ToInt32(Environment.GetCommandLineArgs()[1]);
                }
                catch (Exception e)
                {
                    Console.WriteLine("The value of N is not an interger - this is a terminal error.  Program terminating: "+ e.Message);
                    throw;
                }
            }
            //LoadData loads the data from the file; also cleans up the data;
            LoadData ld = new LoadData();

            //It is possible that there are no days in the right temperature range
            var DaysWithRightTemperature = ld.Data.AsEnumerable().Where(dr => dr.Field<double>("High(celsius)") <= t.HighTempMax && dr.Field<double>("Low(celsius)") >= t.LowTempMin).Select(dr => new { DOW = dr.Field<string>("DayOfWeek(firstletter)"), DOM = dr.Field<double>("DayOfMonth"), COP = dr.Field<double>("ChanceOfPrecipitation") }).ToList();
            if (DaysWithRightTemperature.Count>=1)
            {
                var MinPrecip = DaysWithRightTemperature.Select(z => z.COP).Min<double>();
                var DaysRightTemperatureAndCOP = DaysWithRightTemperature.Where(m => m.COP == MinPrecip).ToList();
                foreach (var dr in DaysRightTemperatureAndCOP)
                {
                    Console.WriteLine("{0} the {1} day of the month is the best day for a picnic.", fe.FullDayName(dr.DOW), fe.GetDOMSuffix(dr.DOM));
                }
            }
            else
            {
                Console.WriteLine("There are no good days for a picnic");
            }


            //If the number of vacation days has been sent, then look for the N best days
            if (N > -1)
            {
                //DayswithRightTemperatire is n annonymous type.  We need to organize these days into groups, so, gove them a definite type (day)
                List<Day> AllPerfectDays = new List<Day>();
                foreach (var annDay in DaysWithRightTemperature)
                {
                    AllPerfectDays.Add(new Day { DayOfWeek = annDay.DOW, DayOfMonth = annDay.DOM, ChanceOfPrecip = annDay.COP });
                }
                if (AllPerfectDays.Count == 0)
                {
                    Console.WriteLine("There are no good days for a vacation");
                }
                else
                {
                    //Continue looking for a good time for a vacation.
                    //Can't use GroupBy here because the grouped by field must be the same property in all objects.  We want to group by consecutive days.
                    List<PerfectDaysInfo> PerfectDayGroups = new List<PerfectDaysInfo>();
                    PerfectDaysInfo pdi = null;
                    bool FirstTime = true;
                    double DayOfMonth = -1;
                    foreach (var day in AllPerfectDays.OrderBy(x => x.DayOfMonth))
                    {
                        if (FirstTime)
                        {
                            DayOfMonth = day.DayOfMonth;
                            pdi = new PerfectDaysInfo();
                            pdi.PerfectDays.Add(day);
                            FirstTime = false;
                        }
                        else
                        {
                            if (DayOfMonth == day.DayOfMonth - 1)
                            {
                                pdi.PerfectDays.Add(day);
                            }
                            else
                            {
                                PerfectDayGroups.Add(pdi);
                                pdi = new PerfectDaysInfo();
                                pdi.PerfectDays.Add(day);
                            }
                            DayOfMonth = day.DayOfMonth;
                        }
                    }
                    if (pdi != null)
                    {
                        PerfectDayGroups.Add(pdi);
                    }

                    //At this polint we have a list of the best days grouped together in lists by consecutive dates
                    //For each group, compute the average chance of precipitation.
                    foreach (PerfectDaysInfo pdi2 in PerfectDayGroups)
                    {
                        pdi2.AverageChanceOfPrecip = pdi2.PerfectDays.Average(d => d.ChanceOfPrecip);
                    }
                    // Get the lowest average chance of precip
                    double Min = PerfectDayGroups.Select(z => z.AverageChanceOfPrecip).Min();

                    List<PerfectDaysInfo> Vacation = PerfectDayGroups.Where(p2 => p2.AverageChanceOfPrecip == Min && p2.PerfectDays.Count >= N).ToList();
                    if (Vacation.Count == 0)
                    {
                        Console.WriteLine("There are no consecutive spans of days with the lowest chance of precipitation.");
                    }
                    else if (Vacation.Count == 1)
                    {
                        Console.WriteLine("The best days for a vacation is from {0} the {1} to {2} the {3}",
                        fe.FullDayName(Vacation[0].PerfectDays[0].DayOfWeek),
                        fe.GetDOMSuffix(Vacation[0].PerfectDays[0].DayOfMonth),
                        fe.FullDayName(Vacation[0].PerfectDays[Vacation[0].PerfectDays.Count - 1].DayOfWeek),
                        fe.GetDOMSuffix(Vacation[0].PerfectDays[Vacation[0].PerfectDays.Count - 1].DayOfMonth));
                    }
                    else
                    {
                        Vacation = PerfectDayGroups.Where(p3 => p3.PerfectDays.Count >= N).ToList();
                        if (Vacation.Count > 1)
                        {
                            Console.WriteLine("The best time(s) for your vacation is:");
                            foreach (PerfectDaysInfo pdi3 in Vacation)
                            {
                                List<Day> VacationDays = pdi3.PerfectDays.OrderBy(pd2 => pd2.DayOfMonth).ToList();
                                Console.WriteLine("The best days for a vacation is from {0} the {1} to {2} the {3}",
                                    fe.FullDayName(VacationDays[0].DayOfWeek),
                                    fe.GetDOMSuffix(VacationDays[0].DayOfMonth),
                                    fe.FullDayName(VacationDays[VacationDays.Count - 1].DayOfWeek),
                                    fe.GetDOMSuffix(VacationDays[VacationDays.Count - 1].DayOfMonth));
                                Console.WriteLine();
                            }
                        }
                    }
                }
            }
            Console.ReadLine();
        }


        public class Day
        {
            public string DayOfWeek { get; set; }
            public double DayOfMonth { get; set; }
            public double ChanceOfPrecip { get; set; }
        }

        public class PerfectDaysInfo
        {
            public List<Day> PerfectDays;   //pd { get; set; }
            public double AverageChanceOfPrecip { get; set; }

            public PerfectDaysInfo()
            {
                PerfectDays = new List<Day>();
            }
        }
        public struct Temperatures
        {
            private double _LowTempMin;
            private  double _HighTempMax;

            public double LowTempMin { get { return _LowTempMin; } }
            public double HighTempMax { get { return _HighTempMax; } }

            public  Temperatures(double HiFahrenheit, double LowFahrenheit)
            {
                _LowTempMin = (LowFahrenheit - 32) / 1.8;
                _HighTempMax = (HiFahrenheit - 32) / 1.8;
            }
        }

        public struct FormattingElements
        {
            public string GetDOMSuffix(double DOM)
            {
                string Suffix;
                if ((DOM % 20) == 1)
                {
                    Suffix = "st";
                }
                else if ((DOM % 20) == 2)
                {
                    Suffix = "nd";
                }
                else if ((DOM % 20) == 3)
                {
                    Suffix = "rd";
                }
                else
                {
                    Suffix = "th";
                }
                return DOM + Suffix;
            }

            public string FullDayName(string Symbol)
            {
                string DayName = string.Empty;
                if (Symbol=="m")
                {
                    DayName = "Monday";
                }
                else if (Symbol=="Tu")
                {
                    DayName = "Tuesday";
                }
                else if (Symbol=="w")
                {
                    DayName =  "Wednesday";
                }
                else if (Symbol=="Th")
                {
                    DayName =  "Thursday";
                }
                else if (Symbol == "f")
                {
                    DayName =  "Friday";
                }
                else if (Symbol=="Sa")
                {
                    DayName =  "Saturday";
                }
                else if (Symbol == "Su")
                {
                    DayName =  "Sunday";
                }
                return DayName;
            }
        }

    }
}
