# LLM Chunker Power Skill #

This Power Skill facilitates the conversion of document pages and slides (.pdf, .ppt, .pptx, .doc, .docx) into PNG images, which are then processed by GPT-4o or GPT-4o mini to produce high-quality markdown output.

This skill is ideal for:

- Documents containing visually rich content such as charts, flow diagrams, and decision trees, where conventional OCR/text extraction methods fail to deliver satisfactory results. GPT-4o transcribes all data into text while preserving the original meaning accurately. For instance, a bar chart is converted into a markdown table, and a decision tree graph is transformed into a bullet list that maintains all “Yes/No” decision points and connections between entities. In a standard text extraction pipeline, these connections are often lost, resulting in disjointed words. You can refer to the [Data](data/) folder for examples of documents that are ideal for this solution.

This skill is **NOT** ideal for:
- Documents that primarily contain text or tables, where the goal is to enhance the quality of the AI Search built-in OCR. In such cases, [Azure Document Intelligence](https://azure.microsoft.com/products/ai-services/ai-document-intelligence) is a more suitable alternative due to its faster processing capabilities. There is also [another Power Skill](https://github.com/Azure-Samples/azure-search-power-skills/blob/main/Vision/FormRecognizer) Power Skill available for this purpose.

## Credits
A lot of this code is inpired by the [original solution](https://github.com/liamca/GPT4oContentExtraction) from [Liam Cavanagh](https://github.com/liamca).

## Requirements

In addition to the common requirements described in the root [README.md](https://github.com/Azure-Samples/azure-search-power-skills/blob/master/README.md) 
file, this Power Skill requires access to a OpenAI resource. 

This PowerSkill will use:
* Docker
* Azure Container Registry
* Azure Blob storage container
* Azure Web App Service - Linux Containers
* Azure AI Search instance
* Azure OpenAI

## Sequence diagram

```mermaid
sequenceDiagram
    participant aisearch as AI Search
    participant PowerSkill
    participant OpenAI
    participant storage as Storage Account

    aisearch->>+PowerSkill: Send document URL
    PowerSkill->>+storage: Download file
    storage-->>-PowerSkill: File
    PowerSkill->>PowerSkill: Convert document to PDF
    PowerSkill->>PowerSkill: Create PNG images for each PDF page
    loop Images
      PowerSkill->>+OpenAI: Send PNG images
      OpenAI-->>-PowerSkill: Return markdown
    end
    PowerSkill->>PowerSkill: Merge responses into a final markdown
    PowerSkill->>PowerSkill: Create context-aware text chunks
    PowerSkill-->>-aisearch: Markdown chunks
```

## Quick Azure deployment
In order to deploy everything, you can simply use bash and type
```bash
make deploy
```
and type 'yes' when prompted. You can test your deployment by running the [test scripts](tests/). You need to install Visual Studio Code [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) extension.

At this point, Terraform has now created the following infrastructure and created a `base.env` file with the values.
- Azure Container Registry to store our docker image
- Azure AI Search
- Azure OpenAI
- Azure Blob Storage to hold our data to seach

## Run locally with Visual Studio Code
 
1) Make sure you rename the file [.env.example file to .env](powerskill/.env.example) and populate it with the relevant values.
1) Run VsCode and connect to your WSL (Linux) locally. If you do not know what WSL is, check [here](https://code.visualstudio.com/docs/remote/wsl#_from-vs-code). 
1) Open the LLMChunker folder in VsCode WSL (avoid opening the root 'azure-search-power-skills' where all other power skills are).
1) Running the app in a Linux distro is required because the document conversion libraries to convert incoming files to PDF requires LibreOffice's Linux libraries. Open your WSL bash terminal in VsCode and then run the command to install it:
  ```bash
  apt-get update
  apt-get -y install libreoffice-nogui
  apt-get -y install wkhtmltopdf
  ```
1) Create a Python virtual environment and install dependencies:
  ```bash
  python -m venv .venv
  pip install -r powerskill/requirements.txt
  ```  
1) Open the file [app.py](powerskill/app.py) in the VsCode editor.
1) Press F5 to start debugging in Visual Studio Code. Select Python Debugger -> Python File as the interpreter. If it asks what Python path to use, select the binaries under your .venv folder as the interpreter.
1) Your application should be running in debug mode at this point, listening to requests in http://localhost:5000
1) You can test your local API by exploring the HTTP request files in [tests](tests/).
  
