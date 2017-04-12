using MbientLab.MetaWear.Peripheral;
using static MbientLab.MetaWear.Functions;
using MbientLab.MetaWear.Core;
using System;
using System.Collections.Generic;
using System.IO;
//NEW
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
        
        public DeviceSetup() {
            this.InitializeComponent();
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

        //private Fn_IntPtr accDataHandler = new Fn_IntPtr(pointer =>
        //{
        //    System.Diagnostics.Debug.WriteLine("This should be working");
        //    Data data = Marshal.PtrToStructure<Data>(pointer);
        //    System.Diagnostics.Debug.WriteLine(Marshal.PtrToStructure<CartesianFloat>(data.value));
        //});

        //NEW
       private Fn_IntPtr fusionAccHandler = new Fn_IntPtr(FusionAccDataPtr =>
       {
       
            Data marshalledData = Marshal.PtrToStructure<Data>(FusionAccDataPtr);
            System.Diagnostics.Debug.WriteLine(DateTime.Now + " Fusion  " + Marshal.PtrToStructure<Quaternion>(marshalledData.value));

           //TextBlock textBlock1 = new TextBlock();
           //textBlock1.Text = string.Format(DateTime.Now + " Fusion  " + Marshal.PtrToStructure<Quaternion>(marshalledData.value));
            //var message = "Fussion " + Marshal.PtrToStructure<Quaternion>(marshalledData.value).ToString();
            // Send(message);
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


        private void accStart_Click(object sender, RoutedEventArgs e)
        {
            ///NEW
            mbl_mw_settings_set_connection_parameters(cppBoard, 2F, 7.5F, 0, 5000);
            mbl_mw_sensor_fusion_set_mode(cppBoard, SensorFusion.Mode.NDOF);
            mbl_mw_sensor_fusion_set_acc_range(cppBoard, SensorFusion.AccRange.AR_4G);

            mbl_mw_sensor_fusion_write_config(cppBoard);

            IntPtr fusionsAccSignal = mbl_mw_sensor_fusion_get_data_signal(cppBoard, SensorFusion.Data.QUATERION); //this line works

            mbl_mw_datasignal_subscribe(fusionsAccSignal, fusionAccHandler);
            mbl_mw_sensor_fusion_enable_data(cppBoard, SensorFusion.Data.QUATERION);
            mbl_mw_sensor_fusion_start(cppBoard);

            
            //ENDNEW


            //IntPtr accSignal = mbl_mw_acc_get_acceleration_data_signal(cppBoard);

            //mbl_mw_datasignal_subscribe(accSignal, accDataHandler);
            //mbl_mw_acc_enable_acceleration_sampling(cppBoard);
            //mbl_mw_acc_start(cppBoard);
        }

        private void accStop_Click(object sender, RoutedEventArgs e)
        {
            ///NEW
            mbl_mw_sensor_fusion_stop(cppBoard);
            ///ENDNEW

            //IntPtr accSignal = mbl_mw_acc_get_acceleration_data_signal(cppBoard);

            //mbl_mw_acc_stop(cppBoard);
            //mbl_mw_acc_disable_acceleration_sampling(cppBoard);
            //mbl_mw_datasignal_unsubscribe(accSignal);
        }
    }
}
