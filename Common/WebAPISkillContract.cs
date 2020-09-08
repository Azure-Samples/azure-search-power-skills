// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using System.Collections.Generic;

namespace AzureCognitiveSearch.PowerSkills.Common
{
    public class WebApiSkillRequest
    {
        public List<WebApiRequestRecord> Values { get; set; } = new List<WebApiRequestRecord>();
    }

    public class WebApiSkillResponse
    {
        public List<WebApiResponseRecord> Values { get; set; } = new List<WebApiResponseRecord>();
    }

    public class WebApiRequestRecord
    {
        public string RecordId { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }

    public class WebApiResponseRecord
    {
        public string RecordId { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
        public List<WebApiErrorWarningContract> Errors { get; set; } = new List<WebApiErrorWarningContract>();
        public List<WebApiErrorWarningContract> Warnings { get; set; } = new List<WebApiErrorWarningContract>();
    }

    public class WebApiErrorWarningContract
    {
        public string Message { get; set; }
    }

    public class FileReference
    {
        public string data { get; set; }
    }
}