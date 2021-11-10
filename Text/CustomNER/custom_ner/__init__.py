import logging
import azure.functions as func
import json, requests, time

def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')

    try:
        body = json.dumps(req.get_json())
    except ValueError:
        return func.HttpResponse(
             "Invalid body",
             status_code=400
        )
    
    if body:
        result = compose_response(body)
        return func.HttpResponse(result, mimetype="application/json")
    else:
        return func.HttpResponse(
             "Invalid body",
             status_code=400
        )

def compose_response(json_data):
    values = json.loads(json_data)['values']

    # Prepare the Output before the loop
    results = {}
    results["values"] = []

    for value in values:
        outputRecord = transform_value(value)
        if outputRecord != None:
            results["values"].append(outputRecord)
    return json.dumps(results, ensure_ascii=False)

def transform_value(value):
    try:
        recordId = value['recordId']
    except AssertionError  as error:
        return None

    # Validate the inputs
    try:         
        assert ('data' in value), "'data' field is required."
        data = value['data']        
        assert ('lang' in data), "'lang' language field is required in 'data' object."        
        assert ('text' in data), "'text' corpus field is required in 'data' object."
    except AssertionError  as error:
        return (
            {
            "recordId": recordId,
            "data":{},
            "errors": [ { "message": "Error:" + error.args[0] }   ]
            })

    try:
        result = get_entities (value)

    except:
        return (
            {
            "recordId": recordId,
            "errors": [ { "message": "Could not complete operation for record." }   ]
            })

    return ({
            "recordId": recordId,
            "data": {
                "text": result
                    }
            })

# Function to submit the analysis job towards the Text Analytics (TA) API
def get_entities (value):
    language = str(value['data']['lang'])
    corpus = str(value['data']['text'])
    #To be filled with your Text Analytics details: endpoint, key, deployment name and project name inside your appsettings (https://docs.microsoft.com/en-us/azure/azure-functions/functions-app-settings)
    endpoint = os.environ["TA_ENDPOINT"] # This will look like 'https://westeurope.api.cognitive.microsoft.com/text/analytics/v3.2-preview.2/analyze' to be updated once product goes GA
    key = os.environ["TA_KEY"]
    project_name = os.environ["PROJECT_NAME"]
    deployment =  os.environ["DEPLOYMENT"]
    body = "{'displayName': 'Extracting custom NERS', 'analysisInput': {'documents': [{'id': '1', 'language': '" + language + "', 'text':'" + corpus + "'}]}, 'tasks': {'customEntityRecognitionTasks': [{'parameters': {'project-name': '"+ project_name +"','deployment-name': '"+ deployment +"'}}]}}"
    header = {'Ocp-Apim-Subscription-Key': key}
    #TA Custom NER API works in two steps, first you post the job, afterwards you get the result
    response_job = requests.post(endpoint, data = body, headers = header)
    time.sleep(2)
    jobid = response_job.headers["operation-location"].partition('jobs/')[2]
    response = requests.get(endpoint+'/jobs/'+jobid, None, headers=header)
    dict=json.loads(response.text)
    #sometimes the TA processing time will be longer, in that case we need to try again after a while
    if (dict['status']!='suceeded'):
        time.sleep(2)
        response = requests.get(endpoint+'/jobs/'+jobid, None, headers=header)
        dict=json.loads(response.text)    
    entities=dict['tasks']['customEntityRecognitionTasks'][0]['results']['documents'][0]['entities'][0] 
    #for your reference, output of the TA Cognitive Service is like {"jobId":"eba___","lastUpdateDateTime":"2021-11-07T18:09:37Z","createdDateTime":"2021-11-07T18:09:37Z","expirationDateTime":"2021-11-08T18:09:37Z","status":"succeeded","errors":[],"displayName":"Extracting custom NERS","tasks":{"completed":1,"failed":0,"inProgress":0,"total":1,"customEntityRecognitionTasks":[{"lastUpdateDateTime":"2021-11-07T18:09:37.8390261Z","state":"succeeded","results":{"documents":[{"id":"1","entities":[{"text":"$192,989.00)","category":"Quantity","offset":508,"length":12,"confidenceScore":1.0}],"warnings":[]}],"errors":[],"projectName":"y","deploymentName":"z"}}]}}
    #after the filtering we just get {'text': '$192,989.00)', 'category': 'Quantity', 'offset': 508, 'length': 12, 'confidenceScore': 1.0}
    return entities
