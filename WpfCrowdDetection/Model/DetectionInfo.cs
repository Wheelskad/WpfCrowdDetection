using Emgu.CV;
using Emgu.CV.Structure;
using System.Collections.Generic;
using System.Drawing;

namespace WpfCrowdDetection.Model
{
    public class DetectionInfo
    {
        #region Properties

        public Image<Bgr, byte> Image { get; set; }

        public ICollection<Rectangle> Rectangles { get; set; }

        #endregion Properties

        public DetectionInfo(Image<Bgr, byte> image, ICollection<Rectangle> rectangles)
        {
            Image = image;
            Rectangles = rectangles;
        }
    }
}