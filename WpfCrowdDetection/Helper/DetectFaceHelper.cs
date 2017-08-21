using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.Structure;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace WpfCrowdDetection.Helper
{
    public static class DetectFaceHelper
    {

        private static readonly IFaceServiceClient faceServiceClient = new FaceServiceClient("fd89d685a0754f0790e4383bfbcd36b8", "https://westus.api.cognitive.microsoft.com/face/v1.0");

        public static void DetectFacesOpenCV(
           IInputArray image, String faceFileName, String eyeFileName,
           List<Rectangle> faces, List<Rectangle> eyes,
           out long detectionTime)
        {
            Stopwatch watch;

            using (InputArray iaImage = image.GetInputArray())
            {
#if !(__IOS__ || NETFX_CORE)
                if (iaImage.Kind == InputArray.Type.CudaGpuMat && CudaInvoke.HasCuda)
                {
                    using (CudaCascadeClassifier face = new CudaCascadeClassifier(faceFileName))
                    using (CudaCascadeClassifier eye = new CudaCascadeClassifier(eyeFileName))
                    {
                        face.ScaleFactor = 1.1;
                        face.MinNeighbors = 10;
                        face.MinObjectSize = Size.Empty;
                        eye.ScaleFactor = 1.1;
                        eye.MinNeighbors = 10;
                        eye.MinObjectSize = Size.Empty;
                        watch = Stopwatch.StartNew();
                        using (CudaImage<Bgr, Byte> gpuImage = new CudaImage<Bgr, byte>(image))
                        using (CudaImage<Gray, Byte> gpuGray = gpuImage.Convert<Gray, Byte>())
                        using (GpuMat region = new GpuMat())
                        {
                            face.DetectMultiScale(gpuGray, region);
                            Rectangle[] faceRegion = face.Convert(region);
                            faces.AddRange(faceRegion);
                            foreach (Rectangle f in faceRegion)
                            {
                                using (CudaImage<Gray, Byte> faceImg = gpuGray.GetSubRect(f))
                                {
                                    //For some reason a clone is required.
                                    //Might be a bug of CudaCascadeClassifier in opencv
                                    using (CudaImage<Gray, Byte> clone = faceImg.Clone(null))
                                    using (GpuMat eyeRegionMat = new GpuMat())
                                    {
                                        eye.DetectMultiScale(clone, eyeRegionMat);
                                        Rectangle[] eyeRegion = eye.Convert(eyeRegionMat);
                                        foreach (Rectangle e in eyeRegion)
                                        {
                                            Rectangle eyeRect = e;
                                            eyeRect.Offset(f.X, f.Y);
                                            eyes.Add(eyeRect);
                                        }
                                    }
                                }
                            }
                        }
                        watch.Stop();
                    }
                }
                else
#endif
                {
                    //Read the HaarCascade objects
                    using (CascadeClassifier face = new CascadeClassifier(faceFileName))
                    using (CascadeClassifier eye = new CascadeClassifier(eyeFileName))
                    {
                        watch = Stopwatch.StartNew();

                        using (UMat ugray = new UMat())
                        {
                            CvInvoke.CvtColor(image, ugray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

                            //normalizes brightness and increases contrast of the image
                            CvInvoke.EqualizeHist(ugray, ugray);

                            //Detect the faces  from the gray scale image and store the locations as rectangle
                            //The first dimensional is the channel
                            //The second dimension is the index of the rectangle in the specific channel
                            Rectangle[] facesDetected = face.DetectMultiScale(
                               ugray,
                               1.1,
                               10,
                               new Size(20, 20));

                            faces.AddRange(facesDetected);

                            foreach (Rectangle f in facesDetected)
                            {
                                //Get the region of interest on the faces
                                using (UMat faceRegion = new UMat(ugray, f))
                                {
                                    Rectangle[] eyesDetected = eye.DetectMultiScale(
                                       faceRegion,
                                       1.1,
                                       10,
                                       new Size(20, 20));

                                    foreach (Rectangle e in eyesDetected)
                                    {
                                        Rectangle eyeRect = e;
                                        eyeRect.Offset(f.X, f.Y);
                                        eyes.Add(eyeRect);
                                    }
                                }
                            }
                        }
                        watch.Stop();
                    }
                }
                detectionTime = watch.ElapsedMilliseconds;
            }
        }

        public static async Task<Face[]> DetectFacesBing(System.IO.Stream imageStream)
        {
            Stopwatch watch;
            try
            {
                //Ajout des attributs du visage pour détection genre, tranche d'âge, sourire, barbe et lunettes
                var requiredFaceAttributes = new FaceAttributeType[] {
                FaceAttributeType.Age,
                FaceAttributeType.Gender,
                FaceAttributeType.Smile,
                FaceAttributeType.Hair,
                FaceAttributeType.FacialHair,
                FaceAttributeType.Glasses,
                FaceAttributeType.Accessories,
                FaceAttributeType.Emotion     
            };

                watch = Stopwatch.StartNew();
                //var faces = await faceServiceClient.DetectAsync(imageStream);
                var faces = await faceServiceClient.DetectAsync(imageStream, returnFaceId:true, returnFaceLandmarks:false, returnFaceAttributes: requiredFaceAttributes);
                //var faceRects = faces.Select(face => face.FaceRectangle);
                watch.Stop();
                var detectionTime = watch.ElapsedMilliseconds;
                return faces.ToArray();
            }
            catch (Exception ex)
            {
                return new Face[0];
            }
        }
    }
}