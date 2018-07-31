using Emgu.CV;
using Emgu.CV.Structure;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfCrowdDetection.Converters;
using WpfCrowdDetection.Helper;
using WpfCrowdDetection.Model;
using WpfCrowdDetection.Services;

namespace WpfCrowdDetection.ViewModel
{
    public class HomeViewModel : ViewModelBase, IDisposable
    {
        #region Attributes

        private const string _configFileDirectory = "Dependencies";
        private const string _faceDetectionConfig = "haarcascade_frontalface_default.xml";
        private const string _eyeDetectionConfig = "haarcascade_eye.xml";
        private readonly string _faceDetectionConfigFilePath;
        private readonly string _eyeDetectionConfigFilePath;

        private readonly ICustomDialogService _dialogService;
        private readonly VideoCaptureManager _videoCaptureManager;
        private readonly IotHubPublisher _iotHubPublisher;

        private BitmapSource _grabImageSource;
        private BitmapSource _faceDetectionImageSource;
        private DetectionInfo _detectionInfo;

        private ICommand _startOrStopCaptureCommand;
        private ICommand _startOrStopFaceDetectionCommand;

        private readonly DispatcherTimer _timerFaceDetectionProcess;

        private enum DetectionModeEnum { OpenCV, Bing };

        #endregion Attributes

        #region Properties

        public BitmapSource GrabImageSource
        {
            get { return _grabImageSource; }
            set
            {
                Set(() => GrabImageSource, ref _grabImageSource, value);
            }
        }

        public BitmapSource FaceDetectionImageSource
        {
            get { return _faceDetectionImageSource; }
            set
            {
                Set(() => FaceDetectionImageSource, ref _faceDetectionImageSource, value);
            }
        }

        public DetectionInfo DetectionInfo
        {
            get { return _detectionInfo; }
            set
            {
                Set(() => DetectionInfo, ref _detectionInfo, value);
            }
        }

        public ICommand StartOrStopCaptureCommand
        {
            get
            {
                if (_startOrStopCaptureCommand == null)
                {
                    _startOrStopCaptureCommand = new RelayCommand(StartOrStopCapture);
                }
                return _startOrStopCaptureCommand;
            }
        }

        public ICommand StartOrStopFaceDetectionCommand
        {
            get
            {
                if (_startOrStopFaceDetectionCommand == null)
                {
                    _startOrStopFaceDetectionCommand = new RelayCommand(StartOrStopFaceDetection);
                }
                return _startOrStopFaceDetectionCommand;
            }
        }

        public string StandName
        {
            get
            {
                return Properties.Settings.Default.StandName;
            }
        }

        //Id dans fichier de config:
        //Id = 0 Caméra intégrée ou caméra USB si connectée
        //Id = 1 Caméra intégrée si caméra USB connectée
        public int CameraId
        {
            get
            {
                return Properties.Settings.Default.CameraId;
            }
        }

        public string DeviceId
        {
            get
            {
                return Properties.Settings.Default.DeviceId;
            }
        }

        public string IotHubHostName
        {
            get
            {
                return Properties.Settings.Default.IotHubHostName;
            }
        }

        public string SharedAccessKey
        {
            get
            {
                return Properties.Settings.Default.SharedAccessKey;
            }
        }

        private DetectionModeEnum DetectionMode { get; set; }

