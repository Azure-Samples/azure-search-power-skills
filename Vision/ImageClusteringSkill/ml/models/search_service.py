import requests
import json
from typing import List, Dict

class SearchService:
    
    def __init__(self,
                 api_key,
                 endpoint,
                 cog_svcs_key):
        
        """Constructs an instance of the SearchService class

        Parameters
        ----------
        api_key : str
            Search service key
        endpoint : str
            Search endpoint
        cog_svcs_key: str
            Cognitive Services Key
        """

        self.search_api_key = api_key
        self.search_endpoint = endpoint
        self.cog_svcs_key = cog_svcs_key


    def construct_Url(self,
                      service: str,
                      resource: str,
                      resource_name: str,
                      action: str,
                      api_version: str) -> str:
        
        """Constructs urls to make API requests

        Parameters
        ----------
        service : str
            Service being used
        resource : str
            Resource being used
        resource_name:
            Name of resource being used
        action: str
            Action being performed
        api_version: str
            Version of API being used
            
        Returns
        -------
        
        str:
            Constructed URL 
        """

        if resource_name:
            if action:
                return service + '/'+ resource + '/' + resource_name + '/' + action + '?api-version=' + api_version
            else:
                return service + '/'+ resource + '/' + resource_name + '?api-version=' + api_version
        else:
            return service + '/'+ resource + '?api-version=' + api_version


    def create_data_source(self,
                           datasource_container: str,
                           storage_connection_string: str,
                           description: str,
                           datasource_type: str = "azureblob",
                           datasource_subtype: str = None,
                           content_type: str = 'application/json',
                           api_version: str ='2019-05-06-Preview'
                           ) -> Dict:
        
        """Function used to create and connect data source
        for an azure search instance

        Parameters
        ----------
        datasource_container : str
            Container that will be registered
            as a data source
        storage_connection_string: str
            Connection String to Storage Account
        description: str
            Description for data source
        datasource_type: str
            Data source type
        datasource_subtype: str
            Data source sub type
        content_type : str
            Content type used for API request
        api_version: str
            Version of API used
            
        Return
        ------
        
        Dict
            JSON response from API
        """

        # set headers
        headers = {'api-key': self.search_api_key, 'Content-Type': content_type}
        #create datasource refereference
        datsource_def = {
            'name': f'{datasource_container}-ds',
            'description': f'{description}',
            'type': datasource_type,
            'subtype': datasource_subtype,
            'credentials': {
                'connectionString': f'{storage_connection_string}'
            },
            'container': {
                'name': f'{datasource_container}'
            },
        }

        #make post request to create data soruce with confugured parameters
        r = requests.post(self.construct_Url(self.search_endpoint, "datasources", None, None, api_version),
                          data=json.dumps(datsource_def),  headers=headers)
        print(r)
        res = r.json()
        print(json.dumps(res, indent=2))
        return res
    
    def create_skillset(self,
                        datasource_container: str,
                        skills: List[Dict],
                        knowledge_store: Dict,
                        description: str,
                        content_type: str = 'application/json',
                        api_version: str ='2019-05-06-Preview'
                        ) -> Dict:

        """Function used to create skillset for Azure Search

        Parameters
        ----------
        datasource_container : str
            Container that will be registered
            as a data source
        skills: Array
            Set of skills that will be used to build skillset
        knowledge_store: Dict
            Knowledge store that will be utilized
            by skillset for search service
        description: str
            Description for skillset
        content_type : str
            Content type used for API request
        api_version: str
            Version of API used
            
        Return
        ------
        
        Dict
            JSON response from API
        """
        
    
        # set headers
        headers = {'api-key': self.search_api_key, 'Content-Type': content_type}

        skillset_name = f'{datasource_container}-ss'
        skillset_def = {
            'name': f'{skillset_name}',
            'description': description,
            'skills': skills,
            'cognitiveServices': {
                '@odata.type': '#Microsoft.Azure.Search.CognitiveServicesByKey',
                'description': '/subscriptions/subscription_id/resourceGroups/resource_group/providers/Microsoft.CognitiveServices/accounts/cog_svcs_acct',
                'key': f'{self.cog_svcs_key}'
            },
            'knowledgeStore': knowledge_store
        }

        #make put request to create skillset
        r = requests.put(self.construct_Url(service=self.search_endpoint,
                                            resource="skillsets",
                                            resource_name=skillset_name,
                                            action=None,
                                            api_version=api_version),
                         data=json.dumps(skillset_def),  headers=headers)
        print(r)
        res = r.json()
        print(json.dumps(res, indent=2))
        return res
    
    def create_index(self,
                     datasource_container: str,
                     fields: List[Dict],
                     default_scoring_profile: str = "",
                     scoring_profiles: List = [],
                     cors_options: List = None,
                     suggesters: List = None,
                     analyzers: List = [],
                     tokenizers: List =[],
                     token_filters: List =[],
                     char_filters:List =[],
                     encryption_key: str = None,
                     similarity:str = None,
                     content_type: str = 'application/json',
                     api_version: str ='2019-05-06-Preview'
                     ) -> Dict:
        
        """Function used to create an index fora serach service

        Parameters
        ----------
        datasource_container : str
            Container that will be registered
            as a data source
        fields: Array
            Fields that will be created
            and utilized for this index
        default_scoring_profile: str
            Default scoring profiel for 
            the index
        scoring_profiles: Array
            Scoring profiles that will be utilized
            by the index
        cors_options: Array
            CORS configuration
        suggesters: Array
            Suggesters that will be used for
            the index
        analyzers: Array
            Set of analyzers that will be utilized
            by the index
        tokenizers: Array
            Set of tokenizers that will be
            used by the index
        token_filters: Array
            Set of token filters that will
            be utilized by the service
        char_filters: Array
            Set of character filters that will be used
            for the index
        encryption_key: str
            Optionally set encryption key
            for the index
        similarity: str
            Set similarity targets for index
        content_type : str
            Content type used for API request
        api_version: str
            Version of API used
            
        Return
        ------
        
        Dict
            JSON response from API
        """
        
    
        # set headers
        headers = {'api-key': self.search_api_key, 'Content-Type': content_type}

        indexname = f'{datasource_container}-idx'
        index_def = {
            "name":f'{indexname}',
              "defaultScoringProfile": default_scoring_profile,
            "fields": fields,
            "scoringProfiles": scoring_profiles,
            "corsOptions": cors_options,
            "suggesters": suggesters,
            "analyzers": analyzers,
            "tokenizers": tokenizers,
            "tokenFilters": token_filters,
            "charFilters": char_filters,
            "encryptionKey": encryption_key,
            "similarity": similarity
        }
        r = requests.post(self.construct_Url(service=self.search_endpoint,
                                             resource="indexes",
                                             resource_name=None,
                                             action=None,
                                             api_version=api_version),
                          data=json.dumps(index_def),  headers=headers)
        print(r)
        res = r.json()
        print(json.dumps(res, indent=2))
        return res
    
    def create_indexer(self,
                       datasource_container: str,
                       description: str,
                       parameters: Dict,
                       field_mappings: List[Dict],
                       output_field_mappings: List[Dict],
                       know_store_cache: str,
                       content_type: str = 'application/json',
                       api_version: str ='2019-05-06-Preview'
                       ) -> Dict:
        
        """Function used to create indexer for search service

        Parameters
        ----------
        datasource_container : str
            Container that will be registered
            as a data source
        description: str
            Description for indexer
        parameters: Dict
            Set of parameters for indexer
        field_mappings: Array
            Set of field mappings thats will
            be utilized by the indexer
        output_field_mappings: Array
            Set of output field mappings thats will
            be utilized by the indexer
        know_store_cache: str
            Knowledge Store Cache
            that will be accessed by indexer
        content_type : str
            Content type used for API request
        api_version: str
            Version of API used
            
        Return
        ------
        
        Dict
            JSON response from API
        """

        # set headers
        headers = {'api-key': self.search_api_key, 'Content-Type': content_type}    

        indexername = f'{datasource_container}-idxr'
        indexer_def = {
            "name": f'{indexername}',
            "description": description,
            "dataSourceName": f'{datasource_container}-ds',
            "skillsetName": f'{datasource_container}-ss',
            "targetIndexName": f'{datasource_container}-idx',
            "disabled": None,
            "schedule": {
                "interval": "PT2H",
                "startTime": "0001-01-01T00:00:00Z"
              },
            "parameters": parameters,
            "fieldMappings": field_mappings,
            "outputFieldMappings": output_field_mappings,
            "cache": {
                "enableReprocessing": True,
                "storageConnectionString": f'{know_store_cache}'
            }
        }

        r = requests.post(self.construct_Url(service=self.search_endpoint,
                                             resource="indexers",
                                             resource_name=None,
                                             action=None,
                                             api_version=api_version),
                          data=json.dumps(indexer_def),  headers=headers)
        print(r)
        res = r.json()
        print(json.dumps(res, indent=2))
        return res
    
    def run_indexer(self,
                    indexer_name: str,
                    content_type: str = 'application/json',
                    api_version: str ='2019-05-06-Preview'
                    ) -> Dict:
        
        """Function used to run indexer for a search service

        Parameters
        ----------
        indexer_name : str
            Name of Indexer 
        content_type : str
            Content type used for API request
        api_version: str
            Version of API used
            
        Return
        ------
        
        Dict
            JSON response from API
        """
        
        # set headers
        headers = {'api-key': self.search_api_key, 'Content-Type': content_type}
        
        r = requests.post(self.construct_Url(service=self.search_endpoint,
                                             resource="indexers",
                                             resource_name=indexer_name,
                                             action="run",
                                             api_version=api_version),
                          data=None,  headers=headers)
        print(r)
        res = r.json()
        print(json.dumps(res, indent=2))
        return res

    def get_indexer_status(self,
                           indexer_name: str,
                           content_type: str = 'application/json',
                           api_version: str ='2019-05-06-Preview'
                           ) -> Dict:
        
        """Function used to get indexer status for search service

        Parameters
        ----------
        indexer_name : str
            Name of Indexer 
        content_type : str
            Content type used for API request
        api_version: str
            Version of API used
            
        Return
        ------
        
        Dict
            JSON response from API
        """
        
        # set headers
        headers = {'api-key': self.search_api_key, 'Content-Type': content_type}
        
        r = requests.get(self.construct_Url(service=self.search_endpoint,
                                            resource="indexers",
                                            resource_name=indexer_name,
                                            action="status",
                                            api_version=api_version),
                         data=None,  headers=headers)
        print(r)
        res = r.json()
        print(json.dumps(res, indent=2))
        #print(res['lastResult']['status'] + ', ' + str(res['lastResult']['itemsProcessed'] ))
        
    def update_skillset(self,
                        datasource_container: str,
                        skills: List[Dict],
                        knowledge_store: Dict,
                        description: str,
                        content_type: str = 'application/json',
                        api_version: str ='2019-05-06-Preview'
                        ) -> Dict:
        
        """Function used to update skillset for Azure Search

        Parameters
        ----------
        datasource_container : str
            Container that will be registered
            as a data source
        skills: Array
            Set of skills that will be used to build skillset
        knowledge_store: Dict
            Knowledge store that will be utilized
            by skillset for search service
        description: str
            Description for skillset
        content_type : str
            Content type used for API request
        api_version: str
            Version of API used
            
        Return
        ------
        
        Dict
            JSON response from API
        """
    
        # set headers
        headers = {'api-key': self.search_api_key, 'Content-Type': content_type}

        skillset_name = f'{datasource_container}-ss'
        skillset_def = {
            'name': f'{skillset_name}',
            'description': description,
            'skills': skills,
            'cognitiveServices': {
                '@odata.type': '#Microsoft.Azure.Search.CognitiveServicesByKey',
                'description': '/subscriptions/subscription_id/resourceGroups/resource_group/providers/Microsoft.CognitiveServices/accounts/cog_svcs_acct',
                'key': f'{self.cog_svcs_key}'
            },
            'knowledgeStore': knowledge_store
        }

        #make put request to create skillset
        r = requests.put(self.construct_Url(service=self.search_endpoint,
                                            resource="skillsets",
                                            resource_name=skillset_name,
                                            action=None,
                                            api_version=api_version),
                         data=json.dumps(skillset_def),  headers=headers)
        print(r)
        res = r.json()
        print(json.dumps(res, indent=2))
        return res
        
    def update_indexer(self,
                       datasource_container: str,
                       description: str,
                       parameters: Dict,
                       field_mappings: List[Dict],
                       output_field_mappings: List[Dict],
                       know_store_cache: str,
                       content_type: str = 'application/json',
                       api_version: str ='2019-05-06-Preview'
                       ) -> Dict:
        
        """Function used to update indexer for search service

        Parameters
        ----------
        datasource_container : str
            Container that will be registered
            as a data source
        description: str
            Description for indexer
        parameters: Dict
            Set of parameters for indexer
        field_mappings: Array
            Set of field mappings thats will
            be utilized by the indexer
        output_field_mappings: Array
            Set of output field mappings thats will
            be utilized by the indexer
        know_store_cache: str
            Knowledge Store Cache
            that will be accessed by indexer
        content_type : str
            Content type used for API request
        api_version: str
            Version of API used
            
        Return
        ------
        
        Dict
            JSON response from API
        """

        # set headers
        headers = {'api-key': self.search_api_key, 'Content-Type': content_type}    

        indexername = f'{datasource_container}-idxr'
        indexer_def = {
            "name": f'{indexername}',
            "description": description,
            "dataSourceName": f'{datasource_container}-ds',
            "skillsetName": f'{datasource_container}-ss',
            "targetIndexName": f'{datasource_container}-idx',
            "disabled": None,
            "schedule": {
                "interval": "PT2H",
                "startTime": "0001-01-01T00:00:00Z"
              },
            "parameters": parameters,
            "fieldMappings": field_mappings,
            "outputFieldMappings": output_field_mappings,
            "cache": {
                "enableReprocessing": True,
                "storageConnectionString": f'{know_store_cache}'
            }
        }

        r = requests.put(self.construct_Url(service=self.search_endpoint,
                                            resource="indexers",
                                            resource_name=indexername,
                                            action=None,
                                            api_version=api_version),
                          data=json.dumps(indexer_def),  headers=headers)
        print(r)
        res = r.json()
        print(json.dumps(res, indent=2))
        return res