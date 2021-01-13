"""
The script is to support Azure DevOps Builds only
It's getting details of the just published pipeline
"""
import argparse
from azureml.pipeline.core import PublishedPipeline  # type: ignore
from mlops.common.env_vars import pipeline_name, build_id, \
    subscription_id, tenant_id, app_id, app_secret, \
    region, resource_group, workspace_name, experiment_name  # type: ignore
from mlops.common.workspace import get_workspace  # type: ignore


def main():
    """
    Read details of the published pipelines (based on name and build id)
    and save it to a provided file

    Raises:
        KeyError: if pipeline has not been found
        ValueError: if there is more than one pipeline
    """
    parser = argparse.ArgumentParser("get_pipeline")
    parser.add_argument(
        "--output_pipeline_file",
        type=str,
        default="pipeline_info.txt",
        help="Name of a file to write pipeline info to"
    )

    args = parser.parse_args()

    aml_workspace = get_workspace(
        workspace_name,
        resource_group,
        subscription_id,
        tenant_id,
        app_id,
        app_secret,
        region,
        create_if_not_exist=False)
    print(aml_workspace)

    # Find the pipeline that was published by the specified build ID
    pipelines = PublishedPipeline.list(aml_workspace)
    matched_pipes = []

    print(pipeline_name)

    for pipe in pipelines:
        # now it will look both through the pipeline version and names
        if pipe.name == pipeline_name and pipe.version == str(build_id):
            matched_pipes.append(pipe)

    if len(matched_pipes) > 1:
        raise ValueError(f"Multiple active pipelines are published for build {build_id}.")
    if len(matched_pipes) == 0:
        raise KeyError(f"Unable to find a published {pipeline_name} for this build {build_id}")

    published_pipeline = matched_pipes[0]
    if args.output_pipeline_file is not None:
        with open(args.output_pipeline_file, "w") as out_file:
            out_file.write(published_pipeline.id + "\n")
            out_file.write(experiment_name + "\n")
            out_file.write(published_pipeline.endpoint)


if __name__ == "__main__":
    main()
