"""
This method returns a reference to an existing datastore or create a new one
"""
from msrest.exceptions import HttpOperationError  # type: ignore
from azureml.core import Workspace  # type: ignore
from azureml.core.datastore import Datastore  # type: ignore


def get_blob_datastore(workspace: Workspace, data_store_name: str,
                       storage_name: str, storage_key: str,
                       container_name: str):
    """
    Returns a reference to a datastore
    Parameters:
      workspace (Workspace): existing AzureML Workspace object
      data_store_name (string): data store name
      storage_name (string): blob storage account name
      storage_key (string): blob storage account key
      container_name (string): container name

    Returns:
        Datastore: a reference to datastore
    """
    try:
        blob_datastore = Datastore.get(workspace, data_store_name)
        print(f"Found Blob Datastore with name: {data_store_name}")
    except HttpOperationError:
        blob_datastore = Datastore.register_azure_blob_container(
            workspace=workspace,
            datastore_name=data_store_name,
            account_name=storage_name,  # Storage account name
            container_name=container_name,  # Name of Azure blob container
            account_key=storage_key)  # Storage account key
    print(f"Registered blob datastore with name: {data_store_name}")
    return blob_datastore
