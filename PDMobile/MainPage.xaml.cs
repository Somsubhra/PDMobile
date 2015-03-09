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

namespace PDMobile
{
    public partial class MainPage : PhoneApplicationPage
    {
        private Accelerometer accelerometer;

        private StorageFolder storageFolder;

        public MainPage()
        {
            InitializeComponent();

            if (!Accelerometer.IsSupported)
            {
                MessageBox.Show("Accelerometer has not been found on phone. So we can not collect the required data.");
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
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
        }

        private void UpdateUI(AccelerometerReading reading)
        {
            AccXLabel.Text = reading.Acceleration.X.ToString();
            AccYLabel.Text = reading.Acceleration.Y.ToString();
            AccZLabel.Text = reading.Acceleration.Z.ToString();
        }
    }
}