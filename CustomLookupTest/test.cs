using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Tests;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Threading;
using System.Timers;
using System.Linq;
using System.Runtime.ExceptionServices;
/// <summary>
/// This is my attempt at a unit test. I took information from VSE documentation and the team's current SkillsetsTest.cs
/// </summary>
namespace Test
{

    [TestClass]
    public class CustomLookupTests
    {
        private static readonly HttpClient client = new HttpClient();
        
        [TestMethod]
        public void CustomLookupTest1()
        {
            // tests against incorrect input (missing words)
            string content = Constants.inputTest1;
            HttpContent jsonContent = new StringContent(content, null, "application/json");
            var response = client.PostAsync("http://localhost:7071/api/CustomEntitySearch", jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("Bad Request", response.ReasonPhrase, false);
            Assert.AreEqual(Constants.checkTest1, responseString, false);
        }

        [TestMethod]
        public void CustomLookupTest2()
        {
            // tests against incorrect input (missing text)
            string content = Constants.inputTest2;
            HttpContent jsonContent = new StringContent(content, null, "application/json");
            var response = client.PostAsync("http://localhost:7071/api/CustomEntitySearch", jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("Bad Request", response.ReasonPhrase, false);
            Assert.AreEqual(Constants.checkTest2, responseString, false);
        }

        [TestMethod]
        public void CustomLookupTest3()
        {
            // tests against empty string text
            string text = Constants.inputElement.Replace("#REPLACE ME#", @"""""");
            string words = text.Replace("#INSERT WORDS#", Constants.inputTest3);
            string content = Constants.inputCheckTest.Replace("#REPLACE ME#", words);
            HttpContent jsonContent = new StringContent(content, null, "application/json");
            var response = client.PostAsync("http://localhost:7071/api/CustomEntitySearch", jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            SkillOutput output = new SkillOutput { values = new List<OutputValues>() };
            try
            {
                 output = JsonConvert.DeserializeObject<SkillOutput>(responseString);
            }
            catch
            {
                Assert.Fail("Skill failed to handle an empty test. Errored out.");
            }
            string data = Constants.outputValue.Replace("#REPLACE ME#", Constants.inputTest3);
            data = data.Replace("#NUMBER#", @"""-1""");
            string oneEntity = Constants.outputElement.Replace("#REPLACE ME#", data);
            string checkTest3 = Constants.outputCheckTest.Replace("#REPLACE ME#", oneEntity);
            Assert.AreEqual(checkTest3, responseString, false);
        }

        [TestMethod]
        public void CustomLookupTest4()
        {
            //tests against empty string words
            string text = Constants.inputElement.Replace("#REPLACE ME#", Constants.inputTest4);
            string words = text.Replace("#INSERT WORDS#", @"""""");
            string content = Constants.inputCheckTest.Replace("#REPLACE ME#", words);
          
            HttpContent jsonContent = new StringContent(content, null, "application/json");
            var response = client.PostAsync("http://localhost:7071/api/CustomEntitySearch", jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            SkillOutput output = new SkillOutput { values = new List<OutputValues>() };
            try
            {
                output = JsonConvert.DeserializeObject<SkillOutput>(responseString);
            }
            catch
            {
                Assert.Fail("Skill failed to handle an empty test. Errored out.");
            }
            string data = Constants.outputValue.Replace("#REPLACE ME#", @"""""");
            data = data.Replace("#NUMBER#", @"""-1""");
            string oneEntity = Constants.outputElement.Replace("#REPLACE ME#", data);
            string checkTest4 = Constants.outputCheckTest.Replace("#REPLACE ME#", oneEntity);
            Assert.AreEqual(checkTest4, responseString, false);
        }

        [TestMethod]
        public void CustomLookupTest5()
        {
            // tests against large text string
            string text = Constants.inputElement.Replace("#REPLACE ME#", Constants.largestText);
            string words = text.Replace("#INSERT WORDS#", Constants.inputTest5);
            string content = Constants.inputCheckTest.Replace("#REPLACE ME#", words);

            HttpContent jsonContent = new StringContent(content, null, "application/json");
            var response = client.PostAsync("http://localhost:7071/api/CustomEntitySearch", jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            SkillOutput output = new SkillOutput { values = new List<OutputValues>() };
            try
            {
                output = JsonConvert.DeserializeObject<SkillOutput>(responseString);
            }
            catch
            {
                Assert.Fail("Skill failed to handle an empty test. Errored out.");
            }

            string[] nameReplace = Constants.inputTest5.Split(", ");
            string[] matchReplace = Constants.outputTest5.Split(", ");
            string data = "";
            for (int i = 0; i < nameReplace.Length; i++)
            {
                string temp = Constants.outputValue.Replace("#REPLACE ME#", nameReplace[i]);
                temp = temp.Replace("#NUMBER#", matchReplace[i]);
                data += temp + ",";
            }
            data = data.Remove(data.Length - 1);
            string oneEntity = Constants.outputElement.Replace("#REPLACE ME#", data);
            string checkTest5 = Constants.outputCheckTest.Replace("#REPLACE ME#", oneEntity);
            Assert.AreEqual(checkTest5, responseString, false);
        }

        [TestMethod]
        public void CustomLookupTest6()
        {
            // tests against large pattern in words array
            string text = Constants.inputElement.Replace("#REPLACE ME#", Constants.inputTest6text);
            string words = text.Replace("#INSERT WORDS#", Constants.inputTest6words);
            string content = Constants.inputCheckTest.Replace("#REPLACE ME#", words);
            HttpContent jsonContent = new StringContent(content, null, "application/json");
            var response = client.PostAsync("http://localhost:7071/api/CustomEntitySearch", jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            SkillOutput output = new SkillOutput { values = new List<OutputValues>() };
            try
            {
                output = JsonConvert.DeserializeObject<SkillOutput>(responseString);
            }
            catch
            {
                Assert.Fail("Skill failed to handle an empty test. Errored out.");
            }

            string[] nameReplace = Constants.inputTest6words.Split(", ");
            string[] matchReplace = Constants.outputTest6.Split(", ");
            string data = "";
            for (int i = 0; i < nameReplace.Length; i++)
            {
                string temp = Constants.outputValue.Replace("#REPLACE ME#", nameReplace[i]);
                temp = temp.Replace("#NUMBER#", matchReplace[i]);
                data += temp + ",";
            }
            data = data.Remove(data.Length - 1);
            string oneEntity = Constants.outputElement.Replace("#REPLACE ME#", data);
            string checkTest6 = Constants.outputCheckTest.Replace("#REPLACE ME#", oneEntity);
            Assert.AreEqual(checkTest6, responseString, false);
        }

        [TestMethod]
        public void CustomLookupTest7()
        {
            //tests against a large number of documents inputted (loadtest)
            string text = Constants.inputElement.Replace("#REPLACE ME#", Constants.largestText);
            string words = text.Replace("#INSERT WORDS#", Constants.inputTest5);
            string docs = String.Concat(Enumerable.Repeat(words + ",", Constants.numDocs));
            docs = docs.Remove(docs.Length - 1);
            string content = Constants.inputCheckTest.Replace("#REPLACE ME#", docs);

            HttpContent jsonContent = new StringContent(content, null, "application/json");
            var response = client.PostAsync("http://localhost:7071/api/CustomEntitySearch", jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            SkillOutput output = new SkillOutput { values = new List<OutputValues>() };
            try
            {
                output = JsonConvert.DeserializeObject<SkillOutput>(responseString);
            }
            catch
            {
                Assert.Fail("Skill failed to handle an empty test. Errored out.");
            }

            string[] nameReplace = Constants.inputTest5.Split(", ");
            string[] matchReplace = Constants.outputTest5.Split(", ");
            string data = "";
            for (int i = 0; i < nameReplace.Length; i++)
            {
                string temp = Constants.outputValue.Replace("#REPLACE ME#", nameReplace[i]);
                temp = temp.Replace("#NUMBER#", matchReplace[i]);
                data += temp + ",";
            }
            data = data.Remove(data.Length - 1);
            string allData = "";
            for (int j = 0; j < Constants.numDocs; j++)
            {
                allData += Constants.outputElement.Replace("#REPLACE ME#", data) + ",";
            }
            allData = allData.Remove(allData.Length - 1);
            string checkTest7 = Constants.outputCheckTest.Replace("#REPLACE ME#", allData);
            Assert.AreEqual(checkTest7, responseString, false);
        }

        [TestMethod]
        public void CustomLookupTest8()
        {
            // tests against a large number of patterns in words array
            string text = Constants.inputElement.Replace("#REPLACE ME#", Constants.largestText);
            string words = text.Replace("#INSERT WORDS#", Constants.largestWords);
            string content = Constants.inputCheckTest.Replace("#REPLACE ME#", words);
            HttpContent jsonContent = new StringContent(content, null, "application/json");
            var response = client.PostAsync("http://localhost:7071/api/CustomEntitySearch", jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            SkillOutput output = new SkillOutput { values = new List<OutputValues>() };
            try
            {
                output = JsonConvert.DeserializeObject<SkillOutput>(responseString);

            }
            catch
            {
                Assert.Fail("Skill failed to handle an empty test. Errored out.");
            }

            string[] nameReplace = Constants.largestWords.Split(", ");
            string[] matchReplace = Constants.outputTest8.Split(", ");
            string data = "";
            for (int i = 0; i < nameReplace.Length; i++)
            {
                string temp = Constants.outputValue.Replace("#REPLACE ME#", nameReplace[i]);
                temp = temp.Replace("#NUMBER#", matchReplace[i]);
                data += temp + ",";
            }
            data = data.Remove(data.Length - 1);
            string oneEntity = Constants.outputElement.Replace("#REPLACE ME#", data);
            string checkTest8 = Constants.outputCheckTest.Replace("#REPLACE ME#", oneEntity);
            Assert.AreEqual(checkTest8, responseString, false);
        }

        [TestMethod]
        public async Task CustomLookupTest9()
        {
            // tests against multiple requests
            HttpClient newClient = new HttpClient();
            string text = Constants.inputElement.Replace("#REPLACE ME#", Constants.inputTest4);
            string words = text.Replace("#INSERT WORDS#", @"""""");
            string content = Constants.inputCheckTest.Replace("#REPLACE ME#", words);
            string newContent = Constants.inputTest1;
            HttpContent newJsonContent = new StringContent(newContent, null, "application/json");
            HttpContent jsonContent = new StringContent(content, null, "application/json");

            var response = client.PostAsync("http://localhost:7071/api/CustomEntitySearch", jsonContent).Result;
            var newResponse = await client.PostAsync("http://localhost:7071/api/CustomEntitySearch", newJsonContent);
            string newResponseString = await newResponse.Content.ReadAsStringAsync();
            string responseString = await response.Content.ReadAsStringAsync();
            SkillOutput output = new SkillOutput { values = new List<OutputValues>() };
            try
            {
                output = JsonConvert.DeserializeObject<SkillOutput>(responseString);
            }
            catch
            {
                Assert.Fail("Skill failed to handle an empty test. Errored out.");
            }
            string data = Constants.outputValue.Replace("#REPLACE ME#", @"""""");
            data = data.Replace("#NUMBER#", @"""-1""");
            string oneEntity = Constants.outputElement.Replace("#REPLACE ME#", data);
            string checkTest4 = Constants.outputCheckTest.Replace("#REPLACE ME#", oneEntity);
            Assert.AreEqual(checkTest4, responseString, false);
            Assert.AreEqual("Bad Request", newResponse.ReasonPhrase, false);
            Assert.AreEqual(Constants.checkTest1, newResponseString, false);
        }
    }
}
