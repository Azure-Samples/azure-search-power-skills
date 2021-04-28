import json
import logging

from azureml.contrib.automl.dnn.vision.common.model_export_utils import load_model
from azureml.core import Workspace
from azureml.core.authentication import ServicePrincipalAuthentication
from azureml.core.experiment import Experiment


class Models:

    def __init__(self, azureml_model_dir, classication_model):
        self.azureml_model_dir = azureml_model_dir
        self.classication_model = classication_model
        self.task_type = 'image-multi-labeling'  # TODO Change to project type when required

    def load_classification_model(self, azureml_model_dir):
        """
        Load the model
        :param azureml_model_dir: Check if locally mounted or download latest model
        :return: Load AML model
        """
        logging.info(f"Loading model from {azureml_model_dir}")
        model_settings = {}
        self.classication_model = load_model(self.task_type, azureml_model_dir, **model_settings)
        self.azureml_model_dir = azureml_model_dir

    def get_workspace(self):
        """
        # Uses the config.json file to load the AML workspace
        :return: An AML workspace instance
        """

        # Our AML config file
        with open("/usr/src/api/config.json", "r") as json_file:
            config_data = json.load(json_file)

        # Let's connect to our workspace
        sp = ServicePrincipalAuthentication(tenant_id=config_data['tenant_id'],  # tenantID
                                            service_principal_id=config_data['service_principal_id'],  # clientId
                                            service_principal_password=config_data[
                                                'service_principal_password'])  # clientSecret

        ws = Workspace.get(name=config_data['workspace_name'],
                           auth=sp,
                           subscription_id=config_data['subscription_id'],
                           resource_group=config_data['resource_group'])

        return ws

    def get_latest_model(self, experiment_name):
        """
        This function finds the experiment associated with the Data Labelling
        Project and finds the best model and downloads the train artifacts. Note,
        at the time of writing no SDK support is available for data labelling projects
        :param experiment_name:
        :return:
        """
        success = False

        ws = self.get_workspace()

        logging.info(f"Connected to Workspace {ws.name}")
        experiment = Experiment(workspace=ws, name=experiment_name)
        list_runs = experiment.get_runs()
        for run in list_runs:
            logging.info(f"Getting last run {run.id}")
            tags = run.get_tags()
            if tags['model_explain_run'] == 'best_run':
                # Get the latest run
                logging.info(f"Getting last best child run {tags['automl_best_child_run_id']}")
                child_run = run.get(ws, tags['automl_best_child_run_id'])
                metrics = run.get_metrics()
                logging.info(f"Accuracy (class) {metrics['accuracy']}")
                file_names = child_run.get_file_names()
                if "train_artifacts/model.pt" in file_names:
                    logging.info('Found a trained model.pt')
                    child_run.download_files(prefix='train_artifacts',
                                             output_directory='/usr/src/api/models')
                    success = True
                    break

        return success
