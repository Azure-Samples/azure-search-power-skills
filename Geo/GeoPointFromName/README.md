# GetGeoPointFromName

 This custom skill takes a string input that represents a location (city, country, address or point of interest) and returns a geo-point with the coordinates for that location.

[![Deploy to Azure](https://azuredeploy.net/deploybutton.svg)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fazure-search-power-skills%2Fmaster%2FGeo%2FGeoPointFromName%2Fazuredeploy.json)

## Requirements

In addition to the common requirements described in [the root `README.md` file](../../README.md), this function requires access to an [Azure Maps](https://azure.microsoft.com/en-us/services/azure-maps/) service.

## Settings

This functions requires a `AZUREMAPS_APP_KEY` setting set to a valid Azure Maps API key.
If running locally, this can be set in your project's debug environment variables (go to project properties, in the debug tab). This ensures your key won't be accidentally checked in with your code.
If running in an Azure function, this can be set in the application settings.

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
