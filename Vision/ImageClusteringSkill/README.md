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

You will need to train a clustering model with your images before you can use this skill for inference. 

## Process

![clustering process](images/imageclustering.png)
 
1) The first step in the process is to extract [VGG16](https://www.tensorflow.org/api_docs/python/tf/keras/applications/VGG16) embeddings from the images and train the [DBSCAN](https://scikit-learn.org/stable/modules/generated/sklearn.cluster.DBSCAN.html) model on the extracted features.  
   * To better understand the algorithm itself, please use [explanatory notebook](notebooks/1-detect-similar-images.ipynb), it contains a local example of the process.
2) Training: Two options exist here;
   For simple local training use the local training cell in [Local Training](notebooks/2-local-training-and-aml-training-pipeline.ipynb)   
   To leverage the power of [Azure Machine Learning](https://azure.microsoft.com/en-us/services/machine-learning/), train a model on a cluster and save trained model into registry to make it available for your custom skill, you can either use [training notebook](notebooks/2-local-training-and-aml-training-pipeline.ipynb) or proceed with setting up the full [MLOps process](https://github.com/microsoft/MLOpsPython/tree/master) using [clustering pipeline](mlops/clustering_pipeline).
3) As with any (especially, unsupervised) machine learning solution, inspecting the clusters generated and playing with the algorithm hyperparameters will be required.
   * To explore generated clusters and generate labels dictionary required for the custom skill, you can use [labeling notebook](notebooks/3-label-clusters.ipynb). These labels are what will be indexed to retrieve the images.
   * Clusters report is also available under the registered model on the Azure Machine Learning Portal.  
4) Deploy the skill and add the endpoint to your [skillset file](deployment/azuresearch/create_skillset.json)
5) Run your indexer [deployment/azuresearch/create_indexer.json]
6) Investigate your indexed data and compare the effect of using Image Clustering Power Skill and Computer Vision Service using [Azure Search notebook](notebooks/4-compare-computer-vision-and-clustering.ipynb).  