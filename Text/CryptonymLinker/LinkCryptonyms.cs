using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using AzureCognitiveSearch.PowerSkills.Common;
using System.Linq;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AzureCognitiveSearch.PowerSkills.Text.CryptonymLinker
{
    public static class LinkCryptonyms
    {
        [FunctionName("link-cryptonyms")]
        public static IActionResult RunCryptonymLinker(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext executionContext)
        {
            log.LogInformation("Link Cryptonyms Custom Skill: C# HTTP trigger function processed a request.");

            string skillName = executionContext.FunctionName;
            IEnumerable<WebApiRequestRecord> requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
            if (requestRecords == null)
            {
                return new BadRequestObjectResult($"{skillName} - Invalid request record array.");
            }

            CryptonymLinker cryptonymLinker = new CryptonymLinker(executionContext.FunctionAppDirectory);
            WebApiSkillResponse response = WebApiSkillHelpers.ProcessRequestRecords(skillName, requestRecords,
                (inRecord, outRecord) => {
                    string word = inRecord.Data["word"] as string;
                    if (word.All(Char.IsUpper) && cryptonymLinker.Cryptonyms.TryGetValue(word, out string description))
                    {
                        outRecord.Data["cryptonym"] = new { value = word, description };
                    }
                    return outRecord;
                });

            return new OkObjectResult(response);
        }

        [FunctionName("link-cryptonyms-list")]
        public static IActionResult RunCryptonymLinkerForLists([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req,
            ILogger log,
            ExecutionContext executionContext)
        {
            log.LogInformation("Link Cryptonyms List Custom Skill: C# HTTP trigger function processed a request.");

            string skillName = executionContext.FunctionName;
            IEnumerable<WebApiRequestRecord> requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
            if (requestRecords == null)
            {
                return new BadRequestObjectResult($"{skillName} - Invalid request record array.");
            }

            CryptonymLinker cryptonymLinker = new CryptonymLinker(executionContext.FunctionAppDirectory);
            WebApiSkillResponse response = WebApiSkillHelpers.ProcessRequestRecords(skillName, requestRecords,
                (inRecord, outRecord) => {
                    var words = JsonConvert.DeserializeObject<JArray>(JsonConvert.SerializeObject(inRecord.Data["words"]));
                    var cryptos = words.Select(jword =>
                    {
                        var word = jword.Value<string>();
                        if (word.All(Char.IsUpper) && cryptonymLinker.Cryptonyms.TryGetValue(word, out string description))
                        {
                            return new { value = word, description };
                        }
                        return null;
                    });

                    outRecord.Data["cryptonyms"] = cryptos.ToArray();
                    return outRecord;
                });

            return new OkObjectResult(response);
        }
    }
}
