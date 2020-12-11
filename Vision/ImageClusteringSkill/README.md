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

![clustering process](images/imageclustering.png)
 
 1) The first step in the process is to train the DBSCAN model on your images. See
 mlops/clustering_pipeline for an [Azure Machine Learning](https://azure.microsoft.com/en-us/services/machine-learning/)
 pipeline for training.
 
 2) As with any machine learning solution, inspecting the clusters generated, normalising and optimising the images
 for the clusters desired will be required. 
 
 3) When the clusters generated are satisfactory run the cell "Generate the label file for your clusters"
 to create a dictionary with the label(s) you would like to assign to the cluster. These labels are what will
 be indexed to retrieve the images.
 
 4) Deploy the skill and add the endpoint to your [skillset file](deployment/azuresearch/create_skillset.json)
 
 5) Run your indexer [deployment/azuresearch/create_indexer.json]