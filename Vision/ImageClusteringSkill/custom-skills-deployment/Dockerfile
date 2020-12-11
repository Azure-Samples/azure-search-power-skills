FROM tensorflow/tensorflow

# [Optional] If your pip requirements rarely change, uncomment this section to add them to the image.
COPY requirements.txt /tmp/pip-tmp/
RUN pip3 --disable-pip-version-check --no-cache-dir install -r /tmp/pip-tmp/requirements.txt \
    && rm -rf /tmp/pip-tmp

RUN mkdir -p /usr/src/mlops
RUN mkdir -p /usr/src/mlops/common
RUN mkdir -p /usr/src/ml
RUN mkdir -p /usr/src/ml/extractors
RUN mkdir -p /usr/src/ml/similarity
RUN mkdir -p /usr/src/ml/models
RUN mkdir -p /usr/src/ml/utils
RUN mkdir -p /usr/src/api
RUN mkdir -p /usr/src/api/common
RUN mkdir -p /usr/src/api/extractor
RUN mkdir -p /usr/src/api/models

WORKDIR /usr/src/api

COPY ../mlops/common /usr/src/mlops/common
COPY ../ml/extractors /usr/src/ml/extractors
COPY ../ml/models /usr/src/ml/models
COPY ../ml/similarity /usr/src/ml/similarity
COPY ../ml/utils /usr/src/ml/utils
COPY ./models /usr/src/api/models/
COPY ./extractor/ /usr/src/api/extractor/
COPY app.py /usr/src/api/

EXPOSE 5000

CMD ["uvicorn", "app:app", "--host", "0.0.0.0", "--port", "5000"]