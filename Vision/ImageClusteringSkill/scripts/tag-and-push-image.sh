#!/bin/bash
set -e

#
# This script tags the clusterextractor container image
# and pushes it to the container registry
#

script_dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

# Load base env vars
if [[ ! -f  "$script_dir/../base.env" ]]; then
    echo "deploy.env not found - see base.env-example"
    exit 1
fi
source "$script_dir/../base.env"

if [[ -z $TF_VAR_container_registry ]]; then
    echo "container_registry not set - check base.env"
    exit 1
fi

# Sign in to container registry 
az acr login --name $TF_VAR_container_registry

# Generate tag based on date
tag=$(date -u +"%Y%m%d-%H%M%S")

docker tag clusterextractor $TF_VAR_container_registry/clusterextractor:$tag
docker push $TF_VAR_container_registry/clusterextractor:$tag

echo -n $tag > "$script_dir/../deployment/outputs/image_tag.txt"
