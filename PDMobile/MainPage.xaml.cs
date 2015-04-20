using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PDMobile.Resources;

using Microsoft.Devices.Sensors;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Diagnostics;

namespace PDMobile
{
    public partial class MainPage : PhoneApplicationPage
    {
        private Accelerometer accelerometer;

        private StorageFile storageFile;

        private ulong seekLocation = 0;

        public MainPage()
        {
            InitializeComponent();

            if (!Accelerometer.IsSupported)
            {
                MessageBox.Show("Accelerometer has not been found on phone. So we can not collect the required data.");
            }

            createAppBar();
        }

        private void createAppBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Mode = ApplicationBarMode.Default;
            ApplicationBar.Opacity = 1.0;
            ApplicationBar.IsVisible = true;
            ApplicationBar.IsMenuEnabled = true;

            ApplicationBarMenuItem syncBtn = new ApplicationBarMenuItem();
            syncBtn.Text = "Send Data";
            ApplicationBar.MenuItems.Add(syncBtn);
            syncBtn.Click += syncBtn_Click;

            ApplicationBarMenuItem aboutBtn = new ApplicationBarMenuItem();
            aboutBtn.Text = "About";
            ApplicationBar.MenuItems.Add(aboutBtn);
            aboutBtn.Click += aboutBtn_Click;
        }

        void aboutBtn_Click(object sender, EventArgs e)
        {
            
        }

        void syncBtn_Click(object sender, EventArgs e)
        {
            
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (accelerometer == null)
            {
                accelerometer = new Accelerometer();
                accelerometer.TimeBetweenUpdates = TimeSpan.FromMilliseconds(100);
                accelerometer.CurrentValueChanged += accelerometer_CurrentValueChanged;
            }

            try
            {
                accelerometer.Start();
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Accelerometer could not be started successfully. Please restart the app.");
            }

            StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(Stopwatch.GetTimestamp().ToString());
            storageFile = await storageFolder.CreateFileAsync("data.csv", CreationCollisionOption.ReplaceExisting);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (accelerometer != null)
            {
                accelerometer.CurrentValueChanged -= accelerometer_CurrentValueChanged;
                accelerometer.Stop();
                accelerometer.Dispose();
            }
        }

        void accelerometer_CurrentValueChanged(object sender, SensorReadingEventArgs<AccelerometerReading> e)
        {
            Dispatcher.BeginInvoke(() => UpdateUI(e.SensorReading));
            Dispatcher.BeginInvoke(() => WriteData(e.SensorReading));
        }

        private void UpdateUI(AccelerometerReading reading)
        {
            AccXLabel.Text = reading.Acceleration.X.ToString();
            AccYLabel.Text = reading.Acceleration.Y.ToString();
            AccZLabel.Text = reading.Acceleration.Z.ToString();
        }

        private async void WriteData(AccelerometerReading reading)
        {
            if (storageFile == null)
            {
                return;
            }

            String x = reading.Acceleration.X.ToString();
            String y = reading.Acceleration.Y.ToString();
            String z = reading.Acceleration.Z.ToString();

            String line = x + ";" + y + ";" + z + "\n";

            try
            {
                using (IRandomAccessStream stream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    IOutputStream outStream = stream.GetOutputStreamAt(seekLocation);

                    using (DataWriter writer = new DataWriter(outStream))
                    {
                        writer.WriteString(line);
                        await writer.StoreAsync();

                        seekLocation += (ulong)System.Text.Encoding.UTF8.GetByteCount(line);
                    }
                }
            }
            catch (Exception) { }
        }

        private void Background_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Background.Visibility = System.Windows.Visibility.Collapsed;
            BackgroundLabel.Visibility = System.Windows.Visibility.Collapsed;
            Content.Visibility = System.Windows.Visibility.Visible;
        }

        private void Content_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Content.Visibility = System.Windows.Visibility.Collapsed;
            Background.Visibility = System.Windows.Visibility.Visible;
            BackgroundLabel.Visibility = System.Windows.Visibility.Visible;
        }
    }
}