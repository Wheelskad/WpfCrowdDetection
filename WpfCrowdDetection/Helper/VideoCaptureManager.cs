using Emgu.CV;
using System;

namespace WpfCrowdDetection.Helper
{
    #region Events

    public class VideoGrabEventArgs : EventArgs
    {
        public VideoGrabEventArgs(IImage image)
        {
            Image = image;
        }

        public IImage Image { get; set; }
    }

    public delegate void VideoGrabEventHandler(object source, VideoGrabEventArgs args);

    #endregion Events

    public class VideoCaptureManager : IDisposable
    {
        #region Attributes

        private VideoCapture _capture = null;
        private Mat _frame;
        private Mat _grayFrame;

        public event EventHandler<VideoGrabEventArgs> NewImageComplete;

        #endregion Attributes

        #region Properties

        public bool IsCaptureInProgress { get; set; }

        #endregion Properties

        #region Methods

        public VideoCaptureManager(int cameraId)
        {
            Initialize(cameraId);
        }

        private void Initialize(int cameraId)
        {
            CvInvoke.UseOpenCL = false;
            _capture = new VideoCapture(cameraId); 
            _capture.ImageGrabbed += ProcessFrame;
            _frame = new Mat();
            _grayFrame = new Mat();
        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            if (_capture != null && _capture.Ptr != IntPtr.Zero)
            {
                _capture.Retrieve(_frame, 0);
                NewImageComplete?.Invoke(this, new VideoGrabEventArgs(_frame));
            }
        }

        public void StartCapture()
        {
            if (!IsCaptureInProgress)
            {
                _capture.Start();
                IsCaptureInProgress = true;
            }
        }

        public void StopCapture()
        {
            if (IsCaptureInProgress)
            {
                _capture.Pause();
                IsCaptureInProgress = false;
            }
        }

        public Mat TakeSnapshot()
        {
            if (IsCaptureInProgress)
            {
                var image = _capture.QueryFrame();
                return image;
            }
            return null;
        }

        public void Dispose()
        {
            if (_capture != null)
            {
                _capture.Dispose();
            }
        }

        #endregion Methods
    }
}