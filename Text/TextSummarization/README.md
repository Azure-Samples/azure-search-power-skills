# Text Summarization Power Skill #

This Power Skill uses the pre-trained [HuggingFace/Facebook BART model](https://huggingface.co/facebook/bart-large-cnn)
deep learning model to generate a summary for an input text - English only.

This skill is ideal for:

1) Exploring your data to summarize and categorise your documents in the data exploration phase
2) Using in conjunction with Azure Text Analytics or a Named Entity Recognition (NER) model and Latent Dirichlet 
Allocation (LDA) topic modelling to further identify areas of interest

See the data folder for sample texts used in the skill. This example uses the
[Meantime News dataset](http://www.newsreader-project.eu/results/data/wikinews/).

## Requirements

In addition to the common requirements described in the root [README.md](https://github.com/Azure-Samples/azure-search-power-skills/blob/master/README.md) 
file, this Power Skill requires access to a Text Analytics resource. This process will use NER to illustrate 
the entities extracted and its relevance to the summarized text.

To run this PowerSkill you will need:
* docker
* An Azure Blob storage container
* A provisioned Azure Cognitive Search (ACS) instance 
* A provisioned Azure Container Registry
* A Cognitive Services key in the region you deploy ACS to

Below is a full working example that you can get working end to end on sample data.

## High level Process

![text summarization process](images/text_summarization_flow.png)

## How to implement

This section describes how to get this working on sample data and how it can be amended for your data.
 
1) ###Data
   The first step is to view the sample data files here [train data](data/). 
2) ###Run the API
   The next step is to run the API locally and test the model against a test record. Create a local python environment
   and install the requirements:
   ```pythonn
      python -m pip install -r requirements.txt
   ```
   Activate your environment and run the API locally, execute the following:
   ```python 

   python app.py
   ``` 
   Run the cell 
   [Test summarization on our local running API](notebooks/Text%20Summarisation.ipynb#Test-our-text-on-our-local-running-API). 
   Make sure you rename the file [sample_env file to .env](sample_env) and populate it with the relevant values. Use the
   variable ```bash URL_LOCAL``` as the URL.
3) ###Build the docker image 
   Now build the [docker image](Dockerfile) and upload the image to your container registry  
   For this step you will need docker running so that we can build and test our inference API locally.
   You will also need a container registry for the build.
   
   Note, the [HuggingFace/Facebook BART model](https://huggingface.co/facebook/bart-large-cnn) is 1.6Gb so if you have
   a slow connection, skip to the 'Deploy the container to an Azure Web App step'.

   Run the following command to build the inference API container image:

    ```bash
    docker build -t [container_registry_name.azurecr.io/text_summarization_extractor:[your_tag] .  
    ```
    
    The container will require the following variables set at runtime, namely:
    
    ```bash
    KEY=[YourSecretKeyCanBeAnything]    # This is a secret key - only requests with this key will be allowed
    DEBUG=True   # This enables verbose logging
    NUM_BEAMS=4  # This is the number beams to use during the beam search
    MAX_LENGTH=1024  # This is the maximum length of the summary
    ```
    See the file [sample_env](deployment/sample_env) for the .env format
    
    Now we can test the container by running it locally with our variables:
    
    ```bash
    docker run -it --rm -p 5000:5000 -e DEBUG=true -e KEY=[YourSecretKeyCanBeAnything] 
    -e SUMMARIZER_MODEL=facebook/bart-large-cnn -e MAX_LENGTH=1024 -e NUM_BEAMS=4 
    [container_registry_name.azurecr.io/text_summarization_extractor:[your_tag]
    ```
    Upon starting you will see the download of the BART model initiate. See below for example:
    
    ```bash
    DEBUG:urllib3.connectionpool:https://cdn-lfs.huggingface.co:443 "GET /facebook/bart-large-cnn/2ac2745c02ac987d82c78a14b426de58d5e4178ae8039ba1c6881eccff3e82f1 HTTP/1.1" 200 1625270765
    Downloading:   1%|███▉                      
    ```
    You should also see the following:
    
    ```bash
    INFO:uvicorn.error:Uvicorn running on http://0.0.0.0:5000 (Press CTRL+C to quit)
    ```
    We are now ready to send a request. Run the cell 
    [Test summarization on local running API](notebooks/Text%20Summarisation.ipynb#Test-our-text-on-our-local-running-API) to test
    the running container.
    
    After issuing the above request you should get a response showing the full and summarized text. 
    
4) ###Deploy the container to an Azure Web App.

    We will deploy this as an [Azure App Service Web App](https://docs.microsoft.com/en-us/azure/app-service/configure-custom-container?pivots=container-linux).
    running a container.
    
    First we need to push our newly built image to our container registry.
    
    Run the following command:
    ```bash
    docker push [container_registry_name].azurecr.io/text_summarization_extractor:[your_tag]
    ```
    
    In the [deployment folder](deployment/webapp) are two [terraform](https://www.terraform.io/)
    files to deploy the inference API to an App Service Web App for linux.
    
    The simplest is to open a cloud [cloud shell](https://shell.azure.com/) and upload
    the [main](deployment/webapp/main.tf) and [variables](deployment/webapp/variables.tf)
    to your cloud shell storage as this avoids the need for any installation. 
    
    Set the following values in the [main](deployment/webapp/main.tf) file:
    ```hcl-terraform
    backend "azurerm" {
        storage_account_name = "[your storage account name"
        container_name = "[your storage container name]"
        key = "[your storage account key"
        resource_group_name = "[your storage account resource group name]"
      }
    ```
    
    Set the following values in the[variables](deployment/webapp/variables.tf)
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
    
    variable "num_beams" {
      description = "Set this to the number of beams to use for beam search"
      default = 4
    }
    
    variable "max_length" {
      description = "The maximum length of the summary"
      default = 1024
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
    
    Type ```bash yes```
    
    Once deployed, copy the Azure Web App URL which may be found in the overview section of the portal as we will need 
    it to plug into Azure Search.
    
5) ###Deploy the datasource, index, skillset and indexer

   #### Data source
   
    Populate your values in the [data source file](deployment/azuresearch/create_data_source.json) or use the 
    [Create the data source](notebooks/Text%20Summarisation.ipynb#Create-the-data-source)

    #### Index
    Populate your values in the [index file](deployment/azuresearch/create_index.json) or use the 
    [Create the index](notebooks/Text%20Summarisation.ipynb#Now-we-create-the-index)
    
    #### Skillset
    
    Populate the values in the [skillset file](deployment/azuresearch/create_skillset.json) or use the 
    [Create the SkillSet](notebooks/Text%20Summarisation.ipynb#Now-we-create-the-skill-set)
      
    Note, you need an already deployed ACS instance in the same region as your cognitive services
    instance as we want to augment what we can extract using custom vision with our similarity
    model.
    
    You will need your [ACS API Key](https://docs.microsoft.com/en-us/azure/search/search-security-api-keys)
    and the URL for your ACS instance. 
   
6) ###Run the ACS indexer 

    Populate the values in the [indexer file](deployment/azuresearch/create_indexer.json) or 
    [Create/Run your indexer](notebooks/Text%20Summarisation.ipynb#Now-we-create-the-indexer)

    The indexer will automatically run and you should see requests coming in if you look at the Web App logs.

7)  ###Test the index 
    Investigate your indexed data, check the most similar images

    Now we are in a position to search on our most similar data, navigate to the [Let's go and test the ACS index](notebooks/Text%20Summarisation.ipynb#Let's-go-and-test-the-ACS-index)
    to summarise our text and also run NER.