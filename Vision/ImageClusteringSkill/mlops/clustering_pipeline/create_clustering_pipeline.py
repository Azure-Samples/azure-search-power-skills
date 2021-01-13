"""
A script to create and publish the clustering pipeline
We are running it as a module to solve import problems
"""
from azureml.core import RunConfiguration  # type: ignore
from azureml.core.compute import ComputeTarget  # type: ignore
from azureml.core.datastore import Datastore  # type: ignore
from azureml.core.runconfig import Environment
from azureml.pipeline.core import PipelineParameter
from azureml.data.data_reference import DataReference  # type: ignore
from azureml.pipeline.steps import PythonScriptStep  # type: ignore
from mlops.common.env_vars import (app_id, app_secret, blob_datastore_name, build_id, compute_name,
                                   max_nodes, min_nodes, pipeline_name, region, resource_group,
                                   scale_down, storage_container, storage_key, storage_name, model_name,
                                   subscription_id, tenant_id, vm_priority, vm_size, workspace_name)
from mlops.common.get_datastores import get_blob_datastore
from mlops.common.pipeline_helpers import (pipeline_base, publish_pipeline)


def main():
    """
    Creates a pipeline

    Returns:
        string: pipeline id
    """
    print("Getting base pipeline")
    aml_workspace, aml_compute, batch_env = pipeline_base(
        workspace_name,
        resource_group,
        subscription_id,
        tenant_id,
        app_id,
        app_secret,
        region,
        compute_name,
        vm_size,
        vm_priority,
        min_nodes,
        max_nodes,
        scale_down
    )

    print("Get Blob datastore")
    blob_ds = get_blob_datastore(aml_workspace, blob_datastore_name, storage_name, storage_key, storage_container)

    print("Get pipeline steps")
    steps = get_pipeline(aml_compute, blob_ds, batch_env)

    print("Publishing pipeline")
    published_pipeline = publish_pipeline(aml_workspace, steps, pipeline_name, build_id)

    print(f"Pipeline ID: {published_pipeline.id}")
    return published_pipeline.id


def get_pipeline(aml_compute: ComputeTarget, blob_ds: Datastore, batch_env: Environment) -> str:
    """
    Creates clustering pipeline steps

    Parameters:
        aml_compute (ComputeTarget): a reference to compute
        blob_ds (DataStore): a reference to compute
        batch_env (Environment): a reference to environment object

    Returns:
        string: published pipeline id
    """
    input_dir = DataReference(datastore=blob_ds, data_reference_name="input_dir", path_on_datastore="books", mode="mount")
    fraction = PipelineParameter(name="fraction", default_value=1.0)
    recursive = PipelineParameter(name="recursive", default_value=False)
    eps = PipelineParameter(name="eps", default_value=0.64)
    min_samples = PipelineParameter(name="min_samples", default_value=3)
    metric = PipelineParameter(name="metric", default_value="cosine")

    step_config = RunConfiguration()
    step_config.environment = batch_env
    step = PythonScriptStep(name="train_step",
                                  script_name="mlops/clustering_pipeline/steps/train.py",
                                  runconfig=step_config,
                                  arguments=[
                                      "--input_dir",
                                      input_dir,
                                      "--fraction",
                                      fraction,
                                      "--recursive",
                                      recursive,
                                      "--eps",
                                      eps,
                                      "--min_samples",
                                      min_samples,
                                      "--metric",
                                      metric,
                                      "--model_name",
                                      model_name
                                  ],
                                  inputs=[input_dir],
                                  outputs=[],
                                  compute_target=aml_compute,
                                  allow_reuse=False)

    print("Steps Created")

    steps = [step]

    print(f"Returning {len(steps)} steps")
    return steps


if __name__ == '__main__':
    main()
