using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AzureCognitiveSearch.PowerSkills.Common;
using Microsoft.Extensions.Configuration;

namespace AzureCognitiveSearch.PowerSkills.Utils.GetFileExtension
{
	public static class GetFileExtension
	{
		[FunctionName("get-file-extension")]
		public static IActionResult RunGetFileExtension(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
			ExecutionContext executionContext, ILogger log)
		{
			var config = new ConfigurationBuilder()
				.SetBasePath(executionContext.FunctionAppDirectory)
				.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();

			string skillName = executionContext.FunctionName;
			log.LogInformation($"{skillName} Custom Skill: C# HTTP trigger function processed a request.");

			var requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
			if (requestRecords == null)
			{
				return new BadRequestObjectResult($"{skillName} - Invalid request record array.");
			}

			var response = WebApiSkillHelpers.ProcessRequestRecords(skillName, requestRecords,
				(inRecord, outRecord) =>
				{
					try
					{
						var documentName = inRecord.Data["documentName"] as string;
						var extension = "";
						var fileName = "";

						if (!string.IsNullOrWhiteSpace(documentName))
						{
							extension = Path.GetExtension(documentName);
							fileName = Path.GetFileNameWithoutExtension(documentName);
						}

						outRecord.Data.Add("extension", extension);
						outRecord.Data.Add("fileName", fileName);
					}
					catch (Exception e)
					{
						outRecord.Errors.Add(new WebApiErrorWarningContract { Message = e.ToString() });
					}

					return outRecord;
				});

			return new OkObjectResult(response);
		}
	}
}
