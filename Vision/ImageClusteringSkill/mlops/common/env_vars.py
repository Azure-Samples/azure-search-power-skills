"""
a set of variables that we are going to use in our pipeline
related code. Usually, they have to be defined in a DevOps
variable group or as environment variables
"""
import sys
import os
from dotenv import load_dotenv

load_dotenv()

workspace_name = os.environ.get("WORKSPACE_NAME")
resource_group = os.environ.get("RESOURCE_GROUP")
subscription_id = os.environ.get("SUBSCRIPTION_ID")
tenant_id = os.environ.get("TENANT_ID")
app_id = os.environ.get("SP_APP_ID")
app_secret = os.environ.get("SP_APP_SECRET")
region = os.environ.get("LOCATION")
vm_size = os.environ.get("AML_CLUSTER_CPU_SKU")
compute_name = os.environ.get("AML_CLUSTER_NAME")
build_id = os.environ.get("BUILD_BUILDID")
release_id = os.environ.get('RELEASE_RELEASEID')

source_branch = os.environ.get('BUILD_SOURCEBRANCHNAME')
model_name = f"{os.environ.get('MODEL_BASE_NAME')}_{source_branch}"

pipeline_name = f"{os.environ.get('PIPELINE_BASE_NAME')}_{source_branch}"

experiment_name = f"{os.environ.get('EXPERIMENT_BASE_NAME')}_{source_branch}"

vm_priority = os.environ.get("AML_CLUSTER_PRIORITY", 'lowpriority')
min_nodes = int(os.environ.get("AML_CLUSTER_MIN_NODES", 0))
max_nodes = int(os.environ.get("AML_CLUSTER_MAX_NODES", 4))
scale_down = int(os.environ.get("AML_CLUSTER_SCALE_DOWN", 600))
blob_datastore_name = os.environ.get("BLOB_DATASTORE_NAME")

storage_name = os.environ.get("STORAGE_NAME")
storage_key = os.environ.get("STORAGE_KEY")
storage_container = os.environ.get("STORAGE_CONTAINER")

if len(str(compute_name)) < 2 or len(str(compute_name)) > 16:
    print(
        f"Cannot use AmlCompute name: {compute_name}. Must be between 2 and 16 chars"
    )
    sys.exit(1)

if len(experiment_name) > 36:
    experiment_name = experiment_name[0:36]

if len(model_name) > 30:
    model_name = model_name[0:30]
