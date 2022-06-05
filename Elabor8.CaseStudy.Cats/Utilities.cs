using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Globalization;

namespace Elabor8.CaseStudy.Cats
{
    public static class Utilities
    {
        public static Dictionary<string, string> AppSettings;

        static Utilities()
        {
            using var stream = new StreamReader("appsettings.json");
            string json = stream.ReadToEnd();
            AppSettings = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        public static async Task<RestResponse> CallApi(String endpoint, Method method)
        {
            var client = new RestClient(AppSettings["baseUrl"]);
            var request = new RestRequest(endpoint, method);
            return await client.GetAsync(request);
        }

        public static List<Fact> GetAllFactsFromDataset()
        {
            var csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture) { HasHeaderRecord = true };
            using var reader = File.OpenText(AppSettings["datasetPath"]);
            using var csvReader = new CsvReader(reader, csvConfig);
            csvReader.Read(); //ignore header

            var facts = new List<Fact>();
            while (csvReader.Read())
            {
                facts.Add(new Fact
                {
                    Id = csvReader.GetField(0),
                    User = csvReader.GetField(1),
                    Text = csvReader.GetField(2),
                    FirstName = csvReader.GetField(3),
                    LastName = csvReader.GetField(4),
                    Type = csvReader.GetField(5)
                });
            }

            return facts;
        }

        public static bool CompareNodes(JObject jsonObj, string jsonPath, string expected)
        {
            var actual = jsonObj.SelectToken($"$.{jsonPath}").ToString();
            var result = actual == expected;
            if (!result)
            {
                Log.Message($"\tValue of '{jsonPath}' is not expected\n\t\tExpected: {expected}\n\t\tActual:   {actual}");
            }

            return result;
        }
    }
}
