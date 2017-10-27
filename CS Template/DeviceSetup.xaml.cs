using MbientLab.MetaWear.Peripheral;
using static MbientLab.MetaWear.Functions;
using MbientLab.MetaWear.Core;
using System;
using System.Collections.Generic;
using System.IO;
//NEW
//using System.Numerics;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using MbientLab.MetaWear.Sensor;
using WindowsQuaternion = System.Numerics.Quaternion;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MbientLab.MetaWear.Template
{
    /// <summary>
    /// Blank page where users add their MetaWear commands
    /// </summary>
    public sealed partial class DeviceSetup : Page
    {
        /// <summary>
        /// Pointer representing the MblMwMetaWearBoard struct created by the C++ API
        /// </summary>
        private IntPtr cppBoard;
        bool startNext = true;
        private Fn_IntPtr gravityDataHandler;
        private Fn_IntPtr quaternionDataHandler;
        List<string> data = new List<string>();
		string dataString = "";
        bool isRunning = true;
        string LiveData = "";
        Quaternion centerQuat;
		bool centered = false;
		bool shouldCenter = false;
		int freq = 0;

		//List<List<float>> saveData = new List<List<float>>();

		public DeviceSetup()
        {
            this.InitializeComponent();

			InitTimer();

            gravityDataHandler = new Fn_IntPtr(GravityDataPtr =>
            {
                Data marshalledData = Marshal.PtrToStructure<Data>(GravityDataPtr);
                string text = "GRAV: " + DateTime.Now + " " + Marshal.PtrToStructure<CartesianFloat>(marshalledData.value);
                //System.Diagnostics.Debug.WriteLine("GRAV: " + DateTime.Now + " " + Marshal.PtrToStructure<CartesianFloat>(marshalledData.value));
                setText(Marshal.PtrToStructure<CartesianFloat>(marshalledData.value).ToString());
            });

            quaternionDataHandler = new Fn_IntPtr(QuaternionDataPtr =>
            {
                Data marshalledData = Marshal.PtrToStructure<Data>(QuaternionDataPtr);
                Quaternion quat = Marshal.PtrToStructure<Quaternion>(marshalledData.value);
				string fullText = "Q: " + DateTime.Now + " " + quat;
				System.Diagnostics.Debug.WriteLine(fullText);
				addPoint(fullText);
				freq += 1;

				Quaternion finalQuat;

                if (shouldCenter)
                {
                    centerQuat = quat;
                    shouldCenter = false;
                    centered = true;
                }

                if (centered)
                {
					finalQuat = centerData(centerQuat, quat);
                    System.Diagnostics.Debug.WriteLine(finalQuat.w.ToString() + "   " + finalQuat.x.ToString() + "   " + finalQuat.y.ToString() + "   " + finalQuat.z.ToString());
                }
                else
                {
					finalQuat = quat;
                }

                double denom = Math.Sqrt(1 - Math.Pow(finalQuat.w, 2));

                if (denom < 0.001)
                {
                    denom = 1;
                }

                double angle = 2 * Math.Acos(finalQuat.w) * (180 / Math.PI);
                double x = finalQuat.x / denom;
                double y = finalQuat.y / denom;
                double z = finalQuat.z / denom;

                string format = "F3";
                string text = "angle: " + angle.ToString(format) + "   x: " + x.ToString(format) + "   y: " + y.ToString(format) + "   z: " + z.ToString(format);
				//System.Diagnostics.Debug.WriteLine("Axis angle: " + DateTime.Now + text);
				setText(text);
			});
    }

		void addPoint(string s)
		{
			if (isRunning)
			{
				data.Add(s);
				dataString = dataString + s + "\r\n";
			}
		}

		private DispatcherTimer timer1;
		public void InitTimer()
		{
			timer1 = new DispatcherTimer();
			timer1.Tick += new EventHandler<object>(displaySampleFreq);
			timer1.Interval = new TimeSpan(0,0,1);
			timer1.Start();
		}

		private void displaySampleFreq(object sender, object e)
		{
			FrequencyTextBlock.Text = freq + " Hz";
			freq = 0;
		}
	

        void setText(String s)
        {
            Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        DataTextBlock.Text = s;
                    }
                    );
        }

        // q1 is center quaternion and q2 is the quaternion to be centered
        Quaternion centerData(Quaternion q1, Quaternion q2)
        {
            WindowsQuaternion q1w = new WindowsQuaternion(q1.w, q1.x, q1.y, q1.z);
            WindowsQuaternion q2w = new WindowsQuaternion(q2.w, q2.x, q2.y, q2.z);

            System.Diagnostics.Debug.WriteLine(q2.x);

            WindowsQuaternion conj = WindowsQuaternion.Conjugate(q1w);
            WindowsQuaternion center = WindowsQuaternion.Multiply(conj, q2w);

            return convertToQuaternion(center);
        }

		Quaternion convertToQuaternion(WindowsQuaternion wq)
		{
			Quaternion quat = new Quaternion();
			quat.w = wq.W;
			quat.x = wq.X;
			quat.y = wq.Y;
			quat.z = wq.Z;

			return quat;
		}

