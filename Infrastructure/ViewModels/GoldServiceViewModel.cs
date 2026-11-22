using Newtonsoft.Json;

namespace Infrastructure.ViewModels
{
    public class GoldServiceViewModel
    {
        public class Data
        {
            [JsonProperty("18ayar")]
            public Data2 Gold { get; set; }
        }
        public class Data2
        {
            public double Price { get; set; }
        }
    }
}
