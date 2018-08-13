using Android.App;
using Android.Widget;
using Android.OS;
using Android.Media;
using System;
using Android.Content.PM;
using Java.Lang;
using System.Linq;

namespace App1
{

    [Activity(Label = "Mobil Akord", MainLauncher = true, Icon = "@drawable/icon",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Activity
    {
        int i = 0;
        private static int sampleRate = 8000; // 8000, 11025, 16000, 22050, 44100
        private AudioRecord audio;
        private int bufferSize;
        private float lastLevel = 0;
        private float frequency = 0;
        private bool clicked = false;
        //private Handler mHandler;
        //private Thread thread;
        //private static int SAMPLE_DELAY = 75;
        TextView tv;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main); // for the textview setting text
            tv = FindViewById<TextView>(Resource.Id.text1);
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 500;
            timer.Elapsed += OnTimedEvent;

            try
            {
                bufferSize = AudioRecord
                        .GetMinBufferSize(sampleRate, ChannelIn.Mono,
                                Encoding.Pcm16bit);
            }
            catch (ArgumentException e)
            {
                //android.util.Log.e("TrackingFlow", "Exception", e);
            }

            Button b = FindViewById<Button>(Resource.Id.button1);

            audio = new AudioRecord(AudioSource.Mic, sampleRate,
                ChannelIn.Mono,
                Encoding.Pcm16bit, bufferSize);

            audio.StartRecording();

            b.Click += delegate
            {
                if (!clicked)
                {
                    clicked = true;
                    b.Text = "STOP";
                    timer.Enabled = true;

                }
                else
                {
                    b.Text = "START";
                    clicked = false;
                    timer.Enabled = false;

                }
            };
        }

        protected override void OnResume()
        {
            base.OnResume();

        }
        protected override void OnPause()
        {
            base.OnPause();
            //audio.Stop();
            //audio.Release();
            //audio = null;
        }
        private void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            read();
            //RunOnUiThread(() => tv.Text = i.ToString() + " - " + frequency.ToString() + " - " + findNote(frequency));
            RunOnUiThread(() => tv.Text = findNote(frequency).ToString());
        }
        void read()
        {
            i++;
            try
            {
                short[] buffer = new short[bufferSize];

                int bufferReadResult = 1;

                if (audio != null)
                {

                    bufferReadResult = audio.Read(buffer, 0, bufferSize);

                    float sumLevel = 0;
                    for (int i = 0; i < bufferReadResult; i++)
                    {
                        sumLevel += buffer[i];
                    }

                    if (bufferReadResult > 0)
                    {
                        lastLevel = Java.Lang.Math.Abs((sumLevel / bufferReadResult));

                        float intensity = averageIntensity(buffer, bufferReadResult);

                        frequency = getFreq(buffer, bufferReadResult / 4, bufferReadResult, sampleRate, 50, 500);

                    }
                    else frequency = 0;

                }

                //lastLevel.ToString();
            }
            catch (ArgumentException e)
            {
                //e.printStackTrace();
            }
            //audio.Release();
        }
       
        private float getFreq(short[] data, int windowSize, int frames, int sampleRate, float minFreq, float maxFreq)
        {

            float maxOffset = (float)sampleRate / minFreq;
            float minOffset = (float)sampleRate / maxFreq;

            int minSum = Integer.MaxValue;
            int minSumLag = 0;

            for (int lag = (int)minOffset; lag <= maxOffset; lag++)
            {
                int sum = 0;
                for (int i = 0; i < windowSize; i++)
                {

                    int oldIndex = i - lag;

                    int sample = ((oldIndex < 0) ? data[frames + oldIndex] : data[oldIndex]);

                    sum += Java.Lang.Math.Abs(sample - data[i]);
                }

                if (sum < minSum)
                {
                    minSum = sum;
                    minSumLag = lag;
                }
            }

            return (float)sampleRate / minSumLag;
        }

        private float averageIntensity(short[] buffer, int frames)
        {
            float sum = 0;
            for (int i = 0; i < frames; i++)
            {
                sum += Java.Lang.Math.Abs(buffer[i]);
            }
            return sum / frames;
        }

        private string findNote(float calculating_freq)
        {
            float[] target_freq = new float[6] { 82, 110, 147, 196, 247, 329 }; // E-A-D-G-B-e
            string[] target_notes = new string[6] { "E", "A", "D", "G", "B", "e" };
            float[] diff = new float[6];
            for (int i = 0; i < target_freq.Length; i++)
            {
                diff[i] = Java.Lang.Math.Abs(calculating_freq - target_freq[i]);
            }
            int index = diff.ToList().IndexOf(diff.Min());
            if ((100*diff[index])/target_freq[index] > 50) return "??";
            else return target_notes[index];
        }

    }

}

