# Power Skill API

Sample PowerSkill API

## Environment variables

The following sample environment variables need to be set for the process to work:

```bash       
"TR_REGION": "westeurope", # Translator region
"DEBUG": "True", # Enable process debugging
"FLASK_DEBUG": "false", # Flask native debugging keep False
"FLASK_ENV": "development", # Toggle between development and production
```

## API Request/Response

### API Inputs POST

```json
{
    "values": [
      {
        "recordId": "100003490593495",
        "data":
           {
             "fileContent": "[some content]",
             "correlationId": "123233434334334",
             "batch": "4535346534654654"
           }
      }
    ]
}
```

### API Outputs

```json
{
    "values": [
        {
            "recordId": "100003490593495",
            "correlationId": "123233434334334",
            "batch": "4535346534654654",
            "errors": "",
            "data": {},
            "warnings": ""
        }
    ]
}
```

## Basic Authentication

The API will perform a simple check to determine whether the KEY Header has been set.

It will validate that the value passed as a header to call this API, namely:

```bash
Ocp-Apim-Subscription-Key: [KEY]
```
## Normal start

To start the application for normal usage, run the following command:

```bash
uvicorn app:app --reload --port 5000
```

## Build and Test

The majority of steps necessary to get you up and running are already done by the dev container. But this project uses the following:

- Python
- Pip

Once your container is up and running you should:

1. Open your test `.py` file (```tests/powerskill_api_test.py```) and set the Python interpreter to be your venv (bottom blue bar of VSCode)
2. Use the python test explorer plugin to run your tests or click the 'run test' prompt above your tests

## Deploy to Azure

The infra folder contains terraform files to deploy this to FastAPI container to Web 
Apps for Linux running a Docker container.

## Remote SSH debugging

To enable the ```SSH``` connection for development debugging if deployed to Azure Web Apps, deploy the file [```Dockerfile_debug```](containers/Dockerfile_debug)
which will enable the Azure Web App to bridge a connection to the running docker instance. See the [Enable SSH](https://docs.microsoft.com/en-gb/azure/app-service/configure-custom-container?pivots=container-linux#enable-ssh)
for more info. This is useful for inspecting running processes and checking model binaries are deployed correctly.

The files [```ssdh_config```](containers/sshd_config) and [```startup.sh```](containers/startup.sh) are used only for this debugging 
```Dockerfile_debug```. 

### Connecting to the container

* Once deployed, select the ```ssh``` option in the Azure portal on the web app
* Click ```Go```
* You should see green message at the bottom of the screen with ```SSH CONNECTION ESTABLISHED``` if successful
* The terminal session should then be available for input