        private SettingsViewModel SettingsViewModel { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public HomeViewModel(ICustomDialogService dialogService, SettingsViewModel settingsViewModel)
        {
            _dialogService = dialogService;
            _videoCaptureManager = new VideoCaptureManager(CameraId);
            _videoCaptureManager.NewImageComplete += videoCaptureManager_OnNewImageComplete;

            _iotHubPublisher = new IotHubPublisher(IotHubHostName, DeviceId, SharedAccessKey);

            _faceDetectionConfigFilePath = Path.Combine(_configFileDirectory, _faceDetectionConfig);
            _eyeDetectionConfigFilePath = Path.Combine(_configFileDirectory, _eyeDetectionConfig);

            _timerFaceDetectionProcess = new DispatcherTimer();
            _timerFaceDetectionProcess.Tick += new EventHandler(timerFaceDetectionProcess_tick);
            _timerFaceDetectionProcess.Interval = new TimeSpan(0, 0, 0, Properties.Settings.Default.FaceDetectionProcessInSeconds, 0);
            SettingsViewModel = settingsViewModel;

            DetectionMode = DetectionModeEnum.OpenCV;
            DetectionInfo = new DetectionInfo();
        }

        #region Events handler

        private void videoCaptureManager_OnNewImageComplete(object sender, VideoGrabEventArgs e)
        {
            if (SettingsViewModel.IsShowCameraPreview && _videoCaptureManager.IsCaptureInProgress)
            {
                Dispatcher.CurrentDispatcher.Invoke(() => GrabImageSource = BitmapSourceConverter.ToBitmapSource(e.Image));
            }
            else
            {
                Dispatcher.CurrentDispatcher.Invoke(() => GrabImageSource = null);
            }
        }

        private void timerFaceDetectionProcess_tick(object sender, EventArgs e)
        {
            if (SettingsViewModel.IsShowDetectionFacePreview && _videoCaptureManager.IsCaptureInProgress && _timerFaceDetectionProcess.IsEnabled)
            {
                DetectFace();
            }
            else
            {
                DetectionInfo = new DetectionInfo();
                Dispatcher.CurrentDispatcher.Invoke(() => FaceDetectionImageSource = null);
            }
        }

        #endregion Events handler

        private void StartOrStopCapture()
        {
            DetectionInfo = new DetectionInfo();
            if (_videoCaptureManager.IsCaptureInProgress)
            {
                _videoCaptureManager.StopCapture();
                Dispatcher.CurrentDispatcher.Invoke(() => GrabImageSource = null);
            }
            else
            {
                _videoCaptureManager.StartCapture();
            }
        }

        private void StartOrStopFaceDetection()
        {
            DetectionInfo = new DetectionInfo();
            if (_timerFaceDetectionProcess.IsEnabled)
            {
                _timerFaceDetectionProcess.Stop();
                Dispatcher.CurrentDispatcher.Invoke(() => FaceDetectionImageSource = null);
            }
            else
            {
                _timerFaceDetectionProcess.Start();
            }
        }

        private async Task<DetectionInfo> DetectFacesBing(Mat snaphsot)
        {
            try
            {
                var detectionImage = snaphsot.ToImage<Bgr, Byte>();
                var bitmap = detectionImage.ToBitmap();
                ICollection<Rectangle> facesRectangle = null;
                ICollection<Microsoft.ProjectOxford.Face.Contract.Face> faces = null;

                using (var imageFileStream = new MemoryStream())
                {
                    bitmap.Save(imageFileStream, ImageFormat.Jpeg);
                    imageFileStream.Position = 0;
                    faces = await DetectFaceHelper.DetectFacesBing(imageFileStream);
                    var rectangles = faces.Select(face => face.FaceRectangle);
                    facesRectangle = rectangles
                        .Select((faceRectangle) => new Rectangle(faceRectangle.Left, faceRectangle.Top, faceRectangle.Width, faceRectangle.Height))
                        .ToList();
                }
                return new DetectionInfo(detectionImage, facesRectangle, faces);
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.FlattenException());
                return null;
            }
        }

        private DetectionInfo DetectFacesOpenCV(Mat snaphsot)
        {
            long detectionTime;
            var facesRectangle = new List<Rectangle>();
            var eyesRectangle = new List<Rectangle>();

            DetectFaceHelper.DetectFacesOpenCV(
              snaphsot, _faceDetectionConfigFilePath, _eyeDetectionConfigFilePath,
              facesRectangle, eyesRectangle,
              out detectionTime);
            var detectionImage = snaphsot.ToImage<Bgr, Byte>();
            return new DetectionInfo(detectionImage, facesRectangle);
        }

        private async void DetectFace()
        {
            try
            {
                if (!_videoCaptureManager.IsCaptureInProgress)
                {
                    return;
                }

                var snaphsot = _videoCaptureManager.TakeSnapshot();
                if (snaphsot == null)
                {
                    return;
                }

                DetectionInfo detectionInfo = null;
                switch (DetectionMode)
                {
                    case DetectionModeEnum.OpenCV:
                        detectionInfo = DetectFacesOpenCV(snaphsot);
                        break;

                    case DetectionModeEnum.Bing:
                        detectionInfo = await DetectFacesBing(snaphsot);
                        break;
                }

                if(detectionInfo != null)
                {
                    DetectionInfo = detectionInfo;
                    if (SettingsViewModel.IsShowDetectionFacePreview)
                    {
                        var detectionImage = detectionInfo.Image;
                        foreach (var faceRectangle in detectionInfo.Rectangles)
                        {
                            detectionImage.Draw(faceRectangle, new Bgr(0, double.MaxValue, 0), 3);
                        }
                        Dispatcher.CurrentDispatcher.Invoke(() => FaceDetectionImageSource = BitmapSourceConverter.ToBitmapSource(detectionImage));
                    }
                    else
                    {
                        Dispatcher.CurrentDispatcher.Invoke(() => FaceDetectionImageSource = null);
                    }

                    if (SettingsViewModel.IsSendToIoTHub)
                    {
                        DeviceNotification facesAnalysis = new DeviceNotification(DeviceId,
                            detectionInfo.Rectangles.Count,
                            detectionInfo.MaleCount,
                            detectionInfo.FemaleCount,
                            detectionInfo.SmileCount,
                            detectionInfo.SunGlassesCount,
                            detectionInfo.ReadingGlassesCount,
                            detectionInfo.AgeAverage,
                            detectionInfo.EmotionHappyCount,
                            detectionInfo.EmotionNeutralCount,
                            detectionInfo.EmotionDisgustCount,
                            detectionInfo.EmotionAngerCount,
                            detectionInfo.HappyRatio,
                            detectionInfo.HearyCount);

                        _iotHubPublisher.SendDataAsync(facesAnalysis);
                    }
                }
                else
                {
                    DetectionInfo = new DetectionInfo();
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.FlattenException());
            }
        }

        public override void Cleanup()
        {
            base.Cleanup();
            Dispose();
        }

        public void Dispose()
        {
            _videoCaptureManager.NewImageComplete -= videoCaptureManager_OnNewImageComplete;
            _videoCaptureManager.StopCapture();
            _videoCaptureManager.Dispose();
            _timerFaceDetectionProcess.Stop();
        }

        #endregion Methods
    }
}
