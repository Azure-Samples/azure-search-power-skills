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

terraform init

terraform apply

# Save output for use in later stages
mkdir -p "$script_dir/../deployment/outputs"
terraform output -json > "$script_dir/../deployment/outputs/skill.json"
"$script_dir/convert-json-to-env.sh" < "$script_dir/../deployment/outputs/skill.json" > "$script_dir/../skill.env"

# save output to .env file in docs folder for testing API call
skill_api_key=$(jq -r .skill_api_key.value < "$script_dir/../deployment/outputs/skill.json")
skill_api_hostname=$(jq -r .skill_api_hostname.value < "$script_dir/../deployment/outputs/skill.json")

echo "API_KEY=$skill_api_key" > "$script_dir/../docs/.env"
echo "API_URL=http://$skill_api_hostname" >> "$script_dir/../docs/.env"