### Note about authentication
The application uses [managed identity](https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/overview) to authenticate itself to storage account and OpenAI. When you deploy this sample to Azure using the provided Terraform scripts, the required permissions and identities are automatically created at the resource group level and are transparent for you.
However, if you are running the application locally, your user needs to have the following permissions:
- Storage account: [Storage Blob Data Reader](https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles/storage#storage-blob-data-reader)
- OpenAI: [Cognitive Services OpenAI User](https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles/ai-machine-learning#cognitive-services-openai-user)

For your convenience, the app returns exactly what user and permissions are needed in case there is an authentication error when acessing these resources.

## Configuration
The available [tests](tests/) scripts only contains the required input parameter *blobUrl*. However, you can also customize the behavior of the extraction process by sending these parameters in each HTTP request:

| Name                          | Description                                                                                                                                                                                                                                                                                                                                                       |
|-------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| image_quality                 | The quality of the images to be sent to OpenAI. The higher the quality, the more tokens it will consume. The available options are: low, high_720p, high_1024p, high_1920p. If your source documents have small text, increase this value until you get satisfactory results. For more details, refer to [OpenAI Vision Guide](https://platform.openai.com/docs/guides/vision/low-or-high-fidelity-image-understanding). Default value = 'high_1024p' |
| chunk_size                    | The size (in tokens) to split the markdown sections in case they go over this value. If a markdown section of heading 3 (###) goes over this value, it will be split into multiple chunks. If the markdown section is below that token size, it will be returned as a single chunk. Default value = 512                                                                                   |
| chunk_overlap                 | The percentage of overlap between chunks. This parameter is used to avoid splitting sentences between chunks. Default value = 25   
| extraction_prompt                 | The prompt used in OpenAI to perform the conversion from images to markdown. Default value can be checked [here](powerskill/models/app_config.py)                  


More settings are set in the application-level environment settings:

| Name                          | Description                                                                                                                                                                                                                                                                                                                                                       |
|-------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| openai_deployment             | The deployment name of the OpenAI instance. Eg: gpt4o-deployment                                                                                                                                                                                                                                                                                                  |
| openai_api_version            | The API version of Azure OpenAI according to Azure OpenAI Reference. Default value = 2024-06-01                                                                                                                                                                                                                             |
| openai_max_concurrent_requests| The maximum number of concurrent requests to make to the OpenAI at a given time. Increase or decrease this based on your available quota. Devault value = 5                                                                                                                                                                                                                         |
| openai_max_images_per_request | The maximum number of images (document pages) to send to OpenAI in a single request. This parameter is highly dependent on the size of the images (which is set by the 'IMAGE_QUALITY' parameter). The more images in the request, the better context OpenAI has to generate coherent markdown output, but it also increases response time and token consumption. Default value = 15 |
| openai_max_retries            | The maximum number of retries to make to the OpenAI API in case of rate limiting responses (HTTP status code 429). Default value = 6                                                                                                                                                                                                                                                |
| openai_max_backoff            | The maximum number of seconds to wait before retrying a request to the OpenAI API in case of rate limiting responses (HTTP status code 429). Default value = 60                                                                                                                                                                                                                       |
| openai_max_tokens_response    | The maximum number of tokens to expect in the OpenAI response per image. If your parameter 'openai_max_images_per_request' is set to 15 and 'openai_max_tokens_response' is set to 1024, that means your total max_tokens in the response will be 15 * 1024 = 15360. Do not set this value above 16000, which is GPT4o/mini max tokens response. Default value = 1024
      
## Samples

### Sample input

```json
{
    "values": [
        {
            "data": {
                "blobUrl": "https://youraccount.blob.core.windows.net/docs/Healthcare-decision-making-process-flow-chart-1.pdf"
                 /* Add here other settings as described in the Configuration section above */
            },
            "recordId": "0"
        }
    ]
}
```

### Sample output

```json
{
  "values": [
    {
      "data": {
        "chunks": [
          {
            "chunk_id": "be61ebe4-ee35-421d-a73b-4f227795c0c9-1",
            "file_name": "Healthcare-decision-making-process-flow-chart-1.pdf",
            "content": "# Healthcare Decision Making in Queensland Process  \n## Presumption of Capacity & Consent  \n- **Is it an emergency?**\n- **Yes:** Consent not required\n- **No:** Consent required  \n- **Can the person validly consent or refuse?**\n- **Yes:** The person makes decision\n- **No/Unsure:** Is the person’s capacity in question?  \n- **Is the person’s capacity in question?**\n- **No:** The person makes decision\n- **Yes:** Supported Decision-Making & Capacity Assessment",
            "title": "Presumption of Capacity & Consent"
          },
          {
            "chunk_id": "be61ebe4-ee35-421d-a73b-4f227795c0c9-2",
            "file_name": "Healthcare-decision-making-process-flow-chart-1.pdf",
            "content": "## Supported Decision-Making & Capacity Assessment  \n- **QLD capacity assessment guidelines** and **Capacity Assessment**\n- **Has capacity?** Adequate for consent/refusal\n- Implement Supported Decision-Making Strategies\n- **Still unsure about capacity:**\n- Inadequate for consent/refusal\n- Substitute Decision-Making  \n- **General capacity test:**\n1. Understands the nature and effect of the decision\n2. Freely & voluntarily making the decision\n3. Can communicate the decision in some way\n4. Assessed at the time decision is required",
            "title": "Supported Decision-Making & Capacity Assessment"
          },
          {
            "chunk_id": "be61ebe4-ee35-421d-a73b-4f227795c0c9-3",
            "file_name": "Healthcare-decision-making-process-flow-chart-1.pdf",
            "content": "## Substitute Decision-Making  \n- **Include the adult by including their views, wishes, and preferences in the decision making**\n- **Is there Advance Health Directive?**\n- **Yes:** Advance Health Directive applies\n- **No:** Is a Healthcare Guardian appointed?\n- **Yes:** Healthcare Guardian makes decision\n- **No:** Is there an enduring power of attorney for health matters?\n- **Yes:** EPOA makes decision\n- **No:** Statutory Health Attorney\n- **No:** eg. family members, close friends, unpaid carer, Public Guardian  \n- **QCAT Order** appoints Guardian if necessary.  \n---  \nContact ADA Law: Phone: 1800 232 529, Email: info@adalaw.com.au, Website: www.adalaw.com.au  \nThis is general information only and does not constitute legal advice. If you have a specific legal problem, please consult your legal advisor.",
            "title": "Substitute Decision-Making"
          }
        ],
        "markdown": "# Healthcare Decision Making in Queensland Process\n\n## Presumption of Capacity & Consent\n\n- **Is it an emergency?**\n  - **Yes:** Consent not required\n  - **No:** Consent required\n\n- **Can the person validly consent or refuse?**\n  - **Yes:** The person makes decision\n  - **No/Unsure:** Is the person’s capacity in question?\n\n- **Is the person’s capacity in question?**\n  - **No:** The person makes decision\n  - **Yes:** Supported Decision-Making & Capacity Assessment\n\n## Supported Decision-Making & Capacity Assessment\n\n- **QLD capacity assessment guidelines** and **Capacity Assessment**\n  - **Has capacity?** Adequate for consent/refusal\n    - Implement Supported Decision-Making Strategies\n  - **Still unsure about capacity:**\n    - Inadequate for consent/refusal\n    - Substitute Decision-Making\n\n- **General capacity test:**\n  1. Understands the nature and effect of the decision\n  2. Freely & voluntarily making the decision\n  3. Can communicate the decision in some way\n  4. Assessed at the time decision is required\n\n## Substitute Decision-Making\n\n- **Include the adult by including their views, wishes, and preferences in the decision making**\n  - **Is there Advance Health Directive?**\n    - **Yes:** Advance Health Directive applies\n    - **No:** Is a Healthcare Guardian appointed?\n      - **Yes:** Healthcare Guardian makes decision\n      - **No:** Is there an enduring power of attorney for health matters?\n        - **Yes:** EPOA makes decision\n        - **No:** Statutory Health Attorney\n          - **No:** eg. family members, close friends, unpaid carer, Public Guardian\n\n- **QCAT Order** appoints Guardian if necessary.\n\n---\n\nContact ADA Law: Phone: 1800 232 529, Email: info@adalaw.com.au, Website: www.adalaw.com.au\n\nThis is general information only and does not constitute legal advice. If you have a specific legal problem, please consult your legal advisor.\n"
      },
      "recordId": "0",
      "errors": null,
      "warnings": null
    }
  ]
}
```

### Sample Skillset Integration

```json
"@odata.type": "#Microsoft.Skills.Custom.WebApiSkill",
            "description": "A custom skill that extract document contents using LLM and return markdown context-aware chunks",
            "uri": "https://{{hostname}}/process",
            "timeout": "PT230S",
            "batchSize": 1,
            "context": "/document",
            "httpHeaders": {
                "api-key": "{{api-key}}"
            },
            "httpMethod": "POST",
            "inputs": [
                {
                    "name": "blobUrl",
                    "source": "/document/metadata_storage_path"
                }
                /* Add here other settings as described in the Configuration section above */
            ],
            "outputs": [
                {
                    "name": "chunks",
                    "targetName": "chunks"
                }
            ]
        }
```