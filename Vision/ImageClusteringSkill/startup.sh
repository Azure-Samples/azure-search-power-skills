#!/usr/bin/env bash

mkdir -p /run/sshd
/usr/sbin/sshd
cd /usr/src/api
uvicorn app:app --host 0.0.0.0 --reload --port 5000
