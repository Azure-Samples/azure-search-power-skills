# Cryptonym Linker

 These two custom skills (`link-cryptonyms` and `link-cryptonyms-list`) give definitions for known acronyms.

[![Deploy to Azure](https://azuredeploy.net/deploybutton.svg)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fazure-search-power-skills%2Fmaster%2FText%2FCryptonymLinker%2Fazuredeploy.json)

## Requirements

These skills have no additional requirements than the ones described in [the root `README.md` file](../../README.md).

## Settings

This function uses a JSON file called `acronyms.json` that can be found at the root of this project, and that will be deployed with the function. This file contains a simple dictionary of acronyms to definitions. We provided a sample file with this project that contains definitions for common computer-related acronyms. Please replace this file with your own data, or point `LinkCryptonyms` to your data.

## link-cryptonyms

### Sample Input:

```json
{
    "values": [
        {
            "recordId": "foobar2",
            "data":
            {
                "word":  "MS"
            }
        },
        {
            "recordId": "foo1",
            "data":
            {
                "word":  "SSL"
            }
        }
    ]
}
```

### Sample Output:

```json
{
    "values": [
        {
            "recordId": "foobar2",
            "data": {
                "cryptonym": {
                    "value": "MS",
                    "description": "Microsoft"
                }
            },
            "errors": [],
            "warnings": []
        },
        {
            "recordId": "foo1",
            "data": {
                "cryptonym": {
                    "value": "SSL",
                    "description": "Secure Socket Layer"
                }
            },
            "errors": [],
            "warnings": []
        }
    ]
}
```

## link-cryptonyms-list

### Sample Input:

```json
{
    "values": [
        {
            "recordId": "foobar2",
            "data":
            {
                "words": [ "MS",  "SSL" ]
            }
        }
    ]
}
```

### Sample Output:

```json
{
    "values": [
        {
            "recordId": "foobar2",
            "data": {
                "cryptonyms": [
                    {
                        "value": "MS",
                        "description": "Microsoft"
                    },
                    {
                        "value": "SSL",
                        "description": "Secure Socket Layer"
                    }
                ]
            },
            "errors": [],
            "warnings": []
        }
    ]
}
```
