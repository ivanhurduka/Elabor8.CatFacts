using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;
using System.Net;

namespace Elabor8.CaseStudy.Cats.Tests
{
    [TestFixture]
    public class GetFactsByIdTests
    {
        private const string _Endpoint = "/facts/{0}";
        private readonly List<Fact> _Facts = Utilities.GetAllFactsFromDataset();

        [Test]
        public void ReturnsValidResponse()
        {
            var id = _Facts[0].Id;

            var response = Utilities.CallApi(string.Format(_Endpoint, id), Method.Get).Result;

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Is.Not.Null);
        }

        [Test]
        public void CheckEachRowInMasterDataset_ReturnedByApi()
        {
            bool areAllFactsExist = true;
            foreach (var fact in _Facts)
            {
                var result = Utilities.CallApi(string.Format(_Endpoint, fact.Id), Method.Get).Result.Content;
                var jsonObj = JObject.Parse(result);

                // Check that fact from dataset exists in the response
                if (jsonObj.SelectToken("$.user._id") == null)
                {
                    areAllFactsExist = false;
                    Log.Message($"Fact Id {fact.Id}\n\tThis fact does not exist in the response.");
                    continue;
                }

                // Check that fact properties is the same in the dataset
                Log.Message($"Fact Id {fact.Id}");
                bool areDetailsEqual = Utilities.CompareNodes(jsonObj, "_id", fact.Id) &&
                    Utilities.CompareNodes(jsonObj, "user._id", fact.User) &&
                    Utilities.CompareNodes(jsonObj, "text", fact.Text) &&
                    Utilities.CompareNodes(jsonObj, "user.name.first", fact.FirstName) &&
                    Utilities.CompareNodes(jsonObj, "user.name.last", fact.LastName) &&
                    Utilities.CompareNodes(jsonObj, "type", fact.Type);
                areAllFactsExist &= areDetailsEqual;

                Log.Message($"\tAll properties are equal: {areDetailsEqual}");
            }

            Assert.That(areAllFactsExist, Is.True, "Some facts are not as expected. Please check logs.");
        }
    }
}
