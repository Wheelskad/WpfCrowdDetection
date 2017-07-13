using Emgu.CV;
using Emgu.CV.Structure;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfCrowdDetection.Converters;
using WpfCrowdDetection.Helper;
using WpfCrowdDetection.Services;

namespace WpfCrowdDetection.ViewModel
{
    public class MainViewModel : ViewModelBase, IDisposable
    {
        #region Attributes

        private const string _configFileDirectory = "Dependencies";
        private const string _faceDetectionConfig = "haarcascade_frontalface_default.xml";
        private const string _eyeDetectionConfig = "haarcascade_eye.xml";
        private readonly string _faceDetectionConfigFilePath;
        private readonly string _eyeDetectionConfigFilePath;

        private readonly ICustomDialogService _dialogService;
        private readonly VideoCaptureManager _videoCaptureManager;
        private BitmapSource _grabImageSource;
        private BitmapSource _faceDetectionImageSource;
        private ICommand _startCaptureCommand;
        private ICommand _stopCaptureCommand;
        private ICommand _startFaceDetectionCommand;
        private ICommand _stopFaceDetectionCommand;

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

        public ICommand StartCaptureCommand
        {
            get
            {
                if (_startCaptureCommand == null)
                {
                    _startCaptureCommand = new RelayCommand(StartCapture);
                }
                return _startCaptureCommand;
            }
        }

        public ICommand StopCaptureCommand
        {
            get
            {
                if (_stopCaptureCommand == null)
                {
                    _stopCaptureCommand = new RelayCommand(StopCapture);
                }
                return _stopCaptureCommand;
            }
        }

        public ICommand StartFaceDetectionCommand
        {
            get
            {
                if (_startFaceDetectionCommand == null)
                {
                    _startFaceDetectionCommand = new RelayCommand(StartFaceDetection);
                }
                return _startFaceDetectionCommand;
            }
        }

        public ICommand StopFaceDetectionCommand
        {
            get
            {
                if (_stopFaceDetectionCommand == null)
                {
                    _stopFaceDetectionCommand = new RelayCommand(StopFaceDetection);
                }
                return _stopFaceDetectionCommand;
            }
        }

        public string StandName {
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

        public bool IsShowCameraPreview { get; set; }

        public bool IsShowDetectionFacePreview { get; set; }

        private DetectionModeEnum DetectionMode { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(ICustomDialogService dialogService)
        {
            _dialogService = dialogService;
            _videoCaptureManager = new VideoCaptureManager(CameraId);
            _videoCaptureManager.NewImageComplete += videoCaptureManager_OnNewImageComplete;
            _faceDetectionConfigFilePath = Path.Combine(_configFileDirectory, _faceDetectionConfig);
            _eyeDetectionConfigFilePath = Path.Combine(_configFileDirectory, _eyeDetectionConfig);

            _timerFaceDetectionProcess = new DispatcherTimer();
            _timerFaceDetectionProcess.Tick += new EventHandler(timerFaceDetectionProcess_tick);
            _timerFaceDetectionProcess.Interval = new TimeSpan(0, 0, 0, Properties.Settings.Default.FaceDetectionProcessInSeconds, 0);
            IsShowCameraPreview = true;
            IsShowDetectionFacePreview = true;

            DetectionMode = DetectionModeEnum.Bing;
        }

        #region Events handler

        private void videoCaptureManager_OnNewImageComplete(object sender, VideoGrabEventArgs e)
        {
            if (IsShowCameraPreview)
            {
                Dispatcher.CurrentDispatcher.Invoke(() => GrabImageSource = BitmapSourceConverter.ToBitmapSource(e.Image));
            }
        }

        private void timerFaceDetectionProcess_tick(object sender, EventArgs e)
        {
            if (IsShowDetectionFacePreview)
            {
                DetectFace();
            }
        }

        #endregion Events handler

        private void StartCapture()
        {
            _videoCaptureManager.StartCapture();
        }

        private void StopCapture()
        {
            _videoCaptureManager.StopCapture();
        }

        private void StartFaceDetection()
        {
            _timerFaceDetectionProcess.Start();
        }

        private void StopFaceDetection()
        {
            _timerFaceDetectionProcess.Stop();
        }

        private async Task<Image<Bgr, byte>> DetectFacesBing(Mat snaphsot)
        {
            var detectionImage = snaphsot.ToImage<Bgr, Byte>();
            var bitmap = detectionImage.ToBitmap();
            using (var imageFileStream = new MemoryStream())
            {
                bitmap.Save(imageFileStream, ImageFormat.Jpeg);
                imageFileStream.Position = 0;
                var facesRectangle = await DetectFaceHelper.DetectFacesBing(imageFileStream);
                foreach (var faceRectangle in facesRectangle)
                {
                    var rectangle = new Rectangle(faceRectangle.Left, faceRectangle.Top, faceRectangle.Width, faceRectangle.Height);
                    detectionImage.Draw(rectangle, new Bgr(0, double.MaxValue, 0), 3);
                }
            }

            return detectionImage;
        }

        private Image<Bgr, byte> DetectFacesOpenCV(Mat snaphsot)
        {
            long detectionTime;
            var facesRectangle = new List<Rectangle>();
            var eyesRectangle = new List<Rectangle>();

            DetectFaceHelper.DetectFacesOpenCV(
              snaphsot, _faceDetectionConfigFilePath, _eyeDetectionConfigFilePath,
              facesRectangle, eyesRectangle,
              out detectionTime);
            var detectionImage = snaphsot.ToImage<Bgr, Byte>();
            foreach (var faceRectangle in facesRectangle)
            {
                detectionImage.Draw(faceRectangle, new Bgr(0, double.MaxValue, 0), 3);
            }

            return detectionImage;
        }

        private async void DetectFace()
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

            Image<Bgr, byte> detectionImage = null;
            switch (DetectionMode)
            {
                case DetectionModeEnum.OpenCV:
                    detectionImage = DetectFacesOpenCV(snaphsot);
                    break;
                case DetectionModeEnum.Bing:
                    detectionImage = await DetectFacesBing(snaphsot);
                    break;
            }
            Dispatcher.CurrentDispatcher.Invoke(() => FaceDetectionImageSource = BitmapSourceConverter.ToBitmapSource(detectionImage));
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