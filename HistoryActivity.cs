using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace SleepMonitor
{
    [Activity(Label = "HistoryActivity")]
    public class HistoryActivity : Activity
    {
        private Nights nights;

        private Logic logic = new Logic();

        private TextView txtTopTen;
        private TextView txtOverallMean;
        private TextView txtWeekdayMean;
        private TextView txtWeekendMean;
        private TextView txtPercentageLastWeek;
        private TextView txtPercentageAllTime;
        private TextView txtBestNight;
        private TextView txtWorstNight;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.History);
            txtTopTen = FindViewById<TextView>(Resource.Id.txtViewTopTen);
            txtOverallMean = FindViewById<TextView>(Resource.Id.txtOverallMean);
            txtWeekdayMean = FindViewById<TextView>(Resource.Id.txtWeekdayMean);
            txtWeekendMean = FindViewById<TextView>(Resource.Id.txtWeekendMean);
            txtPercentageLastWeek = FindViewById<TextView>(Resource.Id.txtPercentageLastWeek);
            txtPercentageAllTime = FindViewById<TextView>(Resource.Id.txtPercentageAllTime);
            txtBestNight = FindViewById<TextView>(Resource.Id.txtBestNight);
            txtWorstNight = FindViewById<TextView>(Resource.Id.txtWorstNight);
            nights = logic.ReadNightsFile();
            BindLastTenNightsTxt();
            BindMeanTimes();
            BindPercentages();
            BindBest();
            BindWorst();//eitoimi
            
        }

        private void BindLastTenNightsTxt()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Night night in logic.GetLastTenNights(nights))
            {
                TimeSpan timeSlept = TimeSpan.FromMinutes(night.TimeSleptInMinutes);
                sb.AppendLine($"{night.StartTime.DayOfWeek} {night.StartTime.Day} - {night.EndTime.DayOfWeek} {night.EndTime.Day} {timeSlept.Hours}h {timeSlept.Minutes}min");
            }

            RunOnUiThread(() => txtTopTen.SetLines(logic.GetLastTenNights(nights).Count));
            RunOnUiThread(() => txtTopTen.Text = sb.ToString());
        }

        private void BindMeanTimes()
        {
            TimeSpan overallMean = logic.CalculateOverallMeanTime(nights.NightList);
            TimeSpan weekdayMean = logic.CalculateWeekDayMeanTime(nights.NightList);
            TimeSpan weekendMean = logic.CalculateWeekEndMeanTime(nights.NightList);//eitoimi

            RunOnUiThread(() => txtOverallMean.Text = $"{overallMean.Hours}h {overallMean.Minutes}min");
            RunOnUiThread(() => txtWeekdayMean.Text = $"{weekdayMean.Hours}h {weekdayMean.Minutes}min");
            RunOnUiThread(() => txtWeekendMean.Text = $"{weekendMean.Hours}h {weekendMean.Minutes}min");
        }

        private void BindPercentages()
        {
            decimal LastWeek = logic.CalculatePercentageOverEightHours(nights.NightList, true);
            decimal AllTime = logic.CalculatePercentageOverEightHours(nights.NightList, false);

            RunOnUiThread(() => txtPercentageLastWeek.Text = $"{LastWeek}%");
            RunOnUiThread(() => txtPercentageAllTime.Text = $"{AllTime}%");
        }

        private void BindBest()
        {
            DayOfWeek bestDay = logic.GetBestDay(nights.NightList).Item1;
            TimeSpan bestMean = logic.GetBestDay(nights.NightList).Item2;

            RunOnUiThread(() => txtBestNight.Text = $"{Enum.GetName(typeof(DayOfWeek), bestDay)} mean: {bestMean.Hours}h {bestMean.Minutes}min");
        }

        private void BindWorst()
        {
            DayOfWeek worstDay = logic.GetWorstDay(nights.NightList).Item1;
            TimeSpan worstMean = logic.GetWorstDay(nights.NightList).Item2;

            RunOnUiThread(() => txtBestNight.Text = $"{Enum.GetName(typeof(DayOfWeek), worstDay)} mean: {worstMean.Hours}h {worstMean.Minutes}min");
        }
    }
}