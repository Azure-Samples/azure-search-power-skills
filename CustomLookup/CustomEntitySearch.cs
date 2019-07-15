using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Formatting;
using System.Text;
using Microsoft.AspNetCore.Routing.Template;

namespace CustomLookup
{
    /// <summary>
    /// Based on sample custom skill provided in Azure Search. Provided a user-defined list of entities
    /// this function determines the entities first occurrence within a given document. This list of entities
    /// must repeatedly be provided by the user for each document.
    /// </summary>
    public static class CustomEntitySearch
    {
        /// <summary>
        /// We assert the following assumptions:
        /// 1. All text files are Latin based (i.e. no languageCode)
        /// 2. Words can contain special characters and numbers
        /// 3. The provided entities are not case sensitive
        /// </summary>
        #region Classes used to deserialize the request
        private class InputRecord
        {
            public class InputRecordData
            {
                public string Text { get; set; }
                public List<string> Words { get; set; }
            }

            public string RecordId { get; set; }
            public InputRecordData Data { get; set; }
        }

        private class WebApiRequest
        {
            public List<InputRecord> Values { get; set; }
        }
        #endregion

        #region Classes used to serialize the response

        private class OutputRecord
        {
            public class OutputRecordData
            {
                public string Name { get; set; } = "";
                public string MatchIndex { get; set; } = "";
            }

            public string RecordId { get; set; }
            public List<OutputRecordData> Data { get; set; }
        }

        private class WebApiResponse
        {
            public List<OutputRecord> Values { get; set; }
        }
        #endregion

        #region The Azure Function definition (JSON conversion, schema check, and response production all from sample)

        [FunctionName("CustomEntitySearch")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Custom Entity Search function: C# HTTP trigger function processed a request.");

            var response = new WebApiResponse
            {
                Values = new List<OutputRecord>()
            };

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            var data = JsonConvert.DeserializeObject<WebApiRequest>(requestBody);
                              
            // Do some schema validation
            if (data == null)
            {
                return new BadRequestObjectResult("The request schema does not match expected schema.");
            }
            if (data.Values == null)
            {
                return new BadRequestObjectResult("The request schema does not match expected schema. Could not find values array.");
            }

            // Calculate the response for each value.
            foreach (var record in data.Values)
            {
                if (record == null || record.RecordId == null) continue;

                if (record.Data.Text == null)
                {
                    return new BadRequestObjectResult("The request schema does not match expected schema. Could not find text string.");
                }

                if (record.Data.Words == null)
                {
                    return new BadRequestObjectResult("The request schema does not match expected schema. Could not find words array.");
                }

                OutputRecord responseRecord = new OutputRecord
                {
                    RecordId = record.RecordId
                };

                try
                {
                    responseRecord.Data = await FindEntity(record.Data.Text, record.Data.Words);
                }
                catch (Exception e)
                {
                    // Something bad happened, log the issue.
                    return new BadRequestObjectResult(e.Message);
                }
                finally
                {
                    response.Values.Add(responseRecord);
                }
            }

            return (ActionResult)new OkObjectResult(response);
        }
        #endregion

        #region Helper functions (My main change to the sample code provided)
        /// <summary>
        /// Given text document and user-defined entities, this function determines if the provided
        /// entities exist in the given text. If so, the first match's index is returned. Otherwise,
        /// an index of "-1" is returned. Assumes user wants just the entity or entity followed by any word character.
        /// </summary>
        /// <param name="text"> Text document provided by user</param>
        /// <param name="words"> User-defined entities </param>
        /// <returns></returns>
        private async static Task<List<OutputRecord.OutputRecordData>> FindEntity(string text, List<string> words)
        {
            List <OutputRecord.OutputRecordData>  data = new List<OutputRecord.OutputRecordData>();

            string[] multiComp = new string[words.Count];
            for (int i = 0; i < words.Count; i++)
            {
                string alteredWord = Regex.Escape(words[i]);
                multiComp[i] = alteredWord;
            }

            for (int j = 0; j < words.Count; j++)
            {
                string pattern = @"\b(?ix-m:" + multiComp[j] + ")";
                Match entityMatch = Regex.Match(text, pattern, RegexOptions.IgnoreCase, new TimeSpan(1));
                bool success = (multiComp[j] == "" && text != "") ? false : entityMatch.Success;
                if (success)
                {
                    data.Add(
                        new OutputRecord.OutputRecordData
                        {
                            Name = words[j],
                            MatchIndex = entityMatch.Index.ToString()
                        }) ;
                }
                else
                {
                    data.Add(
                        new OutputRecord.OutputRecordData
                        {
                            Name = words[j],
                            MatchIndex = "-1"
                        });
                }
            }

            return data;
        }
        #endregion
        }
}