# Hello World (template)

 This "Hello World" custom skills can be used as a template to create your own skills.

[![Deploy to Azure](https://azuredeploy.net/deploybutton.svg)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fazure-search-power-skills%2Fmaster%2FTemplate%2FHelloWorld%2Fazuredeploy.json)

## Requirements

This skill has no additional requirements than the ones described in [the root `README.md` file](../../README.md).

## Settings

This function doesn't require any application settings.

## hello-world

### Sample Input:

```json
{
    "values": [
        {
            "recordId": "r1",
            "data":
            {
            	"name": "World"
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
            "recordId": "r1",
            "data": {
                "greeting": "Hello, World"
            },
            "errors": [],
            "warnings": []
        }
    ]
}
```