//		float centeredAngle(Quaternion q1)
//		{
//			WindowsQuaternion qw = new WindowsQuaternion(q.w, q.x, q.y, q.z);
//
//			float angle = Math.Acos(w)
//		}


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var mwBoard = MetaWearBoard.getMetaWearBoardInstance(e.Parameter as BluetoothLEDevice);
            cppBoard = mwBoard.cppBoard;

            // cppBoard is initialized at this point and can be used
        }

        /// <summary>
        /// Callback for the back button which tears down the board and navigates back to the <see cref="MainPage"/> page
        /// </summary>
        private void back_Click(object sender, RoutedEventArgs e)
        {
            mbl_mw_metawearboard_tear_down(cppBoard);

            this.Frame.Navigate(typeof(MainPage));
        }

        private Fn_IntPtr accDataHandler = new Fn_IntPtr(pointer =>
        {
            Data data = Marshal.PtrToStructure<Data>(pointer);
            System.Diagnostics.Debug.WriteLine("A: " + DateTime.Now + " " + Marshal.PtrToStructure<CartesianFloat>(data.value));
        });

        private Fn_IntPtr magDataHandler = new Fn_IntPtr(MagDataPtr =>
        {
            Data marshalledData = Marshal.PtrToStructure<Data>(MagDataPtr);
            System.Diagnostics.Debug.WriteLine("M: " + DateTime.Now + " " + Marshal.PtrToStructure<CartesianFloat>(marshalledData.value));
        });

        private Fn_IntPtr gyroDataHandler = new Fn_IntPtr(GyroDataPtr =>
        {
            Data marshalledData = Marshal.PtrToStructure<Data>(GyroDataPtr);
            System.Diagnostics.Debug.WriteLine("G: " + DateTime.Now + " " + Marshal.PtrToStructure<CartesianFloat>(marshalledData.value));
        });

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			saveData();
		}

		private void Clear_Click(object sender, RoutedEventArgs e)
		{
			data = new List<String>();
			dataString = "";

			Clear.Background = new SolidColorBrush(Windows.UI.Colors.Gray);
		}

        private void Center_Click(object sender, RoutedEventArgs e)
        {
            shouldCenter = true;
        }

        private void Stamp_Click(object sender, RoutedEventArgs e)
        {

            if (startNext == true)
            {
				String text = "START " + DateTime.Now + " " + printInput.Text;

				System.Diagnostics.Debug.WriteLine(text);
				addPoint(text);
                startNext = false;
                stamp.Background = new SolidColorBrush(Windows.UI.Colors.MediumPurple);
                stamp.Content = "Print 'STOP' +\n";

            }
            else
            {
				String text = "STOP " + DateTime.Now + " " + printInput.Text;
				System.Diagnostics.Debug.WriteLine(text);
				addPoint(text);
				startNext = true;
                stamp.Background = new SolidColorBrush(Windows.UI.Colors.CornflowerBlue);
                stamp.Content = "Print 'START' +\n";

            }

        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
			data = new List<String>();
			dataString = "";
			Clear.Background = new SolidColorBrush(Windows.UI.Colors.Red);

            isRunning = true;

            quatStart.Background = new SolidColorBrush(Windows.UI.Colors.Gray);
            quatStop.Background = new SolidColorBrush(Windows.UI.Colors.Red);

            if (gravityCheckBox.IsChecked == true)
            {
                mbl_mw_settings_set_connection_parameters(cppBoard, 7.5F, 7.5F, 0, 6000);
                mbl_mw_sensor_fusion_set_mode(cppBoard, SensorFusion.Mode.NDOF);
                mbl_mw_sensor_fusion_set_acc_range(cppBoard, SensorFusion.AccRange.AR_8G); ///AR_2G, 4, 8, 16
                mbl_mw_sensor_fusion_set_gyro_range(cppBoard, SensorFusion.GyroRange.GR_2000DPS); ///GR_2000DPS, 1000, 500, 250

                mbl_mw_sensor_fusion_write_config(cppBoard);

                IntPtr gravityDataSignal = mbl_mw_sensor_fusion_get_data_signal(cppBoard, SensorFusion.Data.QUATERION); //this line works

                mbl_mw_datasignal_subscribe(gravityDataSignal, gravityDataHandler);
                mbl_mw_sensor_fusion_enable_data(cppBoard, SensorFusion.Data.QUATERION);
                mbl_mw_sensor_fusion_start(cppBoard);

            }
            else if (quaternionCheckBox.IsChecked == true)
            {
                mbl_mw_settings_set_connection_parameters(cppBoard, 7.5F, 7.5F, 0, 6000);
                mbl_mw_sensor_fusion_set_mode(cppBoard, SensorFusion.Mode.NDOF);
                mbl_mw_sensor_fusion_set_acc_range(cppBoard, SensorFusion.AccRange.AR_16G); ///AR_2G, 4, 8, 16
                mbl_mw_sensor_fusion_set_gyro_range(cppBoard, SensorFusion.GyroRange.GR_2000DPS); ///GR_2000DPS, 1000, 500, 250

                mbl_mw_sensor_fusion_write_config(cppBoard);

                IntPtr quaternionDataSignal = mbl_mw_sensor_fusion_get_data_signal(cppBoard, SensorFusion.Data.QUATERION); //this line works

                mbl_mw_datasignal_subscribe(quaternionDataSignal, quaternionDataHandler);
                mbl_mw_sensor_fusion_enable_data(cppBoard, SensorFusion.Data.QUATERION);
                mbl_mw_sensor_fusion_start(cppBoard);
            }

            if (gyroscopeCheckBox.IsChecked == true)
            {
                IntPtr gyroSignal = mbl_mw_gyro_bmi160_get_rotation_data_signal(cppBoard);

                mbl_mw_datasignal_subscribe(gyroSignal, gyroDataHandler);
                mbl_mw_gyro_bmi160_enable_rotation_sampling(cppBoard);
                mbl_mw_gyro_bmi160_start(cppBoard);
            }

            if (magnetometerCheckBox.IsChecked == true)
            {
                IntPtr magSignal = mbl_mw_mag_bmm150_get_b_field_data_signal(cppBoard);

                mbl_mw_datasignal_subscribe(magSignal, magDataHandler);
                mbl_mw_mag_bmm150_enable_b_field_sampling(cppBoard);
                mbl_mw_mag_bmm150_start(cppBoard);
            }

            if (accelerometerCheckBox.IsChecked == true)
            {
                IntPtr accSignal = mbl_mw_acc_get_acceleration_data_signal(cppBoard);

                mbl_mw_datasignal_subscribe(accSignal, accDataHandler);
                mbl_mw_acc_enable_acceleration_sampling(cppBoard);
                mbl_mw_acc_start(cppBoard);
            }

        }

        private void printData()
        {
            foreach(var item in data)
            {
                System.Diagnostics.Debug.WriteLine(item);
            }
        }

		private string listToString(List<string> list)
		{
			string catString = "";
			foreach(var s in list)
			{
				catString = catString + s + "\r\n";
			}

			return catString;
		}

		private async Task saveData()
		{

			var savePicker = new Windows.Storage.Pickers.FileSavePicker();
			// Default start location
			//savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
			// Dropdown of file types the user can save the file as
			savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
			// Default file name if the user does not type one in or select a file to replace
			savePicker.SuggestedFileName = "New Document";

			Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
			if (file != null)
			{
				// Prevent updates to the remote version of the file until
				// we finish making changes and call CompleteUpdatesAsync.
				Windows.Storage.CachedFileManager.DeferUpdates(file);
				// write to file
				await Windows.Storage.FileIO.WriteTextAsync(file, dataString);
				// Let Windows know that we're finished changing the file so
				// the other app can update the remote version of the file.
				// Completing updates may require Windows to ask for user input.
				Windows.Storage.Provider.FileUpdateStatus status =
					await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
				if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
				{
					//this.textBlock.Text = "File " + file.Name + " was saved.";
				}
				else
				{
					//this.textBlock.Text = "File " + file.Name + " couldn't be saved.";
				}
			}
			else
			{
				//this.textBlock.Text = "Operation cancelled.";
			}
		}

        private void Stop_Click(object sender, RoutedEventArgs e)
        {

            if (quaternionCheckBox.IsChecked == true)
            {
                System.Diagnostics.Debug.WriteLine("Quaternion stop stuff!");
                IntPtr quatSignal = mbl_mw_sensor_fusion_get_data_signal(cppBoard, SensorFusion.Data.QUATERION);

                mbl_mw_sensor_fusion_stop(cppBoard);
                mbl_mw_sensor_fusion_clear_enabled_mask(cppBoard);
                mbl_mw_datasignal_unsubscribe(quatSignal);
            }
            else if (gravityCheckBox.IsChecked == true)
            {
                System.Diagnostics.Debug.WriteLine("Quaternion stop stuff!");
                IntPtr gravitySignal = mbl_mw_sensor_fusion_get_data_signal(cppBoard, SensorFusion.Data.GRAVITY_VECTOR);

                mbl_mw_sensor_fusion_stop(cppBoard);
                mbl_mw_sensor_fusion_clear_enabled_mask(cppBoard);
                mbl_mw_datasignal_unsubscribe(gravitySignal);
            }

            if (accelerometerCheckBox.IsChecked == true)
            {
                IntPtr accSignal = mbl_mw_acc_get_acceleration_data_signal(cppBoard);

                mbl_mw_acc_stop(cppBoard);
                mbl_mw_acc_disable_acceleration_sampling(cppBoard);
                mbl_mw_datasignal_unsubscribe(accSignal);
            }

            if (magnetometerCheckBox.IsChecked == true)
            {
                IntPtr magSignal = mbl_mw_mag_bmm150_get_b_field_data_signal(cppBoard);

                mbl_mw_mag_bmm150_stop(cppBoard);
                mbl_mw_mag_bmm150_disable_b_field_sampling(cppBoard);
                mbl_mw_datasignal_unsubscribe(magSignal);
            }

            if (gyroscopeCheckBox.IsChecked == true)
            {
                IntPtr gyroSignal = mbl_mw_gyro_bmi160_get_rotation_data_signal(cppBoard);

                mbl_mw_gyro_bmi160_stop(cppBoard);
                mbl_mw_gyro_bmi160_disable_rotation_sampling(cppBoard);
                mbl_mw_datasignal_unsubscribe(gyroSignal);
            }

            isRunning = false;
            //printData();
            quatStart.Background = new SolidColorBrush(Windows.UI.Colors.Green);
            quatStop.Background = new SolidColorBrush(Windows.UI.Colors.Gray);

			saveData();


		}
	}
}
