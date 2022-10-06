using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Session2._1
{
    [TestClass]
    public class UnitTest1
    {
        private static HttpClient httpClient;

        private static readonly string BaseURL = "https://petstore.swagger.io/v2/";

        private static readonly string PetEndpoint = "pet";

        private static string GetURL(string endpoint) => $"{BaseURL}{endpoint}";

        private static Uri GetURI(string endpoint) => new Uri(GetURL(endpoint));

        private readonly List<PetModel> cleanUpList = new List<PetModel>();

        [TestInitialize]
        public void TestInitialize()
        {
            httpClient = new HttpClient();
        }

        [TestCleanup]
        public async Task TestCleanUp()
        {
            foreach (var data in cleanUpList)
            {
                var httpResponse = await httpClient.DeleteAsync(GetURL($"{PetEndpoint}/{data.Id}"));
            }
        }

        [TestMethod]
        public async Task PutMethod()
        {
            #region create data
            var random = new Random();
            int randomId = random.Next(1, 100);

            // Create Json Object
            PetModel petData = new PetModel()
            {
                Id = randomId,
                Name = "Tommy",
                PhotoUrls = new string[1] { "Photo_String" },
                Category = new Category { Id = randomId, Name = "Cat" },
                Tags = new Category[1] { new Category { Id = randomId, Name = "Orange" } },
                Status = "available"
            };

            // Serialize Content
            var request = JsonConvert.SerializeObject(petData);
            var postRequest = new StringContent(request, Encoding.UTF8, "application/json");

            // Send Post Request
            await httpClient.PostAsync(GetURL(PetEndpoint), postRequest);

            #endregion

            #region get Username of the created data

            // Get Request
            var getResponse = await httpClient.GetAsync(GetURI($"{PetEndpoint}/{petData.Id}"));

            // Deserialize Content
            var listPetData = JsonConvert.DeserializeObject<PetModel>(getResponse.Content.ReadAsStringAsync().Result);

            // filter created data
            var createdPetData = listPetData.Id;

            #endregion

            #region send put request to update data

            // Update value of userData
            petData = new PetModel()
            {
                Id = listPetData.Id,
                Name = "Tommy.put",
                PhotoUrls = listPetData.PhotoUrls,
                Category = listPetData.Category,
                Tags = listPetData.Tags,
                Status = listPetData.Status,
            };

            // Serialize Content
            request = JsonConvert.SerializeObject(petData);
            postRequest = new StringContent(request, Encoding.UTF8, "application/json");

            // Send Put Request
            var httpResponse = await httpClient.PutAsync(GetURL($"{PetEndpoint}"), postRequest);

            // Get Status Code
            var statusCode = httpResponse.StatusCode;

            #endregion

            #region get updated data

            // Get Request
            getResponse = await httpClient.GetAsync(GetURI($"{PetEndpoint}/{petData.Id}"));

            // Deserialize Content
            listPetData = JsonConvert.DeserializeObject<PetModel>(getResponse.Content.ReadAsStringAsync().Result);

            // filter created data
            string createdPetName = listPetData.Name;

            #endregion

            #region cleanup data

            // Add data to cleanup list
            cleanUpList.Add(listPetData);

            #endregion

            #region assertion

            // Assertion
            Assert.AreEqual(HttpStatusCode.OK, statusCode, "Status code is not equal to 200");
            Assert.AreEqual(petData.Name, createdPetName, "Username not matching");

            #endregion
        }
    }
}
