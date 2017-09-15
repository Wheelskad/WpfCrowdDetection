using GalaSoft.MvvmLight;
using System.Timers;
using System;
using WindowsInput;
using WindowsInput.Native;
using System.Windows.Threading;
using WpfCrowdDetection.Helper;

namespace WpfCrowdDetection.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        #region Attributes
        private bool _isShowCameraPreview;
        private bool _isShowDetectionFacePreview;
        private bool _isSendToIoTHub;
        private bool _isInputSimulationEnable;
        private int _inputSimulationInSeconds;
        private readonly Timer _inputSimulationTimer = new Timer();
        private readonly InputSimulator _inputSimulator = new InputSimulator();

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

        public int InputSimulationInSeconds
        {
            get { return _inputSimulationInSeconds; }
            set
            {
                Set(() => InputSimulationInSeconds, ref _inputSimulationInSeconds, value);
                UpdateInputSimulationTimer();
            }
        }
        #endregion

        #region Constructors
        public SettingsViewModel()
        {
            _inputSimulationTimer.Elapsed += _inputSimulationTimer_Elapsed; ;
            IsShowCameraPreview = true;
            IsShowDetectionFacePreview = true;
            InputSimulationInSeconds = Properties.Settings.Default.InputSimulationInSeconds;
        }

        private void _inputSimulationTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _inputSimulator.Keyboard.KeyDown(VirtualKeyCode.VK_1);
            }
            catch(Exception ex)
            {
                App.Log.Error(ex.FlattenException());
            }
        }

        private void UpdateInputSimulationTimer()
        {
            if (_inputSimulationTimer.Enabled)
            {
                _inputSimulationTimer.Stop();
            }
            if (InputSimulationInSeconds > 0)
            {
                _inputSimulationTimer.Interval = InputSimulationInSeconds * 1000; //Ms
                _inputSimulationTimer.Start();
            }
        }

        public override void Cleanup()
        {
            if (_inputSimulationTimer.Enabled)
            {
                _inputSimulationTimer.Stop();
            }
            _inputSimulationTimer.Elapsed -= _inputSimulationTimer_Elapsed;
            _inputSimulationTimer.Dispose();
        }
        #endregion
    }
}