using GalaSoft.MvvmLight;

namespace WpfCrowdDetection.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        #region Attributes
        public bool _isShowCameraPreview;
        public bool _isShowDetectionFacePreview;
        public bool _isSendToIoTHub;
        #endregion

        #region Properties
        public bool IsShowCameraPreview
        {
            get { return _isShowCameraPreview; }
            set
            {
                Set(() => IsShowCameraPreview, ref _isShowCameraPreview, value);
            }
        }

        public bool IsShowDetectionFacePreview
        {
            get { return _isShowDetectionFacePreview; }
            set
            {
                Set(() => IsShowDetectionFacePreview, ref _isShowDetectionFacePreview, value);
            }
        }

        public bool IsSendToIoTHub
        {
            get { return _isSendToIoTHub; }
            set
            {
                Set(() => IsSendToIoTHub, ref _isSendToIoTHub, value);
            }
        }
        #endregion

        #region Constructors
        public SettingsViewModel()
        {
            IsShowCameraPreview = true;
            IsShowDetectionFacePreview = true;
        }
        #endregion
    }
}