#!/bin/bash
set -e

#
# This script ensures that the test.zip and train.zip are unzipped ready for 
# use from the notebooks
#

script_dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

cd "$script_dir/../data"

unzip -n test.zip
unzip -n train.zip
