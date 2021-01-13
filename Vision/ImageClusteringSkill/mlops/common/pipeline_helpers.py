"""
This is a helper for all Azure ML Pipelines
"""
from azureml.core.runconfig import Environment, CondaDependencies  # type: ignore
from azureml.pipeline.core import Pipeline, PublishedPipeline  # type: ignore
from .workspace import get_workspace
from .attach_compute import get_compute


# pylint: disable=too-many-arguments
def pipeline_base(
    workspace_name: str,
    resource_group: str,
    subscription_id: str,
    tenant_id: str,
    app_id: str,
    app_secret: str,
    region: str,
    compute_name: str,
    vm_size: str,
    vm_priority: str,
    min_nodes: int,
    max_nodes: int,
    scale_down: int
):
    """
    Gets AzureML artifacts: AzureML Workspace, AzureML Compute Tagret and AzureMl Run Config
    Parameters:
        workspace_name (str): The name of the workspace
        resource_group (str): The name of the workspace resource group
        subscription_id (str): Workspace subscription ID
        tenant_id (str): Workspace tenant ID
        app_id (str): Workspace service principal ID
        app_secret (str): Workspace service principal secret
        region (str): workspace region
        compute_name (str): The name of the workspace compute cluster
        vm_size (str): Workspace compute cluster size
        vm_priority (str): Workspace compute cluster priority
        min_nodes (int): Compute cluster min nodes
        max_nodes (int): Compute cluster max nodes
        scale_down (int): Compute cluster scale down timeout
        vnet_name (str): Compute cluster virtual network name
        vnet_subnet_name (str): Compute cluster virtual network subnet name
        vnet_resourcegroup_name (str): Compute cluster virtual network resource group
    Returns:
        Workspace: a reference to the current workspace
        ComputeTarget: compute cluster object
        Environment: environment for compute instances
    """
    # Get Azure machine learning workspace
    aml_workspace = get_workspace(workspace_name,
                                  resource_group,
                                  subscription_id,
                                  tenant_id,
                                  app_id,
                                  app_secret,
                                  region,
                                  create_if_not_exist=False)
    print(aml_workspace)

    # Get Azure machine learning cluster
    aml_compute = get_compute(
        aml_workspace,
        compute_name,
        vm_size,
        vm_priority,
        min_nodes,
        max_nodes,
        scale_down,
    )

    if aml_compute is not None:
        print(aml_compute)

        batch_conda_deps = CondaDependencies.create(
            conda_packages=["python==3.8.5"],
            pip_packages=[
                'numpy==1.18.5',
                'pandas==1.1.3',
                'pillow==7.2.0',
                'pyarrow==1.0.1',
                'scikit-image==0.17.2',
                'scikit-learn==0.23.2',
                'scipy==1.5.2',
                'tqdm==4.48.2',
                'opencv-python-headless',
                'tensorflow==2.3.0',
                'PyYAML==5.3.1',
                'azureml-core==1.16.0'
            ],
        )
        batch_env = Environment(name="train-env")
        batch_env.docker.enabled = True
        batch_env.python.conda_dependencies = batch_conda_deps

    return aml_workspace, aml_compute, batch_env


def publish_pipeline(aml_workspace, steps, pipeline_name, build_id) -> PublishedPipeline:
    """
    Publishes a pipeline to the AzureML Workspace
    Parameters:
      aml_workspace (Workspace): existing AzureML Workspace object
      steps (list): list of PipelineSteps
      pipeline_name (string): name of the pipeline to be published
      build_id (string): DevOps Pipeline Build Id

    Returns:
        PublishedPipeline
    """
    train_pipeline = Pipeline(workspace=aml_workspace, steps=steps)
    train_pipeline.validate()
    published_pipeline = train_pipeline.publish(name=pipeline_name,
                                                description="Model training/retraining pipeline",
                                                version=build_id)
    print(f'Published pipeline: {published_pipeline.name} for build: {build_id}')

    return published_pipeline
