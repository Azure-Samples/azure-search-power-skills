using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using AzureCognitiveSearch.PowerSkills.Common;
/// <summary>
/// This is my attempt at a unit test. I took information from VSE documentation and the team's current SkillsetsTest.cs
/// </summary>
namespace LookupTests
{

    [TestClass]
    public class CustomLookupTests
    {
        private static readonly HttpClient client = new HttpClient();
        
        [TestMethod]
        public void MissingWordsBadRequest()
        {
            // tests against incorrect input (missing words)
            string inputText = TestData.inputTest1;
            HttpContent jsonContent = new StringContent(inputText, null, "application/json");
            var response = client.PostAsync(TestData.hostAddress, jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("Bad Request", response.ReasonPhrase, false);
            Assert.AreEqual(TestData.missingWordsExpectedResponse, responseString, false);
        }

        [TestMethod]
        public void MissingTextBadRequest()
        {
            // tests against incorrect input (missing text)
            string missingTextPayload = TestData.inputTest2;
            HttpContent jsonContent = new StringContent(missingTextPayload, null, "application/json");
            var response = client.PostAsync(TestData.hostAddress, jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("Bad Request", response.ReasonPhrase, false);
            Assert.AreEqual(TestData.missingTextExpectedResponse, responseString, false);
        }

        [TestMethod]
        public void EmptyTextWordsNotFound()
        {
            // tests against empty string text
            string emptyText = TestData.GetPayload(@"""""", TestData.inputTest3);
            HttpContent jsonContent = new StringContent(emptyText, null, "application/json");
            var response = client.PostAsync(TestData.hostAddress, jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            WebApiResponseRecord output = new WebApiResponseRecord();
            try
            {
                 output = JsonConvert.DeserializeObject<WebApiResponseRecord>(responseString);
            }
            catch
            {
                Assert.Fail("Skill failed to handle an empty test. Errored out.");
            }
            string checkTest3 = TestData.GetOutput(TestData.inputTest3, @"-1");
            Assert.AreEqual(checkTest3, responseString, false);
        }

        [TestMethod]
        public void EmptyWordsEmptyEntities()
        {
            //tests against empty string words
            string emptyWords = TestData.GetPayload(TestData.inputTest4, @"""""");
            HttpContent jsonContent = new StringContent(emptyWords, null, "application/json");
            var response = client.PostAsync(TestData.hostAddress, jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            WebApiResponseRecord output = new WebApiResponseRecord();
            try
            {
                output = JsonConvert.DeserializeObject<WebApiResponseRecord>(responseString);
            }
            catch
            {
                Assert.Fail("Skill failed to handle an empty test. Errored out.");
            }
            string checkTest4 = TestData.GetOutput(@"""""", @"-1");
            Assert.AreEqual(checkTest4, responseString, false);
        }

        [TestMethod]
        public void LargeTextQuickResult()
        {
            // tests against large text string
            string largeText = TestData.GetPayload(TestData.largestText, TestData.inputTest5);
            HttpContent jsonContent = new StringContent(largeText, null, "application/json");
            var response = client.PostAsync(TestData.hostAddress, jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            WebApiResponseRecord output = new WebApiResponseRecord();
            try
            {
                output = JsonConvert.DeserializeObject<WebApiResponseRecord>(responseString);
            }
            catch
            {
                Assert.Fail("Skill failed to handle an empty test. Errored out.");
            }
            string checkTest5 = TestData.GetOutput(TestData.inputTest5, TestData.outputTest5);
            Assert.AreEqual(checkTest5, responseString, false);
        }

        [TestMethod]
        public void LargeWordsQuickResult()
        {
            // tests against large pattern in words array
            string largeWord = TestData.GetPayload(TestData.inputTest6text, TestData.inputTest6words);
            HttpContent jsonContent = new StringContent(largeWord, null, "application/json");
            var response = client.PostAsync(TestData.hostAddress, jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            WebApiResponseRecord output = new WebApiResponseRecord();
            try
            {
                output = JsonConvert.DeserializeObject<WebApiResponseRecord>(responseString);
            }
            catch
            {
                Assert.Fail("Skill failed to handle an empty test. Errored out.");
            }
            string checkTest6 = TestData.GetOutput(TestData.inputTest6words, TestData.outputTest6);
            Assert.AreEqual(checkTest6, responseString, false);
        }

        [TestMethod]
        public void LargeDatasetQuickResult()
        {
            //tests against a large number of documents inputted (loadtest)
            string text = TestData.inputElement.Replace("#REPLACE ME#", TestData.largestText);
            string words = text.Replace("#INSERT WORDS#", TestData.inputTest5);
            string docs = String.Concat(Enumerable.Repeat(words + ",", TestData.numDocs));
            docs = docs.Remove(docs.Length - 1);
            string content = TestData.inputCheckTest.Replace("#REPLACE ME#", docs);

            HttpContent jsonContent = new StringContent(content, null, "application/json");
            var response = client.PostAsync(TestData.hostAddress, jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            WebApiResponseRecord output = new WebApiResponseRecord();
            try
            {
                output = JsonConvert.DeserializeObject<WebApiResponseRecord>(responseString);
            }
            catch
            {
                Assert.Fail("Skill failed to handle an empty test. Errored out.");
            }

            string[] nameReplace = TestData.inputTest5.Split(", ");
            string[] matchReplace = TestData.outputTest5.Split(", ");
            string data = "";
            for (int i = 0; i < nameReplace.Length; i++)
            {
                string temp = TestData.outputValue.Replace("#REPLACE ME#", nameReplace[i]);
                temp = temp.Replace("#NUMBER#", matchReplace[i]);
                data += temp + ",";
            }
            data = data.Remove(data.Length - 1);
            string allData = "";
            for (int j = 0; j < TestData.numDocs; j++)
            {
                allData += TestData.outputElement.Replace("#REPLACE ME#", data) + ",";
            }
            allData = allData.Remove(allData.Length - 1);
            string checkTest7 = TestData.outputCheckTest.Replace("#REPLACE ME#", allData);
            Assert.AreEqual(checkTest7, responseString, false);
        }

        [TestMethod]
        public void LargeNumWordsQuickResult()
        {
            // tests against a large number of patterns in words array
            string largeNumWords = TestData.GetPayload(TestData.largestText, TestData.largestWords);
            HttpContent jsonContent = new StringContent(largeNumWords, null, "application/json");
            var response = client.PostAsync(TestData.hostAddress, jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            WebApiResponseRecord output = new WebApiResponseRecord();
            try
            {
                output = JsonConvert.DeserializeObject<WebApiResponseRecord>(responseString);
            }
            catch
            {
                Assert.Fail("Skill failed to handle an empty test. Errored out.");
            }
            string checkTest8 = TestData.GetOutput(TestData.largestWords, TestData.outputTest8);
            Assert.AreEqual(checkTest8, responseString, false);
        } 
    }
}
