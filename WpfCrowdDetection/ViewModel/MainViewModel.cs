using GalaSoft.MvvmLight;

namespace WpfCrowdDetection.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Properties

        public string StandName
        {
            get
            {
                return Properties.Settings.Default.StandName;
            }
        }

        #endregion Properties
    }
}