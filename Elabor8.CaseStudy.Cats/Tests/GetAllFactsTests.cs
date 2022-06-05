using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;
using System.Net;

namespace Elabor8.CaseStudy.Cats.Tests
{
    [TestFixture]
    public class GetAllFactsTests
    {
        private const string _Endpoint = "/facts";
        private readonly List<Fact> _Facts = Utilities.GetAllFactsFromDataset();

        [Test]
        public void ReturnsValidResponse()
        {
            var response = Utilities.CallApi(_Endpoint, Method.Get).Result;

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Is.Not.Null);
        }

        [Test]
        public void CheckAllFactsExistInMasterDataSet()
        {
            var response = Utilities.CallApi(_Endpoint, Method.Get).Result;
            var result = JsonConvert.DeserializeObject(response.Content);
            var actualFacts = JArray.Parse(result.ToString());

            bool areAllFactsExist = true;
            var missingFacts = new List<Fact>(_Facts);
            foreach (JObject actualFact in actualFacts)
            {
                var actualFactId = actualFact.SelectToken("$._id").ToString();
                Log.Message($"Fact Id {actualFactId}");

                // Check that fact exists in the dataset
                var datasetFact = missingFacts.FirstOrDefault(x => x.Id == actualFactId);
                if (datasetFact == null)
                {
                    areAllFactsExist = false;
                    Log.Message("\tThis does not exist in the master dataset.");
                    continue;
                }

                // Check that fact properties is same in the dataset
                bool areDetailsEqual = Utilities.CompareNodes(actualFact, "user", datasetFact.User) &&
                    Utilities.CompareNodes(actualFact, "text", datasetFact.Text) &&
                    Utilities.CompareNodes(actualFact, "type", datasetFact.Type);
                areAllFactsExist &= areDetailsEqual;
                Log.Message($"\tAll properties are equal: {areDetailsEqual}");

                // Remove verified fact
                missingFacts.RemoveAll(x => x.Id == actualFactId);
            }

            foreach (var missingFact in missingFacts)
            {
                Log.Message($"Fact Id {missingFact.Id}\n\tThis fact from master dataset is not found in the response.");
            }

            Assert.Multiple(() =>
            {
                Assert.That(areAllFactsExist, Is.True, "Some facts are not as expected. Please check logs.");
                Assert.That(missingFacts.Any(), Is.False, "There are facts from the master dataset that do not exist from the API response. Please check logs.");
            });
        }
    }
}
