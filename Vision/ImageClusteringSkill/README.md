# Image Clustering Power Skill #

This Power Skill uses the [DBSCAN](https://scikit-learn.org/stable/modules/generated/sklearn.cluster.dbscan.html)
unsupervised clustering algorithm alongside [VGG16](https://keras.io/api/applications/vgg/) to extract
visual features and cluster images. 

This skill is ideal for:

1) Exploring your data to identify clusters based on visual features during your data exploration phase
2) Using in conjunction with Custom Vision Classification to further cluster your images, for example if
you need a hierarchical classification structure.
3) Auto-labelling your images based on the clusters identified and the labels you associated with the clusters. Note
Azure Machine Learning has an auto-labelling feature already, this Power Skill should be used if this feature is not suitable

See the data folder for sample images used for in the skill

## Requirements

In addition to the common requirements described in the root [README.md](https://github.com/Azure-Samples/azure-search-power-skills/blob/master/README.md) 
file, this Power Skill requires access to a Custom Vision resource. This process will use object detection and augment 
it with cluster labels.

To run this PowerSkill you will need:
* docker
* An Azure Blob storage container
* A provisioned Azure Cognitive Search (ACS) instance 
* A provisioned Azure Container Registry
* A Cognitive Services key in the region you deploy ACS to

Below is a full working example that you can get working end
to end on sample data.

## High level Process

![clustering process](images/imageclustering.png)
 
1) The first step in the process is to extract [VGG16](https://www.tensorflow.org/api_docs/python/tf/keras/applications/VGG16) embeddings from the images and train the [DBSCAN](https://scikit-learn.org/stable/modules/generated/sklearn.cluster.DBSCAN.html) model on the extracted features.  
   * To better understand the algorithm itself, please use [explanatory notebook](notebooks/1-detect-similar-images.ipynb), it contains a local example of the process.
2) Training:
   For simple local training use the local training cell in [Local Training](notebooks/2-local-training.ipynb)
3) As with any (especially, unsupervised) machine learning solution, inspecting the clusters generated and playing with the algorithm hyperparameters will be required.
   * To explore generated clusters and generate labels dictionary required for the custom skill, you can use [labeling notebook](notebooks/3-create-label-file-and-deploy.ipynb). These labels are what will be indexed to retrieve the images.
   * Clusters report is also available under the registered model on the Azure Machine Learning Portal.  
4) Deploy the skill and add the endpoint to your [skillset file](deployment/azuresearch/create_skillset.json) using the 
   [deploy notebook](notebooks/3-create-label-file-and-deploy.ipynb)
5) Run your indexer [deployment/azuresearch/create_indexer.json]
6) Investigate your indexed data and compare the effect of using Image Clustering Power Skill and Computer Vision Service using [Azure Search notebook](notebooks/4-compare-computer-vision-and-clustering.ipynb).

## How to implement

This section describes how to get this working on sample data
and how it can be amended for your data.

### Understanding the dataset and running clustering

The first step is to extract the sample data files here [train data](data/train.zip) and the
[test data](data/test.zip) into the existing data folder. 

Open the notebook [Detect Similar Images notebook](notebooks/1-detect-similar-images.ipynb)

This notebooks demonstrates the idea behind the ImageClusteringSkill using a small dataset of open and closed books and
bookshelves.
 
Basically, the PowerSkill consists of the following two steps:

* Extract VGG16 embeddings
* Cluster embeddings using DBSCAN

Run all the cells on the sample dataset to get an idea of how data is clustered.
The notebook will load sample book data from the [train folder](../data/train)
When using on your own data, experiment with the epsilon (eps) parameter as this
will influence the number of clusters detected in the data. Visually inspect it
until it makes sense.

A pre-trained VGG16 model (vgg16_weights_tf_dim_ordering_tf_kernels_notop.h5) will be used
to extract the features from the images.

The last cells display the data that have been clustered as similar.

### Training the model on the data

Open the notebook [Training the model notebook](notebooks/2-local-training.ipynb)

This notebook shows how the model can be trained on the sample data for inference.

