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
    
    public class NightRecentFirst : Comparer<Night>
    {
       
        public override int Compare(Night x, Night y)
        {
            if (x.StartTime.CompareTo(y.StartTime) != 0)
            {
                return x.StartTime.CompareTo(y.StartTime);
            }
            else if (x.EndTime.CompareTo(y.EndTime) != 0)
            {
                return x.EndTime.CompareTo(y.EndTime);
            }
            else return 0;
        }
    }
}