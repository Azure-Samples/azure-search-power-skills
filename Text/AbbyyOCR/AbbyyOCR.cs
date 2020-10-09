// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AzureCognitiveSearch.PowerSkills.Common;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Abbyy.CloudSdk.V2.Client.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Abbyy.CloudSdk.V2.Client;
using Abbyy.CloudSdk.V2.Client.Models.RequestParams;
using Abbyy.CloudSdk.V2.Client.Models.Enums;
using System.Net;
using Polly.Extensions.Http;
using Polly;
using System.Collections.Concurrent;

namespace AbbyyOCR
{
    public static class AbbyyOCR
    {
		private static readonly string ApplicationId = @"[Enter ABBYY Application ID]";
		private static readonly string Password = @"[Enter ABBYY OCR Password]";
		private static readonly string ServiceUrl = "[Enter ABBYY Service URL such as https://cloud-westus.ocrsdk.com]";

        private static int _retryCount = 3;
        private static int _delayBetweenRetriesInSeconds = 3;
        private static string _httpClientName = "OCR_HTTP_CLIENT";

		private static readonly string firstFilePath = "processImage.jpg";
		private static readonly string secondFilePath = "processDocument.jpg";

		private static ConcurrentBag<WebApiResponseRecord> bag = new ConcurrentBag<WebApiResponseRecord>();

		private static readonly AuthInfo AuthInfo = new AuthInfo
        {
            Host = ServiceUrl,
            ApplicationId = ApplicationId,
            Password = Password
        };

        private static ServiceProvider _serviceProvider;
        private static HttpClient _httpClient;

        [FunctionName("AbbyyOCR")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext executionContext)

        {
            log.LogInformation("ABBYY OCR Custom Skill: C# HTTP trigger function processed a request.");

            string skillName = executionContext.FunctionName;
            IEnumerable<WebApiRequestRecord> requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
            if (requestRecords == null)
            {
                return new BadRequestObjectResult($"{skillName} - Invalid request record array.");
            }

			// Create the response record
			WebApiSkillResponse response = new WebApiSkillResponse();

			using (var ocrClient = GetOcrClientWithRetryPolicy())
			{
				List<Task> TaskList = new List<Task>();
				int idx = 0;
				foreach (var record in requestRecords)
				{
					TaskList.Add(ProcessRecord(record, ocrClient, idx));
					idx += 1;
				}
				Task.WaitAll(TaskList.ToArray());

				// Apply all the records from the concurrent bag
				foreach (var record in bag)
				{
					response.Values.Add(record);
				}
			}

            return new OkObjectResult(response);

        }

		public static async Task ProcessRecord(WebApiRequestRecord record, IOcrClient ocrClient, int idx)
		{
			WebApiResponseRecord waRecord = new WebApiResponseRecord();

			record.Data.TryGetValue("formUrl", out object imgFile);
			record.Data.TryGetValue("formSasToken", out object sasToken);
			string imgFileWithSaS = imgFile.ToString() + sasToken.ToString();
			string fileType = Path.GetExtension(imgFile.ToString());
			string localFile = Path.Combine(Path.GetTempPath(), "temp_" + idx.ToString() + fileType);

			try
			{
				using (var client = new WebClient())
				{
					client.DownloadFile(imgFileWithSaS, localFile);
				}

				// Process image 
				// You could also call ProcessDocumentAsync or any other processing method declared below
				var resultUrls = await ProcessImageAsync(ocrClient, localFile);

				//Get results - the first doc is a docx, second is a text file
				using (var client = new WebClient())
				{
					if (resultUrls.Count >= 1)
					{
						waRecord.Data.Add("content", client.DownloadString(resultUrls[1].ToString()));
					}
				}
			}
			finally
			{
				File.Delete(localFile);
				waRecord.RecordId = record.RecordId;
				bag.Add(waRecord);
			}
		}

		private static IOcrClient GetOcrClient()
		{
			return new OcrClient(AuthInfo);
		}

