using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitronicsApi
{
    public static class WeatherManager
    {
        public static string apiKey { get; set; } = "a0fc840e31fd67d0ed2f7ca5ebf5de24"; // Замените на ваш собственный ключ API

        public static async Task<string> GetWeatherCondition(double latitude, double longitude)
        {
            string apiUrl = $"http://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&appid={apiKey}";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();

                    JObject data = JObject.Parse(json);

                    JArray weatherArray = (JArray)data["weather"];

                    JObject weatherObject = (JObject)weatherArray[0];
                    string main = (string)weatherObject["main"];
                    return main;
                }
                else
                    return "none";
            }
        }
    }
    
}
