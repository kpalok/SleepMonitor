using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Reflection;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Xml;

namespace SleepMonitor
{
    public class Logic
    {
        private string filename = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Nights.xml");

        public Nights ReadNightsFile()
        {
            FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            XmlSerializer reader = new XmlSerializer(typeof(Nights));
            Nights nights = reader.Deserialize(fileStream) as Nights;
            fileStream.Flush();
            fileStream.Dispose();
            return nights;
        }

        public List<Night> GetLastTenNights(Nights nights)
        {
            nights.NightList.Sort(new NightRecentFirst());
            if (nights.NightList.Count > 10)
            {
                return nights.NightList.Take(10).ToList();
            }
            else
            {
                return nights.NightList;
            }
        }

        public void AddNewNight(Night night)
        {
            XmlDocument document = new XmlDocument();
            document.Load(filename);
            

        }

        public void WriteNewNightsFile(List<Night> nightList)
        {
            Nights nights = new Nights();
            nights.NightList = nightList;

            FileStream fileStream = new FileStream(filename, FileMode.CreateNew);
            XmlSerializer writer = new XmlSerializer(typeof(Nights));
            writer.Serialize(fileStream, nights);
            fileStream.Flush();
            fileStream.Dispose();
        }

        public void WriteTestFile()
        {
            Nights nights = new Nights();
            nights.NightList = new List<Night>();
            for (int i = 1; i<=7; i++)
            {
                Night night = new Night()
                {
                    StartTime = new DateTime(2018, 7, i, 20, 30, 00),
                    EndTime = new DateTime(2018, 7, i + 1, 4 + i, 00, 00),
                };
                night.TimeSleptInMinutes = night.EndTime.Subtract(night.StartTime).TotalMinutes;
                nights.NightList.Add(night);
            }
            FileStream fileStream = new FileStream(filename, FileMode.OpenOrCreate);
            XmlSerializer writer = new XmlSerializer(typeof(Nights));
            writer.Serialize(fileStream, nights);
            fileStream.Flush();
            fileStream.Dispose();
        }

        public TimeSpan CalculateOverallMeanTime(List<Night> nights)
        {
            List<double> nightsInMinutes = new List<double>();

            foreach (Night night in nights)
            {
                nightsInMinutes.Add(night.TimeSleptInMinutes);
            }

            if (nightsInMinutes.Count > 0)
            {
                double meanInMinutes = nightsInMinutes.Sum() / nightsInMinutes.Count;
                TimeSpan meanTime = TimeSpan.FromMinutes(meanInMinutes);
                return meanTime;
            }
            else return new TimeSpan();
        }

        public TimeSpan CalculateWeekDayMeanTime(List<Night> nights)
        {
            List<double> nightsInMinutes = new List<double>();

            List<Night> weekdayNights = (from Night night in nights
                                         where night.EndTime.DayOfWeek != DayOfWeek.Saturday &&
                                         night.EndTime.DayOfWeek != DayOfWeek.Sunday
                                         select night).ToList();

            foreach (Night night in weekdayNights)
            {
                nightsInMinutes.Add(night.TimeSleptInMinutes);
            }

            if (nightsInMinutes.Count > 0)
            {
                double meanInMinutes = nightsInMinutes.Sum() / nightsInMinutes.Count;
                TimeSpan meanTime = TimeSpan.FromMinutes(meanInMinutes);
                return meanTime;
            }
            else return new TimeSpan();
        }

        public TimeSpan CalculateWeekEndMeanTime(List<Night> nights)
        {
            List<double> nightsInMinutes = new List<double>();

            List<Night> weekEndNights = (from Night night in nights
                                         where night.EndTime.DayOfWeek == DayOfWeek.Saturday ||
                                         night.EndTime.DayOfWeek == DayOfWeek.Sunday
                                         select night).ToList();

            foreach (Night night in weekEndNights)
            {
                nightsInMinutes.Add(night.TimeSleptInMinutes);
            }
            if (nightsInMinutes.Count > 0)
            {
                double meanInMinutes = nightsInMinutes.Sum() / nightsInMinutes.Count;
                TimeSpan meanTime = TimeSpan.FromMinutes(meanInMinutes);
                return meanTime;
            }
            else return new TimeSpan();
        }

        public decimal CalculatePercentageOverEightHours(List<Night> nights, bool onlyLastWeeek)
        {
            DayOfWeek weekStart = DayOfWeek.Monday;
            DateTime startingDate = DateTime.Today;

            while (startingDate.DayOfWeek != weekStart)
                startingDate = startingDate.AddDays(-1);

            DateTime previousWeekStart = startingDate.AddDays(-7);
            DateTime previousWeekEnd = startingDate.AddDays(-1);
            if (onlyLastWeeek)
            {
                List<Night> lastWeek;
                try
                {
                    lastWeek = (from Night night in nights
                                where night.EndTime.Ticks > previousWeekStart.Ticks &&
                                night.EndTime.Ticks < previousWeekEnd.Ticks
                                select night).ToList();
                }
                catch (Exception) { return 0; }

                int timesSleptOverEightHours = (from Night night in lastWeek
                                                where night.TimeSleptInMinutes >= 8 * 60
                                                select night).ToList().Count;

                return (timesSleptOverEightHours / lastWeek.Count) * 100;
            }
            else
            {
                int timesSleptOverEightHours = (from Night night in nights
                                                where night.TimeSleptInMinutes >= 8 * 60
                                                select night).ToList().Count;

                return (timesSleptOverEightHours / nights.Count) * 100;
            }
        }

        public Tuple<DayOfWeek, TimeSpan> GetBestDay(List<Night> nights)
        {
            List<TimeSpan> means = new List<TimeSpan>();
            foreach (int i in Enum.GetValues(typeof(DayOfWeek)))
            {
                means.Add(CalculateMeanPerDay(nights, (DayOfWeek)i));
            }

             return Tuple.Create((DayOfWeek)means.IndexOf(means.Max()), means.Max());
        }

        public Tuple<DayOfWeek, TimeSpan> GetWorstDay(List<Night> nights)
        {
            List<TimeSpan> means = new List<TimeSpan>();
            foreach (int i in Enum.GetValues(typeof(DayOfWeek)))
            {
                means.Add(CalculateMeanPerDay(nights, (DayOfWeek)i));
            }

            return Tuple.Create((DayOfWeek)means.IndexOf(means.Min()), means.Min());
        }

        private TimeSpan CalculateMeanPerDay(List<Night> nights, DayOfWeek endDay)
        {
            List<Night> nightList = (from Night night in nights
                                     where night.EndTime.DayOfWeek == endDay
                                     select night).ToList();

            return CalculateOverallMeanTime(nightList);
        }
    }
}