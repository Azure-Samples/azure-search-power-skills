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

# Load base env vars
if [[ ! -f  "$script_dir/../base.env" ]]; then
    echo "base.env not found - see base.env-example"
    exit 1
fi
set -o allexport
source "$script_dir/../base.env"
set +o allexport

cd "$script_dir/../deployment/skill"

SUBSCRIPTION_ID=$(az account show --query id --output tsv)

terraform init -var="subscription_id=$SUBSCRIPTION_ID"

terraform apply -var="subscription_id=$SUBSCRIPTION_ID"

# Save output for use in later stages
mkdir -p "$script_dir/../deployment/outputs"
terraform output -json > "$script_dir/../deployment/outputs/skill.json"
"$script_dir/convert-json-to-env.sh" < "$script_dir/../deployment/outputs/skill.json" > "$script_dir/../skill.env"

# Copy the test documents to storage
az storage blob upload-batch --account-name $TF_VAR_storage_account_name --destination $TF_VAR_storage_container_name --source "$script_dir/../data/"

# save output to .env file in docs folder for testing API call
skill_api_key=$(jq -r .skill_api_key.value < "$script_dir/../deployment/outputs/skill.json")
skill_api_hostname=$(jq -r .skill_api_hostname.value < "$script_dir/../deployment/outputs/skill.json")

#
# Configure Azure Search Index etc
# Currently this is not supported in the Terraform azurerm provider
# See https://github.com/terraform-providers/terraform-provider-azurerm/issues/11699
#

set -o allexport
source "$script_dir/../skill.env"
set +o allexport

search_url="https://${TF_VAR_azure_search_name}.search.windows.net"

data_source_json=$(cat ../azuresearch/create_data_source.json | envsubst)
data_source_name=$(echo $data_source_json | jq -r .name )
echo "Creating data source $data_source_name ..."
curl -s -X PUT --header "Content-Type: application/json" --header "api-key: $TF_VAR_azure_search_admin_key" --data "$data_source_json" $search_url/datasources/$data_source_name?api-version=2024-07-01

index_json=$(cat ../azuresearch/create_index.json)
index_name=$(echo $index_json | jq -r .name )
echo "Creating index $index_name ..."
curl -s -X PUT --header "Content-Type: application/json" --header "api-key: $TF_VAR_azure_search_admin_key" --data "$index_json" $search_url/indexes/$index_name?api-version=2024-07-01

skillset_json=$(cat ../azuresearch/create_skillset.json | envsubst)
skillset_name=$(echo $skillset_json | jq -r .name )
echo "Creating skillset $skillset_name ... "
curl -s -X PUT --header "Content-Type: application/json" --header "api-key: $TF_VAR_azure_search_admin_key" --data "$skillset_json" $search_url/skillsets/$skillset_name?api-version=2024-07-01

indexer_json=$(cat ../azuresearch/create_indexer.json | envsubst)
indexer_name=$(echo $indexer_json | jq -r .name )
echo "Creating indexer $indexer_name ... "
curl -s -X PUT --header "Content-Type: application/json" --header "api-key: $TF_VAR_azure_search_admin_key" --data "$indexer_json" $search_url/indexers/$indexer_name?api-version=2024-07-01

# Run the indexer...
echo "Running indexer $indexer_name..."
curl -s -X POST --header "Content-Type: application/json" --header "api-key: $TF_VAR_azure_search_admin_key" --data "" $search_url/indexers/$indexer_name/run?api-version=2024-07-01

echo "API_KEY=$TF_VAR_skill_api_key" > "$script_dir/../tests/.env"
echo "API_URL=http://$TF_VAR_skill_api_hostname" >> "$script_dir/../tests/.env"