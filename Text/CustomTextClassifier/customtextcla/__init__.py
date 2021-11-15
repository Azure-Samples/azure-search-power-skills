import azure.functions as func
import json, requests, time, os, logging
#from http.client import HTTPConnection

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
        result = get_classifications (value)

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
def get_classifications (value):
    # # Debug logging, useful if you struggle with the body sent to the endpoint. Uncomment from http.client too 
    # log = logging.getLogger('urllib3')
    # log.setLevel(logging.DEBUG)
    # # logging from urllib3 to console
    # ch = logging.StreamHandler()
    # ch.setLevel(logging.DEBUG)
    # log.addHandler(ch)
    # # print statements from `http.client.HTTPConnection` to console/stdout
    # HTTPConnection.debuglevel = 1 

    language = str(value['data']['lang'])
    corpus = str(value['data']['text'])
    #To be filled with your Text Analytics details: endpoint, key, deployment name and project name inside your appsettings (https://docs.microsoft.com/en-us/azure/azure-functions/functions-app-settings)
    endpoint = os.environ["TA_ENDPOINT"] # This will look like 'https://westeurope.api.cognitive.microsoft.com/text/analytics/v3.2-preview.2/analyze' to be updated once product goes GA
    key = os.environ["TA_KEY"]
    project_name = os.environ["PROJECT_NAME"]
    deployment =  os.environ["DEPLOYMENT"]
    body = "{'displayName': 'Extracting custom text classification', 'analysisInput': {'documents': [{'id': '1', 'language': '" + language + "', 'text':'" + corpus + "'}]}, 'tasks': {'customMultiClassificationTasks': [{'parameters': {'project-name': '"+ project_name +"','deployment-name': '"+ deployment +"'}}]}}"
    header = {'Ocp-Apim-Subscription-Key': key}
    #TA Custom NER API works in two steps, first you post the job, afterwards you get the result
    response_job = requests.post(endpoint, data = body, headers = header)
    #print ('response is: ', response_job.headers)
    time.sleep(2)
    jobid = response_job.headers["operation-location"].partition('jobs/')[2]
    response = requests.get(endpoint+'/jobs/'+jobid, None, headers=header)
    dict=json.loads(response.text)
    #sometimes the TA processing time will be longer, in that case we need to try again after a while
    if (dict['status']!='suceeded'):
        time.sleep(3)
        response = requests.get(endpoint+'/jobs/'+jobid, None, headers=header)
        #print ('response is: ', response.text)
        dict=json.loads(response.text)    
    classifications=dict['tasks']['customMultiClassificationTasks'][0]['results']['documents'][0]['classifications']
    #for your reference, output of the TA Cognitive Service is like {  "jobId": "9x",  "lastUpdateDateTime": "2021-11-14T11:45:09Z",  "createdDateTime": "2021-11-14T11:45:08Z",  "expirationDateTime": "2021-11-15T11:45:08Z",  "status": "succeeded",  "errors": [],  "displayName": "Extracting custom text classification",  "tasks": {    "completed": 1,    "failed": 0,    "inProgress": 0,    "total": 1,    "customMultiClassificationTasks": [{      "lastUpdateDateTime": "2021-11-14T11:45:09.4757945Z",      "state": "succeeded",      "results": {        "documents": [{          "id": "1",          "classifications": [{            "category": "Action",            "confidenceScore": 0.77          }, {            "category": "Drama",            "confidenceScore": 1.0          }, {            "category": "Mystery",            "confidenceScore": 0.9          }],          "warnings": []        }],        "errors": [],        "projectName": "p",        "deploymentName": "prod"      }    }]  }}
    #after the filtering we just get {"text": [{"category": "Action","confidenceScore": 0.77},{"category": "Drama","confidenceScore": 1.0}]}
    return classifications
