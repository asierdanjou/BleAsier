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
    using System.Threading.Tasks;
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
                /*
                btnGetServices.IsEnabled = false;
                btnGetCharacteristics.IsEnabled = false;
                dimSlider.IsEnabled = false;
                btnReadCharacteristics.IsEnabled = false;
                btnWriteOffCharacteristics.IsEnabled = false;
                btnWriteOnCharacteristics.IsEnabled = false;
                btnDisconnect.IsEnabled = false;
                */
            };

            dimSlider.PropertyChanged += (s, e) =>
            {
                Debug.WriteLine($"SLIDER: {dimSlider.Value}");
            };
        }

        private void BtnStatus_Clicked(object sender, EventArgs e)
        {
            var state = ble.State;
            //this.DisplayAlert("State:", state.ToString(), "Ok");
            Debug.WriteLine($"BLE STATUS: {state.ToString()}");
            if (state == BluetoothState.On)
            {
                txtBle.Text = "Bluetooth is ON\r\nNumber of connected devices: " + adapter.ConnectedDevices.Count.ToString();
            }
            if (state == BluetoothState.Off)
            {
                //txtBle.BackgroundColor = Color.Red;
                //txtBle.TextColor = Color.White;
                txtBle.Text = "Bluetooth is OFF";
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

            if (device != null)
            {
                btnConnect.IsEnabled = true;
                //btnKnowConnect.IsEnabled = true;
                //entryGUID.Text = device.Id.ToString();
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
            if (device != null)
            {
                try
                {
                    await adapter.ConnectToDeviceAsync(device);
                    //await _adapter.ConnectToKnownDeviceAsync(guid, cancellationToken);
                    //await DisplayAlert("Connected", "Status:" + device.State, "Accept");
                    txtBle.Text = device.State.ToString() + " (" + device.Name.ToString() + ") : " + device.Id;
                    btnGetServices.IsEnabled = true;
                    btnGetCharacteristics.IsEnabled = true;
                    dimSlider.IsEnabled = true;
                    btnReadCharacteristics.IsEnabled = true;
                    btnWriteOffCharacteristics.IsEnabled = true;
                    btnWriteOnCharacteristics.IsEnabled = true;
                    btnDisconnect.IsEnabled = true;
                }
                catch (DeviceConnectionException ex)
                {
                    // ... could not connect to device
                    await DisplayAlert("Error", ex.Message.ToString(), "Accept");
                }
            }
            else
            {
                await DisplayAlert("Warning", "No device selected !", "Accept");
            }
        }

        private async Task WaitAndExecute(int milisec, Action actionToExecute)
        {
            await Task.Delay(milisec);
            actionToExecute();
        }

        private async void BtnKnowConnect_Clicked(object sender, EventArgs e)
        {
            TimeSpan scantime;
            DateTime datetime = DateTime.Now;
            //Guid myGuid = new Guid("00000000-0000-0000-0000-000b57ef2d90"); // Public Address Silicon Labs peripheral

            if (string.IsNullOrEmpty(entryGUID.Text))
            {
                return;
            }
            Guid myGuid = Guid.Parse(entryGUID.Text);
            try
            {
                device = await adapter.ConnectToKnownDeviceAsync(myGuid);
                //scantime = DateTime.Now - datetime;
                //txtBle.Text = adapter.ConnectedDevices.Count.ToString() + " (" + scantime.ToString() + ")";
                txtBle.Text = device.State.ToString() + " (" + device.Name.ToString() + ") : " + device.Id;
                btnGetServices.IsEnabled = true;
                btnGetCharacteristics.IsEnabled = true;
                dimSlider.IsEnabled = true;
                btnReadCharacteristics.IsEnabled = true;
                btnWriteOffCharacteristics.IsEnabled = true;
                btnWriteOnCharacteristics.IsEnabled = true;
                btnDisconnect.IsEnabled = true;
                //await this.WaitAndExecute(2000, ()=>DisplayAlert("Alert", "This fired after 2 seconds", "Ok"));
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
                if (cdevice != null)
                {
                    try
                    {
                        await adapter.DisconnectDeviceAsync(cdevice);
                        txtBle.Text = cdevice.State.ToString();
                        if (adapter.ConnectedDevices.Count == 0)
                        {
                            btnGetServices.IsEnabled = false;
                            btnGetCharacteristics.IsEnabled = false;
                            dimSlider.IsEnabled = false;
                            btnReadCharacteristics.IsEnabled = false;
                            btnWriteOffCharacteristics.IsEnabled = false;
                            btnWriteOnCharacteristics.IsEnabled = false;
                            btnDisconnect.IsEnabled = false;
                        }
                    }
                    catch (DeviceConnectionException ex)
                    {
                        // ... could not disconnect
                        await DisplayAlert("Error", ex.Message.ToString(), "Accept");
                    }
                }
                else
                {
                    await DisplayAlert("Warning", "No devices conected !", "Accept");
                }
            }
        }

        IList<IService> Services;
        IService Service;
        /// Get list of services
        private async void BtnGetServices_Clicked(object sender, EventArgs e)
        {
            var builder = new StringBuilder();
            //Guid myServiceUuid = new Guid("166e3275-1b3a-465c-b0e1-3cbc24c5acbe"); // UUID of a known service (Dimming Value Service)
            try
            {
                Service = await device.GetServiceAsync(myServiceUuid);
            }
            catch { return; }
            //txtBle.Text = Service.Id.ToString();
            //txtBle.Text = Service.Name.ToString();

            Services = await device.GetServicesAsync(); // Get all services of the device
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
            builder.Append("FIN CADENA\r\n");
            txtBle.Text = builder.ToString();
        }

        IList<ICharacteristic> Characteristics;
        ICharacteristic Characteristic;
        /// Get Characteristics
        private async void BtnGetcharacters_Clicked(object sender, EventArgs e)
        {
            var builder = new StringBuilder();
            //Guid myServiceUuid = new Guid("166e3275-1b3a-465c-b0e1-3cbc24c5acbe"); // UUID of a known service (Dimming Service)
            try
            {
                Service = await device.GetServiceAsync(myServiceUuid);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Alert", ex.Message, "Ok");
                return;
            }
            //Guid myCharacteristicUuid = Guid.Parse("fe68847f-59ec-4429-9701-74423a4a7ad4"); // UUID of a known characteristic (Dimming Value)
            try
            {
                Characteristic = await Service.GetCharacteristicAsync(myCharacteristicUuid);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Alert", ex.Message, "Ok");
                return;
            }
            Characteristics = await Service.GetCharacteristicsAsync();              // Get all Characteristics of the Service
            foreach (var item in Characteristics)
            {
                builder.Append(item.Name.ToString() + ":\r\n " + item.Id.ToString() + "\r\n");
            }
            builder.Append("FIN CADENA\r\n");
            txtBle.Text = builder.ToString();
        }
        /*
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
        */
        byte[] data = { };

        private async void BtnGetR_Clicked(object sender, EventArgs e)
        {
            //Guid myServiceUuid = new Guid("166e3275-1b3a-465c-b0e1-3cbc24c5acbe"); // UUID of a known service (Dimming Service)
            try { Service = await device.GetServiceAsync(myServiceUuid); }
            catch { return; }
            //Guid myCharacteristicUuid = Guid.Parse("fe68847f-59ec-4429-9701-74423a4a7ad4"); // UUID of a known characteristic (Dimming Value)
            try { Characteristic = await Service.GetCharacteristicAsync(myCharacteristicUuid); }
            catch { return; }

            if (Characteristic.CanRead)
            {
                try
                {
                    data = await Characteristic.ReadAsync();
                }
                catch (InvalidOperationException ex)
                {
                    Debug.WriteLine("InvalidOperationException reading the characteristics value: " + ex.Message);
                }
                catch (CharacteristicReadException ex)
                {
                    Debug.WriteLine("CharacteristicReadException reading the characteristics value: " + ex.Message);
                }
                catch (ArgumentNullException ex)
                {
                    Debug.WriteLine("Excpetion in reading the characteristic " + Characteristic.Id);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                txtBle.Text = data[0].ToString();
                Debug.WriteLine($"CHARACTERISTIC READ: {data}");
            }
            else
            {
                Debug.WriteLine($"CAN'T READ CHARACTERISTIC");
            }
        }

        private async void BtnWOff_Clicked(object sender, EventArgs e)
        {
            //Guid myServiceUuid = new Guid("166e3275-1b3a-465c-b0e1-3cbc24c5acbe"); // UUID of a known service (Dimming Service)
            try { Service = await device.GetServiceAsync(myServiceUuid); }
            catch { return; }
            //Guid myCharacteristicUuid = Guid.Parse("fe68847f-59ec-4429-9701-74423a4a7ad4"); // UUID of a known characteristic (Dimming Value)
            try { Characteristic = await Service.GetCharacteristicAsync(myCharacteristicUuid); }
            catch { return; }

            if (Characteristic.CanWrite)
            {
                var result = await Characteristic.WriteAsync(new byte[] { 0 });
                if (!result)
                {
                    Debug.WriteLine("CAN'T WRITE TO CHARACTERISTIC");
                }
                else
                {
                    Debug.WriteLine("CHARACTERISTIC WRITTEN");
                }
            }
        }

        private async void BtnWOn_Clicked(object sender, EventArgs e)
        {
            //Guid myServiceUuid = new Guid("166e3275-1b3a-465c-b0e1-3cbc24c5acbe"); // UUID of a known service (Dimming Service)
            try
            {
                Service = await device.GetServiceAsync(myServiceUuid);
            }
            catch (Exception ex)
            {
                return;
            }
            //Guid myCharacteristicUuid = Guid.Parse("fe68847f-59ec-4429-9701-74423a4a7ad4"); // UUID of a known characteristic (Dimming Value)
            try
            {
                Characteristic = await Service.GetCharacteristicAsync(myCharacteristicUuid);
            }
            catch
            {
                return;
            }
            if (Characteristic.CanWrite)
            {
                var result = await Characteristic.WriteAsync(new byte[] { 255 });
                if (!result)
                {
                    Debug.WriteLine("CAN'T WRITE TO CHARACTERISTIC");
                }
                else
                {
                    Debug.WriteLine("CHARACTERISTIC WRITTEN");
                }
            }
        }
        /*
        private async void btnUpdate_Clicked(object sender, EventArgs e)
        {
            Characteristic.ValueUpdated += (o, args) =>
            {
                var bytes = args.Characteristic.Value;
            };
            await Characteristic.StartUpdatesAsync();
        }
        */
        /*
        private void txtErrorBle_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }
        */
        Guid myServiceUuid = new Guid("166e3275-1b3a-465c-b0e1-3cbc24c5acbe"); // UUID of a known service (Dimming Service)
        Guid myCharacteristicUuid = Guid.Parse("fe68847f-59ec-4429-9701-74423a4a7ad4"); // UUID of a known characteristic (Dimming Value)

        private async void OnSliderChanged(object sender, ValueChangedEventArgs args)
        {
            if (Service == null)
            {
                try { Service = await device.GetServiceAsync(myServiceUuid); }
                catch { return; }
            }
            if (Characteristic == null)
            {
                try { Characteristic = await Service.GetCharacteristicAsync(myCharacteristicUuid); }
                catch { return; }
            }
            double value = args.NewValue;
            //string valor = String.Format("{0:F0}", value);
            int iValue = (int)Math.Round(value);
            //int iValue = Convert.ToByte(value);
            sLabel.Text = iValue.ToString();
            byte[] sliderValue = BitConverter.GetBytes(iValue);
            if (Characteristic.CanWrite)
            {
                try
                {
                    var res = await Characteristic.WriteAsync(sliderValue);
                }
                catch (Exception ex)
                {
                    //await DisplayAlert("Alert", ex.Message ,"Ok");
                }
            }
            //sLabel.Text = BitConverter.ToString(valorb);
            //var characteristic = await GetCharacteristicById(parameters, Guid.Parse("00002A87-0000-1000-8000-00805f9b34fb"));
            //byte[] array = Encoding.UTF8.GetBytes(UserDataMock.Email);
            //characteristic.WriteAsync(array);
        }
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

/*
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Double and byte arrays conversion sample.");
        // Create double to a byte array
        double d = 12.09;
        Console.WriteLine("Double value: " + d.ToString());
        byte[] bytes = ConvertDoubleToByteArray(d);
        Console.WriteLine("Byte array value:");
        Console.WriteLine(BitConverter.ToString(bytes));

        Console.WriteLine("Byte array back to double:");
        // Create byte array to double
        double dValue = ConvertByteArrayToDouble(bytes);
        Console.WriteLine(dValue.ToString());
        Console.ReadLine();
    }

    public static byte[] ConvertDoubleToByteArray(double d)
    {
        return BitConverter.GetBytes(d);
    }

    public static double ConvertByteArrayToDouble(byte[] b)
    {
        return BitConverter.ToDouble(b, 0);
    }

}
*/