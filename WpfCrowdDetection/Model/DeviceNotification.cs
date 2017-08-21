using Newtonsoft.Json;

namespace WpfCrowdDetection.Model
{
    public class DeviceNotification
    {
        #region Properties

        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        [JsonProperty("persons")]
        public int Persons { get; set; }

        [JsonProperty("males")]
        public int Males { get; set; }

        [JsonProperty("females")]
        public int Females { get; set; }

        [JsonProperty("smiles")]
        public int Smiles { get; set; }

        [JsonProperty("age")]
        public double AverageAge { get; set; }

        [JsonProperty("sunglasses")]
        public int SunGlasses { get; set; }

        [JsonProperty("readingglasses")]
        public int ReadingGlasses { get; set; }

        [JsonProperty("happypersons")]
        public int HappyPersons { get; set; }

        [JsonProperty("neutralpersons")]
        public int NeutralPersons { get; set; }

        [JsonProperty("disgustpersons")]
        public int DisgustPersons { get; set; }

        [JsonProperty("angerpersons")]
        public int AngerPersons { get; set; }

        [JsonProperty("happyratio")]
        public double HappyRatio { get; set; }

        [JsonProperty("hearypersons")]
        public int HearyPersons { get; set; }

        #endregion Properties

        #region Constructors

        public DeviceNotification(string deviceId, int persons)
        {
            DeviceId = deviceId;
            Persons = persons;
        }

        public DeviceNotification(string deviceId,
            int persons,
            int males,
            int females,
            int smiles,
            int sunglasses,
            int readingglasses,
            double averageage,
            int happycount,
            int neutralcount,
            int disgustcount,
            int angercount,
            double happyratio,
            int hearycount)
        {
            DeviceId = deviceId;
            Persons = persons;
            Males = males;
            Females = females;
            Smiles = smiles;
            SunGlasses = sunglasses;
            ReadingGlasses = readingglasses;
            AverageAge = averageage;
            HappyPersons = happycount;
            NeutralPersons = neutralcount;
            DisgustPersons = disgustcount;
            AngerPersons = angercount;
            HappyRatio = happyratio;
            HearyPersons = hearycount;
        }

        public DeviceNotification()
        {
        }

        #endregion Constructors
    }
}