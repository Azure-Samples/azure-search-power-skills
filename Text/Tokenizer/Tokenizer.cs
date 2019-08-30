// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AzureCognitiveSearch.PowerSkills.Common;
using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Transforms.Text;
using Microsoft.ML.Data;
using System;
using static Microsoft.ML.Transforms.Text.StopWordsRemovingEstimator.Language;

namespace AzureCognitiveSearch.PowerSkills.Text.Tokenizer
{
    public static class Tokenizer
    {
        [FunctionName("tokenizer")]
        public static IActionResult RunTokenizer(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext executionContext)
        {
            log.LogInformation("Tokenizer Custom Skill: C# HTTP trigger function processed a request.");

            string skillName = executionContext.FunctionName;
            IEnumerable<WebApiRequestRecord> requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
            if (requestRecords == null)
            {
                return new BadRequestObjectResult($"{skillName} - Invalid request record array.");
            }

            WebApiSkillResponse response = WebApiSkillHelpers.ProcessRequestRecords(skillName, requestRecords,
                (inRecord, outRecord) =>
                {
                    var text = new TextData { Text = inRecord.Data["text"] as string };
                    StopWordsRemovingEstimator.Language language =
                        MapToMlNetLanguage(inRecord.Data.TryGetValue("languageCode", out object languageCode) ? languageCode as string : "en");

                    var mlContext = new MLContext();
                    IDataView emptyDataView = mlContext.Data.LoadFromEnumerable(new List<TextData>());
                    EstimatorChain<StopWordsRemovingTransformer> textPipeline = mlContext.Transforms.Text
                        .NormalizeText("Text", caseMode: TextNormalizingEstimator.CaseMode.Lower, keepDiacritics: true, keepPunctuations: false, keepNumbers: false)
                        .Append(mlContext.Transforms.Text.TokenizeIntoWords("Words", "Text", separators: new[] { ' ' }))
                        .Append(mlContext.Transforms.Text.RemoveDefaultStopWords("Words", language: language));
                    TransformerChain<StopWordsRemovingTransformer> textTransformer = textPipeline.Fit(emptyDataView);
                    PredictionEngine<TextData, TransformedTextData> predictionEngine = mlContext.Model.CreatePredictionEngine<TextData, TransformedTextData>(textTransformer);

                    outRecord.Data["words"] = predictionEngine.Predict(text).Words ?? Array.Empty<string>();
                    return outRecord;
                });

            return new OkObjectResult(response);
        }

        private static StopWordsRemovingEstimator.Language MapToMlNetLanguage(string languageCode)
        {
            switch(languageCode.Trim().ToLowerInvariant())
            {
                case "ar": return Arabic;
                case "cs": return Czech;
                case "da": return Danish;
                case "de": return German;
                case "es": return Spanish;
                case "fr": return French;
                case "it": return Italian;
                case "jp": return Japanese;
                case "nb": return Norwegian_Bokmal;
                case "nl": return Dutch;
                case "pl": return Polish;
                case "pt": return Portuguese;
                case "sv": return Swedish;
                case "ru": return Russian;
                default: return English;
            }
        }

        private class TextData
        {
            public string Text { get; set; }
        }

        private class TransformedTextData
        {
            public string[] Words { get; set; }
        }
    }
}