		private static IOcrClient GetOcrClientWithRetryPolicy()
		{
			// Create service collection and configure our services
			var services = ConfigureServices();
			// Generate a provider
			_serviceProvider = services.BuildServiceProvider();

			var httpClientFactory = _serviceProvider.GetService<IHttpClientFactory>();
			_httpClient = httpClientFactory.CreateClient(_httpClientName);

			return new OcrClient(_httpClient);
		}

		private static ServiceCollection ConfigureServices()
		{
			var services = new ServiceCollection();

			//Configure HttpClientFactory with retry handler
			services.AddHttpClient(_httpClientName, conf =>
			{
				conf.BaseAddress = new Uri(AuthInfo.Host);
				//increase the default value of timeout for the duration of retries
				conf.Timeout = conf.Timeout + TimeSpan.FromSeconds(_retryCount * _delayBetweenRetriesInSeconds);
			})
				.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
				{
					PreAuthenticate = true,
					Credentials = new NetworkCredential(AuthInfo.ApplicationId, AuthInfo.Password)
				})
				//Add  custom HttpClientRetryPolicyHandler with polly
				.AddHttpMessageHandler(() => new HttpClientRetryPolicyHandler(GetRetryPolicy()));

			//or you can use Microsoft.Extensions.DependencyInjection Polly extension
			//.AddPolicyHandler(GetRetryPolicy());
			return services;
		}

		private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
		{
			return HttpPolicyExtensions.HandleTransientHttpError()
				//Condition - what kind of request errors should we repeat
				.OrResult(r => r.StatusCode == HttpStatusCode.GatewayTimeout)
				.WaitAndRetryAsync(
					_retryCount,
					sleepDurationProvider => TimeSpan.FromSeconds(_delayBetweenRetriesInSeconds),
					(exception, calculatedWaitDuration, retries, context) =>
					{
						Console.WriteLine($"Retry {retries} for policy with key {context.PolicyKey}");
					}
				)
				.WithPolicyKey("WaitAndRetryAsync_For_GatewayTimeout_504__StatusCode");
		}

		private static async Task<List<string>> ProcessImageAsync(IOcrClient ocrClient, string localFile)
		{
			var imageParams = new ImageProcessingParams
			{
				ExportFormats = new[] { ExportFormat.Docx, ExportFormat.Txt, },
				Language = "English,Arabic,Hebrew",
			};

			using (var fileStream = new FileStream(localFile, FileMode.Open))
			{
				var taskInfo = await ocrClient.ProcessImageAsync(
					imageParams,
					fileStream,
					Path.GetFileName(localFile),
					waitTaskFinished: true);

				return taskInfo.ResultUrls;
			}
		}

		private static async Task<List<string>> ProcessDocumentAsync(IOcrClient ocrClient)
		{
			var taskId = await UploadFilesAsync(ocrClient);

			var processingParams = new DocumentProcessingParams
			{
				ExportFormats = new[] { ExportFormat.Docx, ExportFormat.Txt, },
				Language = "English,Arabic,Hebrew",
				TaskId = taskId,
			};

			var taskInfo = await ocrClient.ProcessDocumentAsync(
				processingParams,
				waitTaskFinished: true);

			return taskInfo.ResultUrls;
		}

		private static async Task<Guid> UploadFilesAsync(IOcrClient ocrClient)
		{
			ImageSubmittingParams submitParams;

			// First file
			using (var fileStream = new FileStream(firstFilePath, FileMode.Open))
			{
				var submitImageResult = await ocrClient.SubmitImageAsync(
					null,
					fileStream,
					Path.GetFileName(firstFilePath));

				// Save TaskId for next files and ProcessDocument method
				submitParams = new ImageSubmittingParams { TaskId = submitImageResult.TaskId };
			}

			// Second file
			using (var fileStream = new FileStream(secondFilePath, FileMode.Open))
			{
				await ocrClient.SubmitImageAsync(
					submitParams,
					fileStream,
					Path.GetFileName(secondFilePath));
			}

			return submitParams.TaskId.Value;
		}

		private static async Task<TaskList> GetFinishedTasksAsync(IOcrClient ocrClient)
		{
			var finishedTasks = await ocrClient.ListFinishedTasksAsync();
			return finishedTasks;
		}

		private static void DisposeServices()
		{
			(_serviceProvider as IDisposable)?.Dispose();
		}
	}
}
