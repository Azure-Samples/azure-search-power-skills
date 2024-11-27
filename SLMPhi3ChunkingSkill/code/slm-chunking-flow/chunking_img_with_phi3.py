import os
import json
from azure.ai.inference import ChatCompletionsClient
from azure.ai.inference.models import (
    UserMessage,
    TextContentItem,
    ImageContentItem,
    ImageUrl,
    ImageDetailLevel,
)
from azure.core.credentials import AzureKeyCredential
from promptflow.core import tool



endpoint = "https://models.inference.ai.azure.com"
model_name = "Phi-3.5-vision-instruct"
token =  'Your GitHub Model Token'
# The inputs section will change based on the arguments of the tool function, after you save the code
# Adding type to arguments and return value will help the system show the types properly
# Please update the function name/signature per need
@tool
def chunking_img_with_phi3(imgs: dict) -> list:

    client = ChatCompletionsClient(
        endpoint=endpoint,
        credential=AzureKeyCredential(token),
    )



    imgList = []

    for item in imgs:

        response = client.complete(
            messages=[
                UserMessage(
                    content=[
                        TextContentItem(text="""You are my analysis assistant, help me analyze charts, flowchart, etc. according to the following conditions

    1. If it is a chart, please analyze according to the data in the chart and tell me the different details

    2. If it is a flowchart, please analyze all the situations in detail according to the flow and  describe all process in details, do NOT simplify. Use bullet lists with identation to describe the process

    3. The output is json {"chunking":"......"}
                                    
    4. If it is not a chart or flowchart(more than single node),it does not need to be analyzed, the output is json {"chunking":"NIL"}                           
                                    
    """),
                        ImageContentItem(
                            image_url=ImageUrl.load(
                                image_file=item["image_path"],
                                image_format="png",
                                detail=ImageDetailLevel.AUTO)
                        ),
                    ],
                ),
            ],
            temperature=0.2,
            top_p=1.0,
            max_tokens=3000,
            model=model_name,
        )

        if response.choices[0].message.content == "":
            continue

        result = json.loads(response.choices[0].message.content.replace('\n','').replace('```json','').replace('```',''))

        if result["chunking"] == "NIL":
            continue
            
        imgList.append(response.choices[0].message.content.replace('\n','').replace('```json','').replace('```',''))



    return imgList
