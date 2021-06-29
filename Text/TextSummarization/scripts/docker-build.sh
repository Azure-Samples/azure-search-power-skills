#!/bin/bash
set -e

script_dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

cd "$script_dir/../powerskill"

if [ ! -f "$script_dir/../powerskill/.env" ]; then
    echo "Rename 'sample_env' to '.env' and add you values in."
    exit 1
fi

source "$script_dir/../powerskill/.env"

: "${ACR_NAME?}"
: "${KEY?}"

docker build -t "${ACR_NAME}".azurecr.io/text_summarization_extractor:"${KEY}" . 