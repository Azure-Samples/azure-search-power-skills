import os
from dotenv import load_dotenv
load_dotenv()
AZURE_CREDENTIAL = os.getenv('AZURE_CREDENTIAL')
a = 5
print(f'the azure credential is: {AZURE_CREDENTIAL}')