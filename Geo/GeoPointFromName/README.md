---
page_type: sample
languages:
- csharp
products:
- azure
- azure-search
- azure-maps
name: Get geo-point from name sample skill for cognitive search
description: "This custom skill takes a string input that represents a location (city, country, address or point of interest) and returns a geo-point."
azureDeploy: https://raw.githubusercontent.com/Azure-Samples/azure-search-power-skills/master/Geo/GeoPointFromName/azuredeploy.json
---

# GetGeoPointFromName

This custom skill takes a string input that represents a location (city, country, address or point of interest) and returns a geo-point with the coordinates for that location.

## Requirements

In addition to the common requirements described in [the root `README.md` file](../../README.md), this function requires access to an [Azure Maps](https://azure.microsoft.com/en-us/services/azure-maps/) service.

## Settings

This function requires a `AZUREMAPS_APP_KEY` setting set to a valid Azure Maps API key.
If running locally, this can be set in your project's debug environment variables (go to project properties, in the debug tab). This ensures your key won't be accidentally checked in with your code.
If running in an Azure function, this can be set in the application settings.

## Deployment

[![Deploy to Azure](https://azuredeploy.net/deploybutton.svg)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fazure-search-power-skills%2Fmaster%2FGeo%2FGeoPointFromName%2Fazuredeploy.json)

## Sample Input:

```json
{
    "values": 
    [
        {
           "recordId": "foo1",
           "data": { "address": "Guatemala City"}
        },
        {
           "recordId": "bar2",
           "data": { "address": "20019 8th Dr SE, Bothell WA, 98012"}
        }
    ]
}
```

## Sample Output:

```json
{
    "values": 
    [
        {
            "recordId": "foo1",
            "data": {
                "mainGeoPoint": {
                    "type": "Point",
                    "coordinates": [
                        -90.51557,
                        14.60043
                    ]
                },
                "results": [
                    {
                        "type": "POI",
                        "score": "4.203",
                        "position": {
                            "lat": "14.60043",
                            "lon": "-90.51557"
                        }
                    },
                    {
                        "type": "POI",
                        "score": "4.048",
                        "position": {
                            "lat": "10.3132",
                            "lon": "-85.7697"
                        }
                    },
                    "..."
                ]
            },
            "errors": [],
            "warnings": []
        },
        "..."
    ]
}
```

## Sample Skillset Integration

In order to use this skill in a cognitive search pipeline, you'll need to add a skill definition to your skillset.
Here's a sample skill definition for this example (inputs and outputs should be updated to reflect your particular scenario and skillset environment):

```json
{
    "@odata.type": "#Microsoft.Skills.Custom.WebApiSkill",
    "description": "Geo point from name",
    "context": "/document/merged_content/locations/*",
    "uri": "[AzureFunctionEndpointUrl]/api/geo-point-from-name?code=[AzureFunctionDefaultHostKey]",
    "batchSize": 1,
    "inputs": [
        {
            "name": "address",
            "source": "/document/merged_content/locations/*"
        }
    ],
    "outputs": [
        {
            "name": "mainGeoPoint",
            "targetName": "geopoint"
        }
    ],
    "httpHeaders": {}
}
```
