#!/bin/bash
set -e

script_dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

if [ ! -f "$script_dir/../powerskill/.env" ]; then
    echo "Rename '.env.example' to '.env' and add you values in."
    exit 1
fi

source "$script_dir/../powerskill/.env"

docker run -it \
    -p 5000:5000 \
    -e DEBUG=true \
    -e OPENAI_URL=${OPENAI_URL} \
    -e OPENAI_DEPLOYMENT=${OPENAI_DEPLOYMENT} \
    -e OPENAI_API_VERSION=${OPENAI_API_VERSION} \
    -e OPENAI_MAX_CONCURRENT_REQUESTS=${OPENAI_MAX_CONCURRENT_REQUESTS} \
    -e OPENAI_MAX_IMAGES_PER_REQUEST=${OPENAI_MAX_IMAGES_PER_REQUEST} \
    -e OPENAI_MAX_RETRIES=${OPENAI_MAX_RETRIES} \
    -e OPENAI_MAX_BACKOFF=${OPENAI_MAX_BACKOFF} \
    -e OPENAI_MAX_TOKENS_RESPONSE=${OPENAI_MAX_TOKENS_RESPONSE} \
    -e IMAGE_QUALITY=${IMAGE_QUALITY} \
    -e CHUNK_SIZE=${CHUNK_SIZE} \
    -e CHUNK_OVERLAP=${CHUNK_OVERLAP} \
    -e FLASK_DEBUG=${FLASK_DEBUG} \
    -e FLASK_ENV=${FLASK_ENV} \
    llm_chunker