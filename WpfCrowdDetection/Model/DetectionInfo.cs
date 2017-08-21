using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.ProjectOxford.Common.Contract;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WpfCrowdDetection.Model
{
    public class DetectionInfo
    {
        #region Properties

        public Image<Bgr, byte> Image { get; set; }

        public ICollection<Rectangle> Rectangles { get; set; }

        private ICollection<string> Genders { get; set; }

        private ICollection<double> Ages { get; set; }

        private ICollection<double> Smiles { get; set; }

        private ICollection<FacialHair> FacialHair { get; set; }

        private ICollection<Glasses> Glasses { get; set; }

        private ICollection<Accessory[]> Accessories { get; set; }

        private ICollection<EmotionScores> Emotions { get; set; }

        private ICollection<Face> Faces { get; set; }

        public int PersonCount { get; set; }

        public int MaleCount { get; set; }

        public int FemaleCount { get; set; }

        public double AgeAverage { get; set; }

        public int HearyCount { get; set; }

        public int SunGlassesCount { get; set; }

        public int ReadingGlassesCount { get; set; }

        public int EmotionAngerCount { get; set; }

        public int EmotionNeutralCount { get; set; }

        public int EmotionHappyCount { get; set; }

        public int EmotionDisgustCount { get; set; }

        public double HappyRatio { get; set; }

        public int SmileCount { get; set; }

        #endregion Properties

        #region Constructors

        public DetectionInfo()
        {
            InitData();
        }

        public DetectionInfo(Image<Bgr, byte> image, ICollection<Rectangle> rectangles)
        {
            Image = image;
            Rectangles = rectangles;
        }

        //Populate DetectionInfo with face recognition Attributes
        public DetectionInfo(Image<Bgr, byte> image, ICollection<Rectangle> rectangles, ICollection<Face> faces)
        {
            InitData();
            Image = image;
            Rectangles = rectangles;
            Faces = faces;

            if (faces.Any())
            {
                PersonCount = Faces.Count;
                //Gestion des genres
                Genders = faces.Select(gender => gender.FaceAttributes.Gender).ToList();
                MaleCount = Genders.Where(g => g == "male").Count();
                FemaleCount = Genders.Where(g => g == "female").Count();
                //Calcul de l'age moyen
                AgeAverage = faces.Select(age => age.FaceAttributes.Age).ToList().Average();
                //Calcul du nombre de sourire
                Smiles = faces.Select(age => age.FaceAttributes.Smile).ToList();
                SmileCount = Smiles.Where(s => s >= 0.5).Count();
                //Calcul du nombre de "poilus" (moustache ou barbe)
                FacialHair = faces.Select(age => age.FaceAttributes.FacialHair).ToList();
                foreach (var f in FacialHair)
                {
                    if (f.Beard > 0.3 || f.Moustache > 0.3) HearyCount++;
                }
                //calcul du nombre de porteur de lunettes de vue et solaire
                Glasses = faces.Select(g => g.FaceAttributes.Glasses).ToList();
                SunGlassesCount = Glasses.Where(g => g == Microsoft.ProjectOxford.Face.Contract.Glasses.Sunglasses).Count();
                ReadingGlassesCount = Glasses.Where(g => g == Microsoft.ProjectOxford.Face.Contract.Glasses.ReadingGlasses).Count();
                //Accessoires : non gérés
                Accessories = faces.Select(age => age.FaceAttributes.Accessories).ToList();
                //Ventilation des emotions selon les 4 grandes tendances
                Emotions = faces.Select(age => age.FaceAttributes.Emotion).ToList();
                foreach (var e in Emotions)
                {
                    if (GetEmotion(e) == "Happiness") EmotionHappyCount++;
                    if (GetEmotion(e) == "Anger") EmotionAngerCount++;
                    if (GetEmotion(e) == "Disgust") EmotionDisgustCount++;
                    if (GetEmotion(e) == "Neutral") EmotionNeutralCount++;
                }
                //Calcul du ratio de "happy" par rapport au nombre de personnes
                HappyRatio = Math.Round((((double)EmotionHappyCount / (double)faces.Count)) * 100, 2);
            }
        }

        #endregion Constructors

        #region Methods

        private void InitData()
        {
            PersonCount = 0;
            MaleCount = 0;
            FemaleCount = 0;
            AgeAverage = 0;
            SmileCount = 0;
            HearyCount = 0;
            SunGlassesCount = 0;
            ReadingGlassesCount = 0;
            EmotionHappyCount = 0;
            EmotionAngerCount = 0;
            EmotionDisgustCount = 0;
            EmotionNeutralCount = 0;
            HappyRatio = 0;
        }

        //Retrieve emotion with best score
        private string GetEmotion(EmotionScores emotion)
        {
            string emotionType = string.Empty;
            double emotionValue = 0.0;
            if (emotion.Anger > emotionValue)
            {
                emotionValue = emotion.Anger;
                emotionType = "Anger";
            }
            if (emotion.Contempt > emotionValue)
            {
                emotionValue = emotion.Contempt;
                emotionType = "Contempt";
            }
            if (emotion.Disgust > emotionValue)
            {
                emotionValue = emotion.Disgust;
                emotionType = "Disgust";
            }
            if (emotion.Fear > emotionValue)
            {
                emotionValue = emotion.Fear;
                emotionType = "Fear";
            }
            if (emotion.Happiness > emotionValue)
            {
                emotionValue = emotion.Happiness;
                emotionType = "Happiness";
            }
            if (emotion.Neutral > emotionValue)
            {
                emotionValue = emotion.Neutral;
                emotionType = "Neutral";
            }
            if (emotion.Sadness > emotionValue)
            {
                emotionValue = emotion.Sadness;
                emotionType = "Sadness";
            }
            if (emotion.Surprise > emotionValue)
            {
                emotionValue = emotion.Surprise;
                emotionType = "Surprise";
            }
            return $"{emotionType}";
        }

        private string GetAccessories(Accessory[] accessories)
        {
            if (accessories.Length == 0)
            {
                return "NoAccessories";
            }

            string[] accessoryArray = new string[accessories.Length];

            for (int i = 0; i < accessories.Length; ++i)
            {
                accessoryArray[i] = accessories[i].Type.ToString();
            }

            return "Accessories: " + string.Join(",", accessoryArray);
        }

        #endregion Methods
    }
}