Run the cell, "Local Training". Here the parameters for the DBSCAN
algorithm can be experimented with if running on your own data. If running on the book
sample data, leave them as is. Go here for more info on [DBSCAN](https://scikit-learn.org/stable/modules/generated/sklearn.cluster.DBSCAN.html)

Once complete this will save a model to the [models directory](../models). Note, we will be
deploying this model later to our API for inference.

### Labelling our clusters

Now that we have identified the clusters in our data, we want to go and label them with our 
search terms that will help users easily find them. In our sample data, we have books that 
are open and closed and we also have bookshelves. 

Now open the notebook [label and deploy notebook](notebooks/3-create-label-file-and-deploy.ipynb),
here you will see we labelled the books with a dictionary that allows multiple labels per cluster:

This cell will train a model on the data and show the clusters. All data with a cluster with value -1 could not be 
clustered, all other numbers represent the cluster id.

```python
dict = {0 : ['book cover', 'closed book'], 1 : ['open book', 'double spread'], 2: ['book shelf', 'library']}
```
Here the key of the dictionary relates to the cluster id discovered. Double check the labels
to ensure they match the cluster images, in case they have changed.

We will deploy our generated label file with our docker image.

#### Building and testing the cluster inference API locally

For this step you will need docker running so that we can build and test our inference API locally.
You will also need a container registry for the build.

Run the following command to build the inference API container image:

```bash
docker build -t [container_registry_name.azurecr.io/clusterextractor:[your_tag] .  
```

The container will require the following variables set at runtime, namely:

```bash
KEY=[YourSecretKeyCanBeAnything]    # This is a secret key - only requests with this key will be allowed
DEBUG=True   # This enables verbose logging
DBSCAN_MODEL=books.pkl  # This is the name of the cluster model created from training
CLUSTER_LABELS=  # This is the labels file we created to label our clusters
```
See the file [sample_env](custom-skills-deployment/sample_env) for the .env format

Now we can test the container by running it locally with our variables:

```bash
docker run -it --rm -p 5000:5000 -e KEY=[YourSecretKeyCanBeAnything] -e DEBUG=True 
-e DBSCAN_MODEL=books.pkl -e CLUSTER_LABELS=labels.pkl 
[container_registry_name.azurecr.io/clusterextractor:[your_tag]
```
Upon starting you will see a few tensorflow warnings and the download of the vgg model will
initiate. See below:

```bash
Downloading data from https://storage.googleapis.com/tensorflow/keras-applications/vgg16/vgg16_weights_tf_dim_ordering_tf_kernels_notop.h5
58892288/58889256 [==============================] - 16s 0us/step
```
You should also see the following:

```bash
INFO:uvicorn.error:Uvicorn running on http://0.0.0.0:5000 (Press CTRL+C to quit)
```
We are now ready to send a request.

The [deploy notebook](notebooks/3-create-label-file-and-deploy.ipynb) contains a cell 

[Test the deployed inference API Web App](notebooks/3-create-label-file-and-deploy.ipynb#Test-the-deployed-inference-API-Web-App)
that will enable you to test the Web App.

Alternatively you can also use Postman, see below:

Use [Postman](https://www.postman.com/) to issue a test request to your local inference API.
As we are emulating what Azure Cognitive Search will send to a PowerSkill, we need to base64
encode an image as a string.

Issue the request with the following include the contents of the file
[postman_request.json](data/postman_request.json) as the body:

```bash
URI: http://0.0.0.0:5000/api/extraction
Headers:
    Ocp-Apim-Subscription-Key: [YourSecretKeyCanBeAnything]
    Content-Type: application/json
Body: Copy the contents of the file ../data/postman_request.json

```
After issuing the above request you should get the following response:

```json
{
    "values": [
        {
            "recordId": "0",
            "errors": "",
            "data": {
                "label": [
                    "open book",
                    "double spread"
                ]
            },
            "warnings": ""
        }
    ]
}
```

### Deploying the inference API

We are now ready to deploy our inference API. We will deploy
this as an [Azure App Service Web App](https://docs.microsoft.com/en-us/azure/app-service/configure-custom-container?pivots=container-linux).
running a container.

First we need to push our newly built image to our container
registry.

Run the following command:
```bash
docker push [container_registry_name.azurecr.io/clusterextractor:[your_tag]
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

variable "dbscan_model" {
  description = "Set this to books.pkl (if using demo value)"
  default = "books.pkl"
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

variable "cluster_labels" {
  description = "Set this to labels.pkl (if using demo value)"
  default = "labels.pkl"
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

Once deployed, copy the Azure Web App URL which may be found
in the overview section of the portal as we will need it to
plug into Azure Search.

### Deploying the inference PowerSkill to Azure Cognitive Search

We are now ready to plug the Clustering PowerSkill into our ACS pipeline and test it.

Note, you need an already deployed ACS instance in the same region as your cognitive services
instance as we want to compare what our clustering provides in addition to the custom vision
services. Obviously we want to augment what we can extract using custom vision with our clustering
model.

You will need your [ACS API Key](https://docs.microsoft.com/en-us/azure/search/search-security-api-keys)
and the URL for your ACS instance. 

Navigate to and execute the [deploy PowerSkill to ACS](notebooks/3-create-label-file-and-deploy.ipynb#Deploy-the-PowerSkill-to-Azure-Search)
cell to deploy our PowerSkill. Alternatively, populate the the values within the [deployment json files](deployment/azuresearch)
files and use [Postman](https://postman.com).

The first step is to upload the [data files](data/) to a container in Azure blob storage and
get the connection values to create the ACS data source.

* Next create the index by running the [create index cell](notebooks/3-create-label-file-and-deploy.ipynb#Now-we-create-the-index)
* Next create the skillset by running the [create the skillset](notebooks/3-create-label-file-and-deploy.ipynb#Now-we-create-the-skill-set)
* Next create the indexer by running the [create indexer cell](notebooks/3-create-label-file-and-deploy.ipynb#Now-we-create-the-indexer)

The indexer will automatically run and you should see requests coming in if you look at the Web App logs.

### Testing the search labels in ACS

Now we are in a position to search on our cluster labelled data, navigate to the [test search cell](notebooks/3-create-label-file-and-deploy.ipynb#Test-the-cluster-labels-in-Azure-Search-queries)
to search on our clustered images.