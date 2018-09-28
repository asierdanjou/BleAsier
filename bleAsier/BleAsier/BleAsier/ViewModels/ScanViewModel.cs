using Plugin.BLE.Abstractions.Contracts;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BleAsier.ViewModels
{
    public class ScanViewModel : INotifyPropertyChanged
    {
        #region Attributes
        private IDevice nativeDevice;
        #endregion

        #region Properties
        public event PropertyChangedEventHandler PropertyChanged;
        public IDevice NativeDevice
        {
            get
            {
                if (nativeDevice.Name is null)
                {
                    //return "Unknown device";
                }
                return nativeDevice;
            }
            set
            {
                nativeDevice = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        protected void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }
    }
}
