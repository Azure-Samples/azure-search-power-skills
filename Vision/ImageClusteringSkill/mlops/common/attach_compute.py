"""
Create a compute cluster or return a reference to an existing one
"""
import sys
from azureml.core import Workspace  # type: ignore
from azureml.core.compute import AmlCompute  # type: ignore
from azureml.core.compute import ComputeTarget  # type: ignore
from azureml.exceptions import ComputeTargetException  # type: ignore


def get_compute(workspace: Workspace, compute_name: str, vm_size: str,
                vm_priority: str, min_nodes: int, max_nodes: int, scale_down: int):
    """
    Returns an existing compute or creates a new one.
    Args:
      workspace: Workspace: AzureML workspace
      compute_name: str: name of the compute
      vm_size: str: VM size
      vm_priority: str: low priority or dedicated cluster
      min_nodes: int: minimum number of nodes
      max_nodes: int: maximum number of nodes in the cluster
      scale_down: int: number of seconds to wait before scaling down the cluster
    Returns:
        ComputeTarget: a reference to compute
    """

    try:
        if compute_name in workspace.compute_targets:
            compute_target = workspace.compute_targets[compute_name]
            if compute_target and isinstance(compute_target, AmlCompute):
                print(f"Found existing compute target {compute_name} so using it.")
        else:
            compute_config = AmlCompute.provisioning_configuration(
                vm_size=vm_size,
                vm_priority=vm_priority,
                min_nodes=min_nodes,
                max_nodes=max_nodes,
                idle_seconds_before_scaledown=scale_down
            )

            compute_target = ComputeTarget.create(workspace, compute_name,
                                                  compute_config)
            compute_target.wait_for_completion(show_output=True)
        return compute_target
    except ComputeTargetException as ex_var:
        print(ex_var)
        print('An error occurred trying to provision compute.')
        sys.exit(-1)
