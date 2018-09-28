namespace BleAsier.Views
{
    using Plugin.BLE;
    using Plugin.BLE.Abstractions.Contracts;
    using Plugin.BLE.Abstractions.Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Text;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ScanPage : ContentPage
	{
        IBluetoothLE ble;
        IAdapter adapter;
        ObservableCollection<IDevice> deviceList;
        IDevice device;

		public ScanPage ()
		{
			InitializeComponent ();
            ble = CrossBluetoothLE.Current;
            adapter = CrossBluetoothLE.Current.Adapter;
            deviceList = new ObservableCollection<IDevice>();
            lv.ItemsSource = deviceList;

            // local ble state changed event
            ble.StateChanged += (s, e) =>
            {
                Debug.WriteLine($"THE BLE STATE CHANGES TO {e.NewState}");
            };

            // start device discovering and device discovered event
            adapter.DeviceDiscovered += (s, a) =>
            {
                if (!deviceList.Contains(a.Device))
                {
                    deviceList.Add(a.Device);
                    Debug.WriteLine($"NEW DEVICE FOUND: {a.Device.Name}, {a.Device.Id}, {a.Device.Rssi}, {a.Device.State}");
                }
            };

            // Scan timeout event
            adapter.ScanTimeoutElapsed += (s, e) =>
            {
                //adapter.StopScanningForDevicesAsync();
                btnScan.IsEnabled = true;
                txtBle.Text = "Scan stopped (timeout)";
                Debug.WriteLine("SCAN STOPPED (TIMEOUT)");
            };

            // Connected event
            //adapter.DeviceConnected += async (s, e) =>
            adapter.DeviceConnected += (s, e) =>
            {
                //await adapter.DisconnectDeviceAsync(e.Device);
                Debug.WriteLine($"CONNECTED DEVICE: {e.Device.Name}");
                //lv.SelectedItem = null;
            };

            // Disconnect event
            adapter.DeviceDisconnected += (s, e) =>
            {
                //txtBle.Text = "Disconnected";
                Debug.WriteLine($"DISCONNECTED DEVICE: {e.Device.Name}");
            };

            // Lost connection event
            adapter.DeviceConnectionLost += (s, e) =>
            {
                //txtBle.Text = "Connection lost";
                Debug.WriteLine($"LOST CONNECTION: {e.Device.Name}");
            };

        }

        private void BtnStatus_Clicked(object sender, EventArgs e)
        {
            var state = ble.State;
            //this.DisplayAlert("State:", state.ToString(), "Ok");
            Debug.WriteLine($"BLE STATUS: {state.ToString()}");
            if (state == BluetoothState.On)
            {
                txtBle.Text = "Your Bluetooth is On !!";
            }
            if (state == BluetoothState.Off)
            {
                //txtBle.BackgroundColor = Color.Red;
                //txtBle.TextColor = Color.White;
                txtBle.Text = "Your Bluetooth is Off !!";
            }
        }

        private async void BtnScan_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (!ble.Adapter.IsScanning)
                {
                    txtBle.Text = "Scanning..";
                    btnScan.IsEnabled = false;
                    deviceList.Clear();
                    /*
                    adapter.DeviceDiscovered += (s, a) =>
                    {
                        if (!deviceList.Contains(a.Device))
                        {
                            deviceList.Add(a.Device);
                            Debug.WriteLine($"NEW DEVICE FOUND: {a.Device.Id}, {a.Device.Name}, {a.Device.Rssi}, {a.Device.State}");
                        }
                    };
                    */
                    adapter.ScanTimeout = 5000;
                    adapter.ScanMode = ScanMode.Balanced;
                    await adapter.StartScanningForDevicesAsync();
                    //txtBle.Text = "";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Aviso", ex.Message.ToString(), "Accept");
            }
        }

        private async void DevicesList_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (lv.SelectedItem == null)
            {
                return;
            }
            device = lv.SelectedItem as IDevice;

            //var result = await DisplayAlert("AVISO", "Desea conectarse a ese dispositivo?", "Conectar", "Cancelar");
            //if (!result) return;

            if (!ble.Adapter.IsScanning)
            {
                await adapter.StopScanningForDevicesAsync();
                txtBle.Text = "";
            }
            /*
            try
            {
                await adapter.ConnectToDeviceAsync(device);
                await DisplayAlert("Connected", "Status:" + device.State, "Ok");
            }
            catch (DeviceConnectionException ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }
            */

        }

        private async void BtnConnect_Clicked(object sender, EventArgs a)
        {
            try
            {
                if (device != null)
                {
                    await adapter.ConnectToDeviceAsync(device);
                    //await _adapter.ConnectToKnownDeviceAsync(guid, cancellationToken);
                    //await DisplayAlert("Connected", "Status:" + device.State, "Accept");
                    txtBle.Text = device.State.ToString() + " (" + device.Name.ToString() + ") : " + device.Id;
                }
                else
                {
                    await DisplayAlert("Warning", "No device selected !", "Accept");
                }
            }
            catch (DeviceConnectionException ex)
            {
                // ... could not connect to device
                await DisplayAlert("Error", ex.Message.ToString(), "Accept");
            }
        }

        private async void BtnKnowConnect_Clicked(object sender, EventArgs e)
        {
            TimeSpan scantime;
            DateTime datetime = DateTime.Now;
            Guid myGuid = new Guid("00000000-0000-0000-0000-000b57ef2d90"); // Public Address Silicon Labs peripheral

            try
            {
                await adapter.ConnectToKnownDeviceAsync(myGuid);
                scantime = DateTime.Now - datetime;
                txtBle.Text = adapter.ConnectedDevices.Count.ToString() + " (" + scantime.ToString() + ")";
            }
            catch (DeviceConnectionException ex)
            {
                //Could not connect to the device
                await DisplayAlert("Notice", ex.Message.ToString(), "OK");
            }
        }

        private async void BtnDisconnect_Clicked(object sender, EventArgs a)
        {
            IDevice cdevice;
            if (adapter.ConnectedDevices.Count > 0)
            {
                cdevice = adapter.ConnectedDevices[0];
                try
                {
                    if (cdevice != null)
                    {
                        await adapter.DisconnectDeviceAsync(cdevice);
                    }
                    else
                    {
                        await DisplayAlert("Warning", "No devices conected !", "Accept");
                    }
                }
                catch (DeviceConnectionException ex)
                {
                    // ... could not disconnect
                    await DisplayAlert("Error", ex.Message.ToString(), "Accept");
                }
            }
        }

        IList<IService> Services;
        IService Service;
        /// Get list of services
        private async void BtnGetServices_Clicked(object sender, EventArgs e)
        {
            var builder = new StringBuilder();
            Guid myUuid = new Guid("166e3275-1b3a-465c-b0e1-3cbc24c5acbe"); // Public Address Silicon Labs peripheral
            Services = await device.GetServicesAsync();
            // Service = await device.GetServiceAsync(Guid.Parse("guid")); 
            //or we call the Guid of selected Device
            //Service = await device.GetServiceAsync(device.Id);
            Service = await device.GetServiceAsync(myUuid);
            //txtBle.Text = Service.Id.ToString();
            //txtBle.Text = Service.Name.ToString();
            /*
            if (Services.Count > -1)
            {
                txtBle.Text = Services[0].Name.ToString() + " (" + Services[0].Id.ToString() + ")";    // Primer servicio
            }
            */
            /*
            for ( var i = 0; i < Services.Count; i++ )
            {
                //txtBle.Text = Services[i].Name.ToString() + " (" + Services[i].Id.ToString() + ")";
                builder.Append(Services[i].Name.ToString() + ":\r\n " + Services[i].Id.ToString() + "\r\n");
                //await DisplayAlert("Services", Services[i].Name.ToString() + " (" + Services[i].Id.ToString() + ")", "Ok");
            }
            */
            foreach (var item in Services)
            {
                builder.Append(item.Name.ToString() + ":\r\n " + item.Id.ToString() + "\r\n");
            }
            txtBle.Text = builder.ToString();
        }

        IList<ICharacteristic> Characteristics;
        ICharacteristic Characteristic;
        /// Get Characteristics
        private async void BtnGetcharacters_Clicked(object sender, EventArgs e)
        {
            Characteristics = await Service.GetCharacteristicsAsync();
            Guid idGuid = Guid.Parse("guid");
            Characteristic = await Service.GetCharacteristicAsync(idGuid);
            //  Characteristic.CanRead
        }

        IDescriptor descriptor;
        IList<IDescriptor> descriptors;
        private async void btnDescriptors_Clicked(object sender, EventArgs e)
        {
            descriptors = await Characteristic.GetDescriptorsAsync();
            descriptor = await Characteristic.GetDescriptorAsync(Guid.Parse("guid"));
        }

        private async void btnDescRW_Clicked(object sender, EventArgs e)
        {
            var bytes = await descriptor.ReadAsync();
            await descriptor.WriteAsync(bytes);
        }

        private async void btnGetRW_Clicked(object sender, EventArgs e)
        {
            var bytes = await Characteristic.ReadAsync();
            await Characteristic.WriteAsync(bytes);
        }

        private async void btnUpdate_Clicked(object sender, EventArgs e)
        {
            Characteristic.ValueUpdated += (o, args) =>
            {
                var bytes = args.Characteristic.Value;
            };
            await Characteristic.StartUpdatesAsync();
        }
        /*
        private void txtErrorBle_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }
        */
    }
}

//adapter.GetSystemConnectedOrPairedDevices(ServiceGuidsList);

/*
//CONNECT
_bluetoothGatt = _device.ConnectGatt(this, false, new BGattCallback(this));
Plugin.BLE.Android.Device device = new Plugin.BLE.Android.Device(
    (Plugin.BLE.Android.Adapter)Plugin.BLE.CrossBluetoothLE.Current.Adapter, _device, _bluetoothGatt, Rssi);

Plugin.BLE.CrossBluetoothLE.Current.Adapter.GetSystemConnectedOrPairedDevices().FirstOrDefault(x => x.Id == Id);

await Plugin.BLE.CrossBluetoothLE.Current.Adapter.ConnectToDeviceAsync(device);

return device != null && device.State == Plugin.BLE.Abstractions.DeviceState.Connected 
    || device.State == Plugin.BLE.Abstractions.DeviceState.Limited? true : false;

//DISCONNECT
var device = Plugin.BLE.CrossBluetoothLE.Current.Adapter.GetSystemConnectedOrPairedDevices().FirstOrDefault(x => x.Id == Id);
if (device != null)
{
    await Plugin.BLE.CrossBluetoothLE.Current.Adapter.DisconnectDeviceAsync(device);
}
*/
