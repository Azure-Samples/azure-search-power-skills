#!/bin/bash
set -e

#
# This script converts Terraform JSON output to .env files
#

jq -r 'to_entries | .[] | select(.value.type == "string") | "TF_VAR_\(.key)=\(.value.value)"' 
