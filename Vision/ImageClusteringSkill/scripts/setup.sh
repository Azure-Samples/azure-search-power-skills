#!/bin/bash
set -e

script_dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

# sudo chown -R $(whoami) /home/vscode/.cache/

pip install -r requirements.txt

"$script_dir/unzip-data.sh"
