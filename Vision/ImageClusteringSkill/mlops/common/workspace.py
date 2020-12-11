"""
Return a reference to AML workspace
"""
import sys
from azureml.core import Workspace  # type: ignore
from azureml.core.authentication import InteractiveLoginAuthentication, ServicePrincipalAuthentication  # type: ignore
from azureml.exceptions import WorkspaceException


def get_workspace(
    name: str,
    resource_group: str,
    subscription_id: str,
    tenant_id: str,
    app_id: str,
    app_secret: str,
    region: str,
    create_if_not_exist=False,
):
    """

    Parameters:
      name (str): name of the workspace
      resource_group (str): resource group name
      subscription_id (str): subscription id
      tenant_id (str): tenant id (aad id)
      app_id (str): service principal id
      app_secret (str): service principal password
      region (str): location of the workspace
      create_if_not_exist (bool): Default value is False

    Returns:
      Workspace: a reference to a workspace
    """
    service_principal = ServicePrincipalAuthentication(
        tenant_id=tenant_id,
        service_principal_id=app_id,
        service_principal_password=app_secret,
    )

    try:
        aml_workspace = Workspace.get(
            name=name,
            subscription_id=subscription_id,
            resource_group=resource_group,
            auth=service_principal,
        )

    except WorkspaceException as exp_var:
        print(f"Error while retrieving Workspace...: {exp_var}")
        if create_if_not_exist:
            print(f"Creating AzureML Workspace: {name}")
            aml_workspace = Workspace.create(
                name=name,
                subscription_id=subscription_id,
                resource_group=resource_group,
                create_resource_group=True,
                location=region,
                auth=service_principal,
            )
            print(f"{aml_workspace.name} created.")
        else:
            sys.exit(-1)

    return aml_workspace


def get_workspace_interactive_login(
    tenant_id: str, subscription_id: str, resource_group: str, workspace_name: str
):
    """Get AML Workspace using the InteractiveLogin option with Azure. Intended to be used with running scripts

    Parameters
    ----------
    tenant_id : str
        Tenant ID with proper permissions for the given AML Workspace
    subscription_id : str
        Subscription ID for the given AML Workspace
    resource_group : str
        Resource group that the AML Workspace is in
    workspace_name : str
        Name of the AML Workspace

    Returns
    -------
    aml_workspace : Workspace
        Instance of AML Workspace
    """
    interactive_auth = InteractiveLoginAuthentication(tenant_id=tenant_id)

    aml_workspace = Workspace(
        subscription_id=subscription_id,
        resource_group=resource_group,
        workspace_name=workspace_name,
        auth=interactive_auth,
    )

    return aml_workspace
