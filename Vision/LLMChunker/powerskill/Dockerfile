FROM python:3.11

RUN apt-get update
RUN apt-get -y install libreoffice-nogui
RUN apt-get -y install wkhtmltopdf

RUN wget https://github.com/wkhtmltopdf/packaging/releases/download/0.12.6.1-2/wkhtmltox_0.12.6.1-2.jammy_amd64.deb
RUN dpkg -x wkhtmltox_0.12.6.1-2.jammy_amd64.deb .

COPY requirements.txt /tmp/pip-tmp/
RUN pip3 --disable-pip-version-check --no-cache-dir install -r /tmp/pip-tmp/requirements.txt \
    && rm -rf /tmp/pip-tmp

RUN mkdir -p /usr/src/api
RUN mkdir -p /usr/src/api/models
RUN mkdir -p /usr/src/api/skill
RUN mkdir -p /usr/src/api/utils

WORKDIR /usr/src/api

COPY models /usr/src/api/models/
COPY skill /usr/src/api/skill/
COPY utils /usr/src/api/utils/
COPY app.py /usr/src/api/
#COPY .env .env

EXPOSE 5000

CMD ["uvicorn", "app:app", "--host", "0.0.0.0", "--port", "5000"]
