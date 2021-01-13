"""
The script is to support Azure DevOps Builds only
It's getting the latest model
"""
import argparse
from azureml.core.model import Model
from mlops.common.env_vars import model_name, \
    subscription_id, tenant_id, app_id, app_secret, \
    region, resource_group, workspace_name  # type: ignore
from mlops.common.workspace import get_workspace  # type: ignore


def main():
    """
    Download a model to a specified folder
    """
    parser = argparse.ArgumentParser("get_model")
    parser.add_argument(
        "--output_dir",
        type=str,
        required=True,
        help="Name of a dir to write model to"
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

    model = Model(aml_workspace, model_name, version=None)
    model.download(args.output_dir)


if __name__ == "__main__":
    main()