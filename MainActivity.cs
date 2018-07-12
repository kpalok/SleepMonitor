using Android.App;
using Android.Widget;
using Android.OS;
using System;
using System.Threading;
using Android.Content;
using Android.Preferences;
using System.Threading.Tasks;
using static SleepMonitor.Classes.DialogDisplayer;

namespace SleepMonitor
{
    [Activity(Label = "SleepMonitor", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private DateTime startTime;
        private DateTime stopTime;
        private TimeSpan elapsedTime;
        private TextView txtTimer;
        private Button btnTimer;
        private Button btnHistory;
        private Timer timer;
        private TimerCallback timerDelegate;
        private bool timerOn;

        private Logic logic = new Logic();

        private Classes.DialogDisplayer dialogDisplayer;

        #region Activity overrides

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);
            txtTimer = FindViewById<TextView>(Resource.Id.txtTimer);
            btnTimer = FindViewById<Button>(Resource.Id.btnTimer);
            btnHistory = FindViewById<Button>(Resource.Id.btnHistory);
            timerDelegate = new TimerCallback(UpdatetxtTimer);
            dialogDisplayer = new Classes.DialogDisplayer(this);
            AddHandlers();
            if (savedInstanceState != null)
            {
                timerOn = savedInstanceState.GetBoolean("timerOn", false);
                startTime = new DateTime(savedInstanceState.GetLong("startTimeTicks"));
                ResumeTimer();
            }
            System.Diagnostics.Debug.WriteLine($"MainActivity - OnCreate {startTime}, {timerOn}");
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutLong("startTimeTicks", startTime.Ticks);
            outState.PutBoolean("timerOn", timerOn);
            System.Diagnostics.Debug.WriteLine($"MainActivity - Saving Instance state {startTime}, {timerOn}");
            base.OnSaveInstanceState(outState);
        }

        protected override void OnPause()
        {
            base.OnPause();

            System.Diagnostics.Debug.WriteLine("MainActivity - OnPause");
            // Storing values between instances
            ISharedPreferences preferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            ISharedPreferencesEditor editor = preferences.Edit();

            editor.PutBoolean("timerOn", timerOn);
            editor.PutLong("startTimeTicks", startTime.Ticks);
            editor.Commit();
        }

        protected override void OnResume()
        {
            base.OnResume();

            ISharedPreferences preferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            timerOn = preferences.GetBoolean("timerOn", false);
            startTime = new DateTime(preferences.GetLong("startTimeTicks", 0));
            System.Diagnostics.Debug.WriteLine($"MainActivity - OnResume {startTime}, {timerOn}");

            if (timerOn) ResumeTimer();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            RemoveHandlers();
            System.Diagnostics.Debug.WriteLine("MainActivity - OnDestroy");
        }

        #endregion

        #region Timer methods

        private void StartTimer()
        {
            startTime = DateTime.Now;
            elapsedTime = new TimeSpan();
            timer = new Timer(timerDelegate, null, 0, 1000);
        }

        private void ResumeTimer()
        {
            timer = new Timer(timerDelegate, null, 0, 1000);
            RunOnUiThread(() => btnTimer.Text = "Stop");
        }

        private void StopTimer()
        {
            stopTime = DateTime.Now;
            timer.Dispose();
            Night night = new Night() { StartTime = startTime, EndTime = stopTime};
            
        }

        private void UpdatetxtTimer(object state)
        {
            elapsedTime = DateTime.Now.Subtract(startTime);
            string elapsedTimeString = String.Format("{0:00}:{1:00}", elapsedTime.Minutes, elapsedTime.Seconds);

            RunOnUiThread(() => txtTimer.Text = elapsedTimeString);
        }

        #endregion

        #region Handlers

        private void ChangeTimerStatus(object sender, EventArgs e)
        {
            if (!timerOn)
            {
                btnTimer.Text = "Stop";
                StartTimer();
                timerOn = true;
            }
            else if (timerOn)
            {
                TimeSpan timeslept = DateTime.Now.Subtract(startTime);
                Task<MessageResult> task =  dialogDisplayer.ShowDialog("Confirmation", $"Time slept {timeslept.Hours}h {timeslept.Minutes}min. Is the time right? Yes - continue, No - adjust.",
                    false, false, MessageResult.YES, MessageResult.NO);

                if (task.Result == MessageResult.OK)
                {
                    btnTimer.Text = "Start";
                    StopTimer();
                    timerOn = false;
                }
                else if (task.Result == MessageResult.NO)
                {

                }
            }
        }

        private void ShowHistory(object sender, EventArgs e)
        {            
            Intent intent = new Intent(this, (new HistoryActivity()).Class);
            StartActivity(intent);
        }

        private void AddHandlers()
        {
            btnTimer.Click += ChangeTimerStatus;
            btnHistory.Click += ShowHistory;
        }

        private void RemoveHandlers()
        {
            btnTimer.Click -= ChangeTimerStatus;
            btnHistory.Click -= ShowHistory;
        }

        #endregion
    }
}

