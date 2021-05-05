FROM python:3.8

# [Optional] If your pip requirements rarely change, comment this section to add them to the image.
COPY requirements.txt /tmp/pip-tmp/
RUN pip3 --disable-pip-version-check --no-cache-dir install -r /tmp/pip-tmp/requirements.txt \
    && rm -rf /tmp/pip-tmp

RUN python -m spacy download en_core_web_lg

RUN mkdir -p /usr/src/api
RUN mkdir -p /usr/src/api/powerskill

WORKDIR /usr/src/api

COPY . /usr/src/api

EXPOSE 5000

CMD ["uvicorn", "app:app", "--host", "0.0.0.0", "--port", "5000"]
