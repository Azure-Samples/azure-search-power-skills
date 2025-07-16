import json
from promptflow.core import tool


# The inputs section will change based on the arguments of the tool function, after you save the code
# Adding type to arguments and return value will help the system show the types properly
# Please update the function name/signature per need
@tool
def set_content(content: str,table:list,img:list)->list:
    result = []

    json_content = ''

    if content.find("```json") == -1:
        json_content = content
    else:
        start = content.find('```json')+7
        end = content.find('}]') +2
        json_content = content[start:end]

    # start = content.index('```json')+7

    

    # end = content.index('}]') +2

    # jsonstr = content[start:end]

    # json_content = json.loads(jsonstr.replace('"]','"}]').replace('}  { ','},{ ').replace('   }','"}').replace('],[ ',',').replace('*  }','*"  }'))

    json_content = json.loads(json_content)
    
    print('Extracting test from pdf file...')

    for item in json_content:
        print(item)
        result.append(item)

    
    print('Extracting table from pdf file...')

    for item in table:
        print(item)
        result.append(item)

    

    print('Extracting images from pdf file...')

    for item in img:
        print(item)
        result.append(json.loads(item))

    return result
