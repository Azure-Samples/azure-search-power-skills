#!/bin/bash
set -e

script_dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

cd "$script_dir/../powerskill"

if [ ! -f ".env" ]; then
    echo "ERROR: Rename '/powerskill/.env.example' to '/powerskill/.env' and add you values in."
    exit 1
fi

cp ".env" "../docs/.env"

if [ $DEVCONTAINER != "true" ]; then
    python -m pip install -r requirements.txt
fi
python app.py