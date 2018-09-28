namespace BleAsier.ViewModels
{
    public class MainViewModel
    {
        #region ViewModels
        public ScanViewModel Scan
        {
            get;
            set;
        }
        #endregion

        #region Constructors
        public MainViewModel()
        {
            this.Scan = new ScanViewModel(); 
        }
        #endregion
    }
}
