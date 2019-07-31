using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AzureCognitiveSearch.PowerSkills.Common;
using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Transforms.Text;
using System;
using Microsoft.ML.Data;

namespace Tokenizer
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

            var mlContext = new MLContext();
            IDataView emptyDataView = mlContext.Data.LoadFromEnumerable(new List<TextData>());
            EstimatorChain<StopWordsRemovingTransformer> textPipeline = mlContext.Transforms.Text
                .NormalizeText("Text", caseMode: TextNormalizingEstimator.CaseMode.Lower, keepDiacritics: true, keepPunctuations: false, keepNumbers: false)
                .Append(mlContext.Transforms.Text.TokenizeIntoWords("Words", "Text", separators: new[] { ' ' }))
                .Append(mlContext.Transforms.Text.RemoveDefaultStopWords("Words", language: StopWordsRemovingEstimator.Language.English));
            TransformerChain<StopWordsRemovingTransformer> textTransformer = textPipeline.Fit(emptyDataView);
            PredictionEngine<TextData, TransformedTextData> predictionEngine = mlContext.Model.CreatePredictionEngine<TextData, TransformedTextData>(textTransformer);

            WebApiSkillResponse response = WebApiSkillHelpers.ProcessRequestRecords(skillName, requestRecords,
                (inRecord, outRecord) =>
                {
                    var text = new TextData { Text = inRecord.Data["text"] as string };
                    outRecord.Data["words"] = predictionEngine.Predict(text).Words;
                    return outRecord;
                });

            return new OkObjectResult(response);
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
