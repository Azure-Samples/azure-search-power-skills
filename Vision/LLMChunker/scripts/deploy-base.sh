#!/bin/bash
set -e

#
# This script is used to run the base deployment
#

script_dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

# Load deploy env vars
if [[ ! -f  "$script_dir/../deploy.env" ]]; then
    echo "deploy.env not found - see deploy.env-example"
    exit 1
fi

set -o allexport
source "$script_dir/../deploy.env"
set +o allexport

cd "$script_dir/../deployment/base"

# Extract the current Azure CLI subscription ID
SUBSCRIPTION_ID=$(az account show --query id --output tsv)

terraform init -var="subscription_id=$SUBSCRIPTION_ID"

terraform apply -var="subscription_id=$SUBSCRIPTION_ID"

# Save output for use in later stages
mkdir -p "$script_dir/../deployment/outputs"
terraform output -json > "$script_dir/../deployment/outputs/base.json"
"$script_dir/convert-json-to-env.sh" < "$script_dir/../deployment/outputs/base.json" > "$script_dir/../base.env"