#!/bin/bash
set -e

# Convert the terraform outputs (for string values) to env files
# prefixing the key names with TF_VAR_
# values are formatted as KEY=$'VALUE' with any single quotes escaped as \'
# e.g. a value of (MY'VALUE) would result in: KEY=$'MY\'VALUE'

jq -r "to_entries | .[] | select(.value.type == \"string\") | \"TF_VAR_\\(.key)=\$'\\(.value.value | gsub(\"(?<x>('))\"; \"\\\\'\"))'\"" 
