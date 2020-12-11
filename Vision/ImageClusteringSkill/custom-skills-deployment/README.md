# Image Clustering Power Skill #

This Power Skill will uses the [DBSCAN](https://scikit-learn.org/stable/modules/generated/sklearn.cluster.dbscan.html)
unsupervised clustering algorithm alongside [VGG16](https://keras.io/api/applications/vgg/) to extract
visual features and cluster images. 

This skill is ideal for:

1) Exploring your data to identify clusters based on visual features during your data exploration phase
2) Using in conjunction with Custom Vision Classification to further cluster your images, for example if
you need a hierarchical classification structure.
3) Auto-labelling your images based on the clusters identified and the labels you associated with the clusters. Note
Azure Machine Learning has an auto-labelling feature already, this Power Skill should be used if this feature is not suitable

## Requirements

In addition to the common requirements described in the root [README.md](https://github.com/Azure-Samples/azure-search-power-skills/blob/master/README.md) 
file, this Power Skill requires access to a Custom Vision resource. This process will use object detection and augment 
it with cluster labels.

You will need to train a clustering model with your images before you can use this skill for inference. 

## Process

![clustering process](../images/imageclustering.png)
 
 1) The first step in the process is to train the DBSCAN model on your images. See
 mlops/clustering_pipeline for an [Azure Machine Learning](https://azure.microsoft.com/en-us/services/machine-learning/)
 pipeline for training.
 
 2) As with any machine learning solution, inspecting the clusters generated, normalising and optimising the images
 for the clusters desired will be required. 
 
 3) When the clusters generated are satisfactory run the cell "Generate the label file for your clusters"
 to create a dictionary with the label(s) you would like to assign to the cluster. These labels are what will
 be indexed to retrieve the images.
 
 4) Deploy the skill and add the endpoint to your [skillset file](../deployment/azuresearch/create_skillset.json)
 
 5) Run your indexer [deployment/azuresearch/create_indexer.json]
 
 
 ## The FastAPI app
 
 ## Environment variables

The following sample environment variables need to be set for the process to work:

```bash
        KEY=[YourSecretKeyCanBeAnything] # Secret key used as a header for authentication
        DEBUG=True  # Will enable verbose logging
        DBSCAN_MODEL=books.pkl  # The trained model for the sample book dataset
        DOWNLOAD_AML_MODEL=False  # If true will download the model from AML
        WORKSPACE_NAME=  # AML Workspace
        RESOURCE_GROUP=  # The resource groups of the assets
        SUBSCRIPTION_ID=  # Azure sub Id
        TENANT_ID=  # Id of the tenant in the Azure sub
        SP_APP_ID=  # Service Principal Id for AML
        SP_APP_SECRET=  # Service Principal Secret
        LOCATION=  # Resource location
        CLUSTER_LABELS_LOCATION=https://[storage].blob.core.windows.net/tester/labels.pkl?[SAS]

```
 
 ### Authentication

The API will perform a simple check to determine whether the KEY Header has been set.

It will validate that the value passed as a header to call this API, namely:

```bash
Ocp-Apim-Subscription-Key: [KEY]
```
### Normal start

To start the application for normal usage, run the following command:

```bash
uvicorn app:app --reload --port 5000
```

### Build and Test

The majority of steps necessary to get you up and running are already done by the dev container. But this project uses the following:

- Python
- Pip

Once your container is up and running you should:

1. Open your test `.py` file (```tests/powerskill_api_test.py```) and set the Python interpreter to be your venv (bottom blue bar of VSCode)
2. Use the python test explorer plugin to run your tests or click the 'run test' prompt above your tests


### Remote SSH debugging

To enable the ```SSH``` connection for development debugging if deployed to Azure Web Apps, deploy the file [```Dockerfile_debug```](containers/Dockerfile_debug)
which will enable the Azure Web App to bridge a connection to the running docker instance. See the [Enable SSH](https://docs.microsoft.com/en-gb/azure/app-service/configure-custom-container?pivots=container-linux#enable-ssh)
for more info. This is useful for inspecting running processes and checking model binaries are deployed correctly.

The files [```ssdh_config```](containers/sshd_config) and [```startup.sh```](containers/startup.sh) are used only for this debugging 
```Dockerfile_debug```. 

#### Connecting to the container

* Once deployed, select the ```ssh``` option in the Azure portal on the web app
* Click ```Go```
* You should see green message at the bottom of the screen with ```SSH CONNECTION ESTABLISHED``` if successful
* The terminal session should then be available for input
 
