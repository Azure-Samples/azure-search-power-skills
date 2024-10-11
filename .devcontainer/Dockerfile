FROM mcr.microsoft.com/devcontainers/universal:latest

# Install Azure Cli
RUN curl -sL https://aka.ms/InstallAzureCLIDeb | bash

# Install Jupyter Notebook
USER codespace
RUN pip install jupyter
