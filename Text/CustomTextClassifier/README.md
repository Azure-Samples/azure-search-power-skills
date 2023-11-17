# Text Classification Skill

---

Description:

- It is common to require a text classification along knowledge base scenarios, for example you might to want to classify a document as a RFI response, a contract, a letter of intent or just a BoM. Custom Text Classification (in preview as of Nov2021) provides the capability to ingest your training texts, label your set of custom labels (both single and multi class) and train a model to classify them. You can easily deploy the model in a secured fashion to later on run your inference along your texts. As an outcome you will get the detected custom classes and the confidence level

- text_classification_skill is an Azure AI Search skill to integrate [Azure Text Analytics Custom Text Classification](https://docs.microsoft.com/azure/cognitive-services/language-service/custom-classification/overview) within a Azure AI Search skillset. This will enable the cracking of documents in a programmatic way to enrich your search with different custom classes. For example, show me the RFI responses by X employee between May and June 2021. This filtering is possible because Text Analytics has identified all those classes along the skillset execution and exposes the ability to narrow the results within the ACS index.

Languages:

- ![python](https://img.shields.io/badge/language-python-orange)

Products:

- Azure AI Search
- Azure AI Services for Language (Text Analytics)
- Azure Functions

Table of Contents:

* [Steps](#steps)
  
  - [Create or reuse a Custom Text Classification project](#create-or-reuse-a-custom-text-classification-project)
  - [Create a Function App resource and deploy the powerskill to Azure](#create-a-function-app-resource-and-deploy-the-powerskill-to-Azure)
    
    - [ARM Deployment](#arm-deployment)
    
    - [Manual Deployment](#manual-deployment)
      
      - [Create a Function App](#create-a-function-app)
      
      - [Deploy the powerskill](#deploy-the-powerskill)
    
    - [Additional details](#additional-details)
  
  - [Integrate with Azure AI Search](integrate-with-azure-cognitive-search)
    
    - [Skillset](#skillset)
    
    - [Index](#index)
    
    - [Indexer](#indexer)
    
    - [Query the index](#query-the-index)

- [Testing](#testing)

---

# Steps

#### Create or reuse a Custom Text Classification project

In order to use Custom Text Classification, we need a language resource and a trained (and deployed) project to be used in custom classification. If they aren't previously created, now is the time to do that. A good place to start is [Quickstart: Custom text classification - Azure AI Services | Microsoft Docs](https://docs.microsoft.com/en-us/azure/cognitive-services/language-service/custom-text-classification/quickstart).

After this step you should have:

- A Language resource.

- A project

- A deployment

#### Create a Function App resource and deploy the powerskill to Azure

A powerskill is basically just an Azure Function written to be used as a custom skill in an Azure AI Search pipeline. To deploy a function, an Azure App resource is needed. Notice the difference between a function (code that can be deployed, like the one in this subrepo) and an Azure Function App (the Azure resource that a function can be deployed to). An ARM template (see [Templates overview - Azure Resource Manager | Microsoft Docs](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/overview)) is provided to automate creating the Function App resource and deploying this powerskill on the resource. Alternatively, one may choose to manually create a Function App resource (for example, from the Azure portal), clone the repository, and deploy the code from VSCode. Both options are explored below.

##### ARM deployment

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fazure-search-power-skills%2Fmain%2FText%2FCustomTextClassifier%2Fazuredeploy.json)

This ARM template provides a streamlined way to both create a Function App resource and deploy the powerskill. The template takes the following parameters

| App setting          | Description                                                         | Details                                                                                                          |
| -------------------- | ------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- |
| Function App Name    | The name of the created Function App resource                       | A default value of `txtcla<a-generated-unique-id>` is provided, but can be set to any other unique name.        |
| Storage Account Name | The name of the created Function App resource                       | A default value of `txtcla<the-same-generated-unique-id>` is provided, but can be set to any other unique name. |
| Storage Account Type | The type of the storage account                                     | `Standard_LRS`, `Standard_GRS` or `Standard_RAGRS`                                                               |
| Location             | The location of all created resources                               | `West US`, `Central US`, etc. Uses the resource group location by default.                                       |
| Package Uri          | A link to the zipped function                                       | See below. By default, uses the zip file in the repository.                                                      |
| Language Endpoint    | The endpoint of the Language resource created in the previous step. | Can be found under `Resource Management -> Keys and Endpoint` in the Language resource page in Azure Portal.     |
| Language Key         | The key of the Language resource.                                   | Can be found under `Resource Management -> Keys and Endpoint` in the Language resource page in Azure Portal.     |
| Project Name         | The name of the project in the Language resource.                   |                                                                                                                  |
| Deployment Name      | The name of the deployment in the Language resource.                |                                                                                                                  |
| Classification Type  | The classification type of the project.                             | `multi` or `single`                                                                                              |

Due to limitations in ARM templates, this ARM template cannot grab the source code for the function project automatically from Github. Instead, the user needs to package the project in a Zip format, upload it somewhere accessible to Azure, and provide a link to the uploaded Zip file to the template. One way to do this is

* [Download](https://downgit.github.io/#/home?url=https:%2F%2Fgithub.com%2FAzure-Samples%2Fazure-search-power-skills%2Ftree%2Fmain%2FText%2FCustomTextClassifier) (and extract) the source code for this powerskill using the open-source utility [DownGit](https://downgit.github.io). More experienced users might prefer to clone the repository instead.
* Zip the necessary folders/files. These are `customtextcla`, `host.json` and `requirements.txt`. On Windows, you can select the folders/files in File Explorer, right-click, and select `Compress to ZIP file` (or `Send to -> Compressed (zipped) folder` on older versions). On Linux, running `zip -r customtextcla-powerskill.zip customtextcla host.json requirements.txt` inside the project directory should yield the same result.
* Upload the created Zip file to any file hosting service. An obvious service is Azure Blob Storage. Instructions for Blob Storage can be found [here](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-portal). Make sure to make your file publicly accessible, so that the ARM template can access it.
* Use the URL of the uploaded file in the ARM template.

The template creates the following resources:

* A Function App with the powerskill deployed on it.
* A storage account to be used by the Function App.
* A serverless Consumption hosting plan for the function app.
* An Application Insights resource for the Function App. This is useful for monitoring the Function App.

Generally, the Function App is the main resource, while the other resources are only occasionally interacted with.

##### Manual Deployment

###### Create a Function App

From the Azure portal, select `Create a resource` and search for `Function App`. The creation page should appear, click `Create`. In the new page, in the `Basics` tab, select your subscription and resource group, give your Function App a globally unique name and select a suitable region. Fill the remaining parameters as follows:

| App setting      | Value                      |
| ---------------- | -------------------------- |
| Publish          | `Code`                     |
| Runtime Stack    | `Python`                   |
| Version          | `3.9`                      |
| Operating System | `Linux`                    |
| Plan type        | `Consumption (Serverless)` |

When done, click `Review + create` and wait a few seconds while the inputs are validated. If all goes well, a red warning telling you that `You will need to Agree to the terms of service below to create this resource successfully.` should appear. To accept the terms and deploy the template, click `Create`. You should now have a Function App resource.

After creating the resource, some app settings (visible inside Azure Functions as environment variables) need to be added for the powerskill to run correctly. here's how to [change app settings in the Azure portal](https://docs.microsoft.com/en-us/azure/app-service/configure-common?tabs=portal#configure-app-settings).

| App setting         | Description                                                         | Details                                                                                                      |
| ------------------- | ------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------ |
| LANG_ENDPOINT       | The endpoint of the Language resource created in the previous step. | E.g. https://<language-resource-name>.cognitiveservices.azure.com/                                           |
| LANG_KEY            | The access key to be able to access the endpoint.                   | Can be found under `Resource Management -> Keys and Endpoint` in the Language resource page in Azure Portal. |
| PROJECT_NAME        | The key of the Language resource.                                   |                                                                                                              |
| DEPLOYMENT_NAME     | The name of the deployment in the Language resource.                |                                                                                                              |
| CLASSIFICATION_TYPE | The classification type of the project.                             | `multi` or `single`                                                                                          |

###### Deploy the powerskill

In this set of instructions, VSCode is used to deploy the function following this [quickstart](https://docs.microsoft.com/en-us/azure/azure-functions/create-first-function-vs-code-python). For more information on deployment methods, see [Deployment technologies in Azure Functions | Microsoft Docs](https://docs.microsoft.com/en-us/azure/azure-functions/functions-deployment-technologies).

First, we need to download the source code for the powerskill. [Download](https://downgit.github.io/#/home?url=https:%2F%2Fgithub.com%2FAzure-Samples%2Fazure-search-power-skills%2Ftree%2Fmain%2FText%2FCustomTextClassifier) (and extract) the source code for this powerskill using the open-source utility [DownGit](https://downgit.github.io). More experienced users might prefer to clone the repository instead.

Afterwards, make sure you have [the following requirements](https://docs.microsoft.com/en-us/azure/azure-functions/create-first-function-vs-code-python#configure-your-environment). If so, open the downloaded directory in VSCode (or its equivalent location in a cloned repo). [Sign into Azure](https://docs.microsoft.com/en-us/azure/azure-functions/create-first-function-vs-code-python#sign-in-to-azure) and [deploy the project to Azure](https://docs.microsoft.com/en-us/azure/azure-functions/create-first-function-vs-code-python#deploy-the-project-to-azure). Select the previously created Function App when prompted. The powerskill project should now be deployed and running.

##### Additional details

```json
{
    "values": [
      {
        "recordId": "0",
        "data": {
            "id": "1",
            "lang": "en",
            "text": "The film deals with student strength. Shiva , a college student is leading a happy go lucky life, having a nice time with his friends, until two of his friends fall in love. Facing stiff opposition from the girl’s father, all the friends get together and get the couple married. But the married girl get gang raped in the college campus, by Madan, the son of a politician. Unable to get justice in court, Shiva along with his friends takes it upon himself to avenge his friend’s death, but pays a very heavy price. His mother will be killed by the politician and they lose at court. Now Shiva must get justice in his own way. He can kill Mahan and his father."
        }
      }
    ]
}
```

```json
{
    "values": [
        {
            "recordId": "0",
            "data": {
                "class": [
                    {
                        "category": "Comedy",
                        "confidence_score": 0.99
                    },
                    {
                        "category": "Drama",
                        "confidence_score": 0.87
                    }
                ]
            },
            "warnings": []
        }
    ]
}
```

#### Integrate with Azure AI Search

To finally be able to use the deployed powerskill, an Azure AI Search resource should be available. If not, now is the time to create one. For instructions, see [Create a search service in the portal - Azure AI Search | Microsoft Docs](https://docs.microsoft.com/en-us/azure/search/search-create-service-portal).

###### Skillset

An Azure AI Search pipeline consists of an Index, an Indexer, a skillset and data source. This function can be used as a custom skill in a skillset (either as a singular custom skill in a skillset, or as a skill among many others in a skillset). To add this function as a custom skill, some parameters need to be specified, including the the endpoint URL of the Function App deployed in the previous steps, the `x-functions-key` header, what is needed as input, and what the output is named. An example is shown below. Here, the text of each document (`/document/content`) is sent to the api as a value for the key named `text` (as the function expects) and the output is the value of the key named `class` in the response. An example of what a skillset may look like is shown below. For more information on skillsets, see [Skillset concepts - Azure AI Search | Microsoft Docs](https://docs.microsoft.com/en-us/azure/search/cognitive-search-working-with-skillsets).

```json
{
    "skills": [
      "[... your existing skills remain here]",  
      {
        "@odata.type": "#Microsoft.Skills.Custom.WebApiSkill",
        "name": "customtextcla-skill",
        "description": "",
        "context": "/document",
        "uri": "https://<azure-function-name>.azurewebsites.net/api/customtextcla",
        "httpMethod": "POST",
        "timeout": "PT30S",
        "batchSize": 1,
        "degreeOfParallelism": 1,
        "inputs": [
          {
            "name": "text",
            "source": "/document/content"
          }
        ],
        "outputs": [
          {
            "name": "class",
            "targetName": "class"
          }
        ],
        "httpHeaders": {
          "x-functions-key": "<azure-function-key>"
        }
      }
  ]
}
```

###### Index

Next, the output from the custom skill can be used as an input to yet another skill, or be part of the final output that is saved into the index. Assuming the latter, the index needs to have a field definition that matches the output it's given. An example for what the definition should look like is shown below. For more information on search indexes, see [Index overview - Azure AI Search | Microsoft Docs](https://docs.microsoft.com/en-us/azure/search/search-what-is-an-index).

```json
{
      "name": "class",
      "type": "Collection(Edm.ComplexType)",
      "fields": [
        {
          "name": "category",
          "type": "Edm.String",
          "searchable": true,
          "filterable": true,
          "retrievable": true,
          "sortable": false,
          "facetable": false,
          "key": false,
          "indexAnalyzer": null,
          "searchAnalyzer": null,
          "analyzer": "standard.lucene",
          "normalizer": null,
          "synonymMaps": []
        },
        {
          "name": "confidence_score",
          "type": "Edm.Double",
          "searchable": false,
          "filterable": true,
          "retrievable": true,
          "sortable": false,
          "facetable": false,
          "key": false,
          "indexAnalyzer": null,
          "searchAnalyzer": null,
          "analyzer": null,
          "normalizer": null,
          "synonymMaps": []
        }
      ]
    }
```

###### Indexer

Finally, the indexer ties everything together. The indexer needs to be setup up such that the outputs from the custom skill are mapped to the field that was just defined. Notice the `outputFieldMappings` key in the example shown below, the content of each document is mapped to the a field named `content`, and the output from the powerskill (that was named `class` in the skillset) is mapped to a field named `class` in the index. For more information on output mappings, see [Map skill output fields - Azure AI Search | Microsoft Docs](https://docs.microsoft.com/en-us/azure/search/cognitive-search-output-field-mapping) in addition to the previously linked article about skillsets.

```json
{
  "@odata.context": "...",
  "@odata.etag": "...",
  "name": "<indexer-name>",
  "dataSourceName": "<datasource-name>",
  "skillsetName": "<skillset-name>",
  "targetIndexName": "<index-name>",
  ...
  "outputFieldMappings": [
    {
      "sourceFieldName": "/document/content",
      "targetFieldName": "content"
    },
    {
      "sourceFieldName": "/document/class",
      "targetFieldName": "class"
    }
  ]
}
```

#### Query the index

Now that we have a populated index, we can use the capabilities of the index to make useful queries. For example, using the sample data for scientific papers from the custom text classification documentation (found [here](https://github.com/Azure-Samples/cognitive-services-sample-data-files/tree/master/language-service/Custom%20text%20classification)), we can query the index to find papers in specific fields. For example,

```
$queryType=full&$searchMode=all&$count=true&$filter=class/any(c: c/category eq 'Computer_science')
```

will get all `Computer_science` research papers. The response should look like the following.

```json
{
  "@odata.context": "...",
  "@odata.count": 10,
  "value": [
    {
      "@search.score": 1,
      "id": "MDE2LnR4dA",
      "content": "A set of algorithms for simultaneous localization and mapping in industrial television systems is discussed. A probabilistic model of this problem is described with the FastSLAM algorithm as an example. The possibility of using a sigma-point Kalman filter for estimating the movement of spatial landmarks, a key feature of images characterized by stable detection and recognition within the video stream, is considered. A general model of the camera motion and a method for evaluating its spatial position using a particle filter are presented.\n",
      "class": [
        {
          "category": "Computer_science",
          "confidence_score": 1
        }
      ]
    },
    {
      "@search.score": 1,
      "id": "MDA5LnR4dA",
      "content": "An accurate algorithm for three-dimensional (3-D) pose recognition of a rigid object is presented. The algorithm is based on adaptive template matched filtering and local search optimization. When a scene image is captured, a bank of correlation filters is constructed to find the best correspondence between the current view of the target in the scene and a target image synthesized by means of computer graphics. The synthetic image is created using a known 3-D model of the target and an iterative procedure based on local search. Computer simulation results obtained with the proposed algorithm in synthetic and real-life scenes are presented and discussed in terms of accuracy of pose recognition in the presence of noise, cluttered background, and occlusion. Experimental results show that our proposal presents high accuracy for 3-D pose estimation using monocular images. (C) 2016 Society of Photo-Optical Instrumentation Engineers (SPIE)\n",
      "class": [
        {
          "category": "Computer_science",
          "confidence_score": 1
        }
      ]
    },
    {
      "@search.score": 1,
      "id": "MDI0LnR4dA",
      "content": "A correspondence is a set of mappings that establishes a relation between the elements of two data structures (i.e. sets of points, strings, trees or graphs). If we consider several correspondences between the same two structures, one option to define a representative of them is through the generalised median correspondence. In general, the computation of the generalised median is an NP-complete task. In this paper, we present two methods to calculate the generalised median correspondence of multiple correspondences. The first one obtains the optimal solution in cubic time, but it is restricted to the Hamming distance. The second one obtains a sub-optimal solution through an iterative approach, but does not have any restrictions with respect to the used distance. We compare both proposals in terms of the distance to the true generalised median and runtime.\n",
      "class": [
        {
          "category": "Computer_science",
          "confidence_score": 1
        }
      ]
    },
    {
      "@search.score": 1,
      "id": "MDEzLnR4dA",
      "content": "A number of computer vision problems such as facial age estimation, crowd counting and pose estimation can be solved by learning regression mapping on low-level imagery features. We show that visual regression can be substantially improved by two-stage regression where imagery features are first mapped to an attribute space which explicitly models latent correlations across continuously-changing output. We propose an approach to automatically discover \"spectral attributes\" which avoids manual work required for defining hand-crafted attribute representations. Visual attribute regression outperforms direct visual regression and our spectral attribute visual regression achieves state-of-the-art accuracy in multiple applications.\n",
      "class": [
        {
          "category": "Computer_science",
          "confidence_score": 1
        }
      ]
    },
    {
      "@search.score": 1,
      "id": "MDIyLnR4dA",
      "content": "A basic question in computational geometry is how to find the relationship between a set of points and a line in a real plane. In this paper, we present multidimensional data structures for N points that allow answering the following queries for any given input line: (1) estimate in O (log N) time the number of points below the line; (2) return in O (log N + k) time the k <= N points that are below the line; and (3) return in O (log N) time the point that is closest to the line. We illustrate the utility of this computational question with GIS applications in air defense and traffic control.\n",
      "class": [
        {
          "category": "Computer_science",
          "confidence_score": 1
        }
      ]
    },
    {
      "@search.score": 1,
      "id": "MDAyLnR4dA",
      "content": "3D digital visualization technology is a new research field along with the rapid development of computer technology. It is a multi-scale technology which consists of computer graphics, image information processing, computer aided design. This paper is about the research and development of products innovation design method, thereby realize the innovation of product design rapidly. At the same time to verify the feasibility and practicality and broad application prospect of applying 3D scan technology to the rapid development, design and manufacture of products.\n",
      "class": [
        {
          "category": "Computer_science",
          "confidence_score": 1
        }
      ]
    },
    {
      "@search.score": 1,
      "id": "MDIwLnR4dA",
      "content": "Video tracking is a main field of computer vision, and TLD algorithm plays a key role in long-term tracking. However, the original TLD ignores the color features of patch in detection, and tracks the common points from grid, then, the tracking accuracy is limited to both of them. This paper presents a novel TLD algorithm with Harris corner and color moment to overcome this drawback. Instead of tracking common points, we screen more important points utilizing Harris corner to reject a half patches, these points are better able to show the object's textural features. In addition, the color moment classifier replaces patch variance to reduce the errors of detection. The classifier compares mine-dimensional color moment vectors so that it can keep the TLD's stable speed. Experiment has proved that our TLD tracks a more reliable position and higher ability without affecting the speed.\n",
      "class": [
        {
          "category": "Computer_science",
          "confidence_score": 1
        }
      ]
    },
    {
      "@search.score": 1,
      "id": "MDA2LnR4dA",
      "content": "A phase-only computer-generated holography (CGH) calculation method for stereoscopic holography is proposed in this paper. The two-dimensional (2D) perspective projection views of the three-dimensional (3D) object are generated by the computer graphics rendering techniques. Based on these views, a phase-only hologram is calculated by using the Gerchberg-Saxton (GS) iterative algorithm. Comparing with the non-iterative algorithm in the conventional stereoscopic holography, the proposed method improves the holographic image quality, especially for the phase-only hologram encoded from the complex distribution. Both simulation and optical experiment results demonstrate that our proposed method can give higher quality reconstruction comparing with the traditional method.\n",
      "class": [
        {
          "category": "Computer_science",
          "confidence_score": 1
        }
      ]
    },
    {
      "@search.score": 1,
      "id": "MDI1LnR4dA",
      "content": "A gradient-statistic-based diagnostic measure is developed in the context of the generalized linear mixed models. Its performance is assessed by some real examples and simulation studies, in terms of ability in detecting influential data structures and of concordance with the most used influence measures.\n",
      "class": [
        {
          "category": "Computer_science",
          "confidence_score": 1
        }
      ]
    },
    {
      "@search.score": 1,
      "id": "MDA3LnR4dA",
      "content": "Accurately and reliably tracking the undulatory motion of deformable fish body is of great significance for not only scientific researches but also practical applications such as robot design and computer graphics. However, it remains a challenging task due to severe body deformation, erratic motion and frequent occlusions. This paper proposes a tracking method which is capable of tracking the midlines of multiple fish based on midline evolution and head motion pattern modeling with Long Short-Term Memory (LSTM) networks. The midline and head motion state are predicted using two LSTM networks respectively and the predicted state is associated with detections to estimate the state of each target at each moment. Experiment results show that the system can accurately track midline dynamics of multiple zebrafish even when mutual occlusions occur frequently.\n",
      "class": [
        {
          "category": "Computer_science",
          "confidence_score": 1
        }
      ]
    }
  ]
}
```

For more details and examples, see [Use full Lucene query syntax - Azure AI Search | Microsoft Docs](https://docs.microsoft.com/en-us/azure/search/search-query-lucene-examples) and [Filter on search results - Azure AI Search | Microsoft Docs](https://docs.microsoft.com/en-us/azure/search/search-filters).

## Testing

A small test suite is provided in the `tests` directory. There are tests for single and multi classifications The tests assume `multi.env` and `single.env` files that contains the parameters that the powerskill needs to run. The files use a simple `KEY=VAL` syntax separated by newlines.

```
PROJECT_NAME=<project-name>
DEPLOYMENT_NAME=<deployment-name>
LANG_KEY=<language-resource-key>
LANG_ENDPOINT=https://<language-resource-name>.cognitiveservices.azure.com
CLASSIFICATION_TYPE=<multi-or-single>
```

The test cases assume that the model (in both single and multi classification) is trained to recognise data like the example in [Quickstart: Custom text classification - Azure AI Services | Microsoft Docs](https://docs.microsoft.com/en-us/azure/cognitive-services/language-service/custom-text-classification/quickstart). Training and test data for  can be found [here](https://github.com/Azure-Samples/cognitive-services-sample-data-files/tree/master/language-service/Custom%20text%20classification).

To run the tests, `cd` into the root of the project and run `unittest`.

```bash
cd CustomTextClassifier
python -m unittest
```
