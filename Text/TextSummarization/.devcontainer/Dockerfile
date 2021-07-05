FROM mcr.microsoft.com/vscode/devcontainers/miniconda:0-3

# [Option] Install Node.js
ARG INSTALL_NODE="true"
ARG NODE_VERSION="lts/*"
RUN if [ "${INSTALL_NODE}" = "true" ]; then su vscode -c "umask 0002 && . /usr/local/share/nvm/nvm.sh && nvm install ${NODE_VERSION} 2>&1"; fi

ARG USERNAME=vscode
USER $USERNAME

RUN sudo chown -R $(whoami) /opt/conda 

# Configure apt, install packages and general tools
RUN sudo apt-get update \
    && sudo apt-get -y install --no-install-recommends apt-utils dialog nano bash-completion sudo bsdmainutils \
    #
    # Verify git, process tools, lsb-release (common in install instructions for CLIs) installed
    && sudo apt-get -y install git iproute2 procps lsb-release figlet build-essential


# Save command line history
RUN echo "export HISTFILE=/home/$USERNAME/commandhistory/.bash_history" >> "/home/$USERNAME/.bashrc" \
    && echo "export PROMPT_COMMAND='history -a'" >> "/home/$USERNAME/.bashrc" \
    && mkdir -p /home/$USERNAME/commandhistory \
    && touch /home/$USERNAME/commandhistory/.bash_history \
    && chown -R $USERNAME /home/$USERNAME/commandhistory

# Set env for tracking that we're running in a devcontainer
ENV DEVCONTAINER=true

# Copy environment.yml (if found) to a temp locaition so we update the environment. Also
# copy "noop.txt" so the COPY instruction does not fail if no environment.yml exists.
COPY environment.yml* .devcontainer/noop.txt /tmp/conda-tmp/
RUN if [ -f "/tmp/conda-tmp/environment.yml" ]; then /opt/conda/bin/conda env update -n base -f /tmp/conda-tmp/environment.yml; fi \
    && sudo rm -rf /tmp/conda-tmp

# TODO - collapse this/create script once done experimenting
RUN bash --login -c "conda create -n ipykernel_py3 python=3 ipykernel -y \
    && conda init bash"
RUN bash --login -c "conda activate ipykernel_py3 \
    && python -m ipykernel install --user \
    && conda install opencv tensorflow -y"
RUN echo "conda activate ipykernel_py3" >> /home/vscode/.bashrc

RUN sudo apt-get update \
    # libgl dependencies
    && sudo apt-get install ffmpeg libsm6 libxext6  -y 

# [Optional] Uncomment to install a different version of Python than the default
# RUN conda install -y python=3.6 \
#     && pip install --no-cache-dir pipx \
#     && pipx reinstall-all

# [Optional] Uncomment this section to install additional OS packages.
# RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
#     && apt-get -y install --no-install-recommends <your-package-list-here>

# docker-client
ARG DOCKER_GROUP_ID
COPY .devcontainer/scripts/docker-client.sh /tmp/
RUN /tmp/docker-client.sh

# terraform
COPY .devcontainer/scripts/terraform.sh /tmp/
RUN /tmp/terraform.sh

# azure-cli
COPY .devcontainer/scripts/azure-cli.sh /tmp/
RUN /tmp/azure-cli.sh


# Sync timezone (if TZ value not already present on host it defaults to Europe/London)
# Note: if running on WSL (Windows) you can add the below to your $profile so your tz is automatically synced
# $tz =  [Windows.Globalization.Calendar,Windows.Globalization,ContentType=WindowsRuntime]::New().GetTimeZone()
# [Environment]::SetEnvironmentVariable("TZ",$tz, "User")
RUN if [ -z "$TZ" ]; then TZ="Europe/London"; fi && sudo ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ | sudo tee /etc/timezone > /dev/null

# install envsubst
RUN sudo apt install gettext-base

# __DEVCONTAINER_SNIPPET_INSERT__ (control where snippets get inserted using the devcontainer CLI)