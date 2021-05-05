# Text PII Anonymization Power Skill

[Presidio](https://github.com/microsoft/presidio) is an open-source tool to recognize, analyze and anonymize personally identifiable information (PII). Using trained ML models, Presidio was built to ensure sensitive text is properly managed and governed.

This Power Skill uses [Presidio](https://github.com/microsoft/presidio) Analyzer and Anonymizer
to find and remove PII entities. Even though Presidio supports several anonymization methods (hash, encrypt, redact, replace, mask), 
this Power Skill only uses redact and removes the PIIs completely from the text  .

This skill is ideal for finding and removing PII entities from the search text.

Using the [PII detection custom skill](https://docs.microsoft.com/en-us/azure/search/cognitive-search-skill-pii-detection) can give you only some features Presidio offers.
Presidio could be customized for specific needs, either by [adding PII recognizers](https://microsoft.github.io/presidio/analyzer/adding_recognizers/) or [custom anonymizers](https://microsoft.github.io/presidio/anonymizer/adding_operators/).

⚠️ Presidio can help identify sensitive/PII data in un/structured text. However, because Presidio is using trained ML models, there is no guarantee that Presidio will find all sensitive information. Consequently, additional systems and protections should be employed.


### Deploy with docker
To run this PowerSkill you will need:
* Docker
* An Azure Blob storage container
* A provisioned Azure Cognitive Search (ACS) instance 
* A provisioned Azure Container Registry
* A Cognitive Services key in the region you deploy ACS to

Below is a full working example that you can get working E2E on sample data.

## How to implement

This section describes how to get this working on sample data and how it can be amended for your data.
 
1) ### Data
   The first step is to view the sample data. [Link to sample data](data/). 
1) ### [Run the power skill API](powerskill/app.py)
   The next step is to run the API locally and test the model against a test record. Create a local python environment
   and install the requirements:
   
   ```python
   python -m pip install -r powerskill/requirements.txt
   ```
   
    In addition to the common requirements described in the root [README.md](https://github.com/Azure-Samples/azure-search-power-skills/blob/master/README.md) 
    file, this Power Skill requires spacy en_core_web_lg module being downloaded:
    ```python
    python -m spacy download en_core_web_lg
    ```
   
   Activate your environment and run the API locally, execute the following:
   ```python 
   python app.py
   ``` 
   Run the cell 
   [Test PII anonymization on our local running API](notebooks/PII%20Anonymization.ipynb#Test-our-text-on-our-local-running-API). 
   Make sure you rename the file [sample_env file to .env](powerskill/sample_env) and populate it with the relevant values. Use the
   variable ```bash URL_LOCAL``` as the URL.
1) ### Build the docker image 
   Now build the [docker image](Dockerfile) and upload the image to your container registry  
   For this step you will need docker running so that we can build and test our Presidio inference API locally.
   You will also need a container registry for the build.

   Run the following command to build the inference API container image:

    ```bash
    docker build -t [container_registry_name.azurecr.io]/pii_anonymization:[your_tag] .  
    ```
    
    The container will require the following variables set at runtime, namely:
    
    ```bash
    KEY=[YourSecretKeyCanBeAnything]    # This is a secret key - only requests with this key will be allowed
    DEBUG=True   # This enables verbose logging
    ```
    See the file [sample_env](sample_env) for the .env format
    
    Now we can test the container by running it locally with our variables:
    
    ```bash
    docker run -it --rm -p 5000:5000 -e DEBUG=true -e KEY=[YourSecretKeyCanBeAnything] 
    [container_registry_name.azurecr.io]/pii_anonymization:[your_tag]
    ```
    Upon starting, you will see the server initializing:
    ```bash
    INFO:     Uvicorn running on http://0.0.0.0:5000 (Press CTRL+C to quit)
    ```
    We are now ready to send a request. Run the cell 
    [Test PII anonymization on local running API](notebooks/PII%20Anonymization.ipynb#Test-our-text-on-our-local-running-API) to test
    the running container.
    
    The response should show the anonymized text. 
    
1) ### Deploy the container to an Azure Web App.

    We will deploy this as an [Azure App Service Web App](https://docs.microsoft.com/en-us/azure/app-service/configure-custom-container?pivots=container-linux).
    running a container.
    
    First we need to push our newly built image to our container registry.
    
    Run the following command:
    ```bash
    docker push [container_registry_name].azurecr.io/pii_anonymization:[your_tag]
    ```
    
    In the [deployment folder](deployment/webapp) there are two [terraform](https://www.terraform.io/)
    files to deploy the inference API to an App Service Web App for linux.
    
    The simplest is to open a [cloud shell in the portal](https://ms.portal.azure.com/#home) and upload
    the [main](deployment/webapp/main.tf) and [variables](deployment/webapp/variables.tf)
    to your cloud shell storage as this avoids the need for any installation. 
    
    Set the following values in the [main](deployment/webapp/main.tf) file:
    ```hcl-terraform
    backend "azurerm" {
        storage_account_name = "[your storage account name]"
        container_name = "[your storage container name]"
        key = "[your storage account key]"
        resource_group_name = "[your storage account resource group name]"
      }
    ```
    
    Set the following values in the [variables](deployment/webapp/variables.tf)
    file:
    
    ```bash
    variable "app_service_sku" {
      description = "The SKU (size - cpu/mem) of the app plan hosting the container. See: https://azure.microsoft.com/en-us/pricing/details/app-service/linux/"
      default = "P2V2"
    }
    
    variable "docker_registry_url" {
      description = "[your container registry].azurecr.io"
      default = ""
    }
    
    variable "docker_registry_username" {
      description = "[your container registry username]"
      default = ""
    }
    
    variable "docker_registry_password" {
      description = "[your container registry password]"
      default = ""
    }
    
    variable "docker_image" {
      description = "[your docker image name]:[your tag]"
      default = ""
    }
    
    variable "resource_group" {
      description = "This is the name of an existing resource group to deploy to"
      default = ""
    }
    
    variable "location" {
      description = "This is the region of an existing resource group you want to deploy to"
      default = "eastus2"
    }
    
    variable "debug" {
      description = "API logging - set to True for verbose logging"
      default = false
    }

    ```
    
    Navigate to the directory containing the files and enter:
    
    ```bash
    terraform init
    ```
    Then enter:
    ```bash
    terraform apply
    ```
    You will be prompted with:
    
    ```bash
    Do you want to perform these actions?
      Terraform will perform the actions described above.
      Only 'yes' will be accepted to approve.
    ```
    
    Type `yes`
    
    Once deployed, copy the Azure Web App URL which may be found in the overview section of the portal as we will need 
    it to plug into Azure Search.
    
1) ### Deploy the datasource, index, skillset and indexer

   #### Data source
   
    Populate your values in the [data source file](deployment/azuresearch/create_data_source.json) or use the 
    ['Create the data source'](notebooks/PII%20Anonymization.ipynb#Create-the-data-source) script

    #### Index
    Populate your values in the [index file](deployment/azuresearch/create_index.json) or use the 
    ['Create the index'](notebooks/PII%20Anonymization.ipynb#Now-we-create-the-index) script
    
    #### Skillset
    
    Populate the values in the [skillset file](deployment/azuresearch/create_skillset.json) or use the 
    ['Create the SkillSet'](notebooks/PII%20Anonymization.ipynb#Now-we-create-the-skill-set) script
      
    Note, you need an already deployed ACS instance in the same region as your cognitive services
    instance as we want to augment what we can extract using custom vision with our similarity
    model.
    
    You will need your [ACS API Key](https://docs.microsoft.com/en-us/azure/search/search-security-api-keys)
    and the URL for your ACS instance. 
   
1) ### Run the ACS indexer 

    Populate the values in the [indexer file](deployment/azuresearch/create_indexer.json) or 
    use the ['Create/Run your indexer'](notebooks/PII%20Anonymization.ipynb#Now-we-create-the-indexer) script

    The indexer will automatically run. You should see requests coming in if you look at the Web App logs.

1)  ### Test the index 
    When looking at your data, you will now see the imported text data without PII entities in it.
    
    Now we are in a position to search on our most similar data, navigate to the ['Let's go and test the ACS index'](notebooks/PII%20Anonymization.ipynb#Let's-go-and-test-the-ACS-index)
    to view the anonymized text.
