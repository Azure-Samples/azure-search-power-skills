FROM python:3.8

# [Optional] If your pip requirements rarely change, uncomment this section to add them to the image.
COPY requirements.txt /tmp/pip-tmp/
RUN pip3 --disable-pip-version-check --no-cache-dir install -r /tmp/pip-tmp/requirements.txt \
    && rm -rf /tmp/pip-tmp

RUN apt-get update
RUN apt-get install 'libsm6'\
    'libgl1-mesa-glx'\
    'libxext6'  -y

RUN mkdir -p /usr/src/api
RUN mkdir -p /usr/src/api/powerskill
RUN mkdir -p /usr/src/api/models

WORKDIR /usr/src/api

COPY models /usr/src/api/models/
COPY powerskill /usr/src/api/powerskill/
COPY app.py /usr/src/api/

# Set the cache up so that we don't download the 1.6Gb Model each time
ENV TRANSFORMERS_CACHE=/usr/src/api/models/

EXPOSE 5000

CMD ["uvicorn", "app:app", "--host", "0.0.0.0", "--port", "5000"]
