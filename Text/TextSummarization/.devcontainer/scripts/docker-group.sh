#!/bin/bash 
set -e

if [ -z "${1}" ]; then
    echo "You must call this script with your USERNAME to add to the docker group."
    exit 1
fi

if [ -z "${2}" ]; then
    sudo groupadd docker
else
    sudo groupadd -g ${2} docker
fi

sudo usermod -aG docker ${1} && newgrp docker
getent group docker