using MbientLab.MetaWear.Core;
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using static MbientLab.MetaWear.Functions;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MbientLab.MetaWear.Template {
    public sealed class MacAddressHexString : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            string hexString = ((ulong)value).ToString("X");
            return hexString.Insert(2, ":").Insert(5, ":").Insert(8, ":").Insert(11, ":").Insert(14, ":");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }

    public sealed class ConnectionStateColor : IValueConverter {
        public SolidColorBrush ConnectedColor { get; set; }
        public SolidColorBrush DisconnectedColor { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language) {
            switch ((BluetoothConnectionStatus)value) {
                case BluetoothConnectionStatus.Connected:
                    return ConnectedColor;
                case BluetoothConnectionStatus.Disconnected:
                    return DisconnectedColor;
                default:
                    throw new MissingMemberException("Unrecognized connection status: " + value.ToString());
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }

    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            refreshDevices_Click(null, null);
        }

        /// <summary>
        /// Callback for the refresh button which populates the devices list
        /// </summary>
        private async void refreshDevices_Click(object sender, RoutedEventArgs e) {
			/* foreach (BluetoothLEDevice device in pairedDevices.Items)
			{
				mbl_debug
			} */
            pairedDevices.Items.Clear();
            foreach (var info in await DeviceInformation.FindAllAsync(BluetoothLEDevice.GetDeviceSelector())) {
                pairedDevices.Items.Add(await BluetoothLEDevice.FromIdAsync(info.Id));
            }
        }
        /// <summary>
        /// Callback for the devices list which navigates to the <see cref="DeviceSetup"/> page with the selected device
        /// </summary>
		
        private async void pairedDevices_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selectedDevice = ((ListView)sender).SelectedItem as BluetoothLEDevice;
			

			/*
            if (selectedDevice != null) {
                initFlyout.ShowAt(pairedDevices);
                var board = MetaWearBoard.getMetaWearBoardInstance(selectedDevice);
                var initResult = await board.Initialize();

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, async () => {
                    initFlyout.Hide();

                    if (initResult == Status.ERROR_TIMEOUT) {
                        await new ContentDialog() {
                            Title = "Error",
                            Content = "API initialization timed out.  Try re-pairing the MetaWear or moving it closer to the host device",
                            PrimaryButtonText = "OK"
                        }.ShowAsync();
                    } else {
						BluetoothLEDevice[] devices = { selectedDevice };
                        this.Frame.Navigate(typeof(DeviceSetup), devices);
                    }
                });
            }
			*/
        }
		private async void continue_Click(object sender, RoutedEventArgs e)
		{
			var devices = pairedDevices.SelectedItems;

			// Unsupported number of devices
			if (devices.Count == 0 || devices.Count > 2)
			{
				await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
				CoreDispatcherPriority.Normal, async () =>
				{
						await new ContentDialog()
						{
							Title = "Invalid Number of Devices",
							Content = "Please select 1 or 2 sensors from the list of connected sensors to proceed.",
							PrimaryButtonText = "OK"
						}.ShowAsync();
				});
				return;
			}

			BluetoothLEDevice[] initDevices = new BluetoothLEDevice[devices.Count];

			Debug.WriteLine("helo");
			Debug.WriteLine(devices.Count);

			var i = 1;

			foreach (BluetoothLEDevice selectedDevice in devices)
			{
				initFlyout.ShowAt(pairedDevices);
				var board = MetaWearBoard.getMetaWearBoardInstance(selectedDevice);
				var initResult = await board.Initialize();

				await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
				CoreDispatcherPriority.Normal, async () =>
				{
					initFlyout.Hide();

					if (initResult == Status.ERROR_TIMEOUT)
					{
						//IntPtr cppBoard = board.cppBoard;
						//mbl_mw_metawearboard_tear_down(cppBoard);
						//mbl_mw_debug_reset(cppBoard);

						await new ContentDialog()
						{
							Title = "Error",
							Content = "API initialization timed out.  Try re-pairing the MetaWear or moving it closer to the host device",
							PrimaryButtonText = "OK"
						}.ShowAsync();
					}
					else
					{
						initDevices[i-1] = selectedDevice;
						if (i == devices.Count) // if this was the last board that needed to be initialized, proceed
						{
							this.Frame.Navigate(typeof(DeviceSetup), initDevices);
						}
					}
				});

				i += 1;
			}

		}
    }
}
