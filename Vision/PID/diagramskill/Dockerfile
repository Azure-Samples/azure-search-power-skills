# To enable ssh & remote debugging on app service change the base image to the one below
# FROM mcr.microsoft.com/azure-functions/python:2.0-python3.7-appservice
FROM mcr.microsoft.com/azure-functions/python:3.0-python3.7-appservice

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true



RUN apt-get update && apt-get install -y \
    software-properties-common 

#poppler-utils

RUN apt-get install  curl wget -y
RUN apt-get install python3-pip python3-venv libjpeg-dev libfreetype6 libfreetype6-dev zlib1g-dev -y
RUN pip3 install -U pip


RUN apt-get install  libtesseract4 tesseract-ocr-osd tesseract-ocr -y
RUN apt-get install tesseract-ocr-eng -y
#RUN apt-get install tesseract-ocr-dev -y


COPY requirements.txt /
RUN pip3 install -r /requirements.txt



COPY . /home/site/wwwroot

ARG BLOB_CONNECTION_STRING=''
ARG CONTAINER=''

ENV BLOB_CONNECTION_STRING=$BLOB_CONNECTION_STRING
ENV CONTAINER=$CONTAINER
