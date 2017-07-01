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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MbientLab.MetaWear.Template {
    /// <summary>
    /// Blank page where users add their MetaWear commands
    /// </summary>
    public sealed partial class DeviceSetup : Page {
        /// <summary>
        /// Pointer representing the MblMwMetaWearBoard struct created by the C++ API
        /// </summary>
        private IntPtr cppBoard;
        Val val1 = new Val();

        public DeviceSetup() {
            this.InitializeComponent();
            //bindData();
        }


        public class Val
        {
            public string quatString = "";
        }
        
        void bindData()
        {
            orientationText.DataContext = val1.quatString;
        }


        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            var mwBoard= MetaWearBoard.getMetaWearBoardInstance(e.Parameter as BluetoothLEDevice);
            cppBoard = mwBoard.cppBoard;

            // cppBoard is initialized at this point and can be used
        }

        /// <summary>
        /// Callback for the back button which tears down the board and navigates back to the <see cref="MainPage"/> page
        /// </summary>
        private void back_Click(object sender, RoutedEventArgs e) {
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

        private Fn_IntPtr quaternionDataHandler = new Fn_IntPtr(QuaternionDataPtr =>
       {
       
            Data marshalledData = Marshal.PtrToStructure<Data>(QuaternionDataPtr);
            System.Diagnostics.Debug.WriteLine("Q: " + DateTime.Now + " " + Marshal.PtrToStructure<Quaternion>(marshalledData.value));
            //val1.quatString = Marshal.PtrToStructure<Quaternion>(marshalledData.value);
            //sSystem.Diagnostics.Debug.WriteLine(marshalledData);

           //TextBlock textBlock1 = new TextBlock();
           //textBlock1.Text = string.Format(DateTime.Now + " Fusion  " + Marshal.PtrToStructure<Quaternion>(marshalledData.value));
           //var message = "Fussion " + Marshal.PtrToStructure<Quaternion>(marshalledData.value).ToString();
           // Send(message);
       });

        private Fn_IntPtr gravityDataHandler = new Fn_IntPtr(GravityDataPtr =>
        {

            Data marshalledData = Marshal.PtrToStructure<Data>(GravityDataPtr);
            System.Diagnostics.Debug.WriteLine("GRAV: " + DateTime.Now + " " + Marshal.PtrToStructure<CartesianFloat>(marshalledData.value));

        });
        //ENDNEW
        //public static void SetReadingText(TextBlock textBlock, Data marshalledData)
        //{
        //    textBlock.Text = string.Format(DateTime.Now + " Fusion  " + Marshal.PtrToStructure<Quaternion>(marshalledData.value));
        //}

        //async private void ReadingChanged(object sender, AccelerometerReadingChangedEventArgs e)
        //{
        //    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        //    {
        //        SetReadingText(ScenarioOutput, e.Reading);
        //    });
        //}


        private void Start_Click(object sender, RoutedEventArgs e)
        {
            ///NEW
            ///
            if (gravityCheckBox.IsChecked == true)
            {
                mbl_mw_settings_set_connection_parameters(cppBoard, 7.5F, 7.5F, 0, 6000);
                mbl_mw_sensor_fusion_set_mode(cppBoard, SensorFusion.Mode.NDOF);
                mbl_mw_sensor_fusion_set_acc_range(cppBoard, SensorFusion.AccRange.AR_16G); ///AR_2G, 4, 8, 16
                mbl_mw_sensor_fusion_set_gyro_range(cppBoard, SensorFusion.GyroRange.GR_2000DPS); ///GR_2000DPS, 1000, 500, 250

                mbl_mw_sensor_fusion_write_config(cppBoard);

                IntPtr gravityDataSignal = mbl_mw_sensor_fusion_get_data_signal(cppBoard, SensorFusion.Data.GRAVITY_VECTOR); //this line works

                mbl_mw_datasignal_subscribe(gravityDataSignal, gravityDataHandler);
                mbl_mw_sensor_fusion_enable_data(cppBoard, SensorFusion.Data.GRAVITY_VECTOR);
                mbl_mw_sensor_fusion_start(cppBoard);

            }  else if (quaternionCheckBox.IsChecked == true )
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
                IntPtr gyroSignal =  mbl_mw_gyro_bmi160_get_rotation_data_signal(cppBoard);

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

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (quaternionCheckBox.IsChecked == true)
            {
                System.Diagnostics.Debug.WriteLine("Quaternion stop stuff!");
                IntPtr quatSignal = mbl_mw_sensor_fusion_get_data_signal(cppBoard, SensorFusion.Data.QUATERION);

                mbl_mw_sensor_fusion_stop(cppBoard);
                mbl_mw_sensor_fusion_clear_enabled_mask(cppBoard);
                mbl_mw_datasignal_unsubscribe(quatSignal);
            } else if (gravityCheckBox.IsChecked == true)
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


        }
    }
}
