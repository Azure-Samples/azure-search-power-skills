#!/bin/bash
set -e

script_dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

if [ ! -f "$script_dir/../powerskill/.env" ]; then
    echo "Rename 'sample_env' to '.env' and add you values in."
    exit 1
fi

source "$script_dir/../powerskill/.env"

: "${KEY?}"

docker run -it \
    -p 5000:5000 \
    -e DEBUG=true \
    -e SUMMARIZER_MODEL=${SUMMARIZER_MODEL} \
    -e MAX_LENGTH=${MAX_LENGTH} \
    -e NUM_BEAMS=${NUM_BEAMS} \
    -e KEY=${KEY} \
    -e FLASK_DEBUG=${FLASK_DEBUG} \
    -e FLASK_ENV=${FLASK_ENV} \
    text_summarization_extractor