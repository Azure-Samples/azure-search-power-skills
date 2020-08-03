from PIL import Image
from io import BytesIO
import cv2 
from azure.storage.blob import BlobServiceClient
import numpy as np

def write_with_bounding(img,boxes):
    color = (200,0,0)
    for b in boxes:  
        cv2.rectangle(img, (b.topleft_x, b.topleft_y), (b.bottomright_x,b.bottomright_y), color, 3)

    #if circles is not None:
    #    circles = np.uint16(np.around(circles))
    #    for i in circles[0, :]:
    #        center = (i[0], i[1])
    #        radius = i[2]
    #        cv2.circle(img,center,radius,(0,255,0),3)
        

    stream = utils.get_stream(img)

    blob_service_client = BlobServiceClient.from_connection_string('connectionstring')
    container_client = blob_service_client.get_container_client("functiontest1")
    blob_client = container_client.get_blob_client("test.jpeg")
    blob_client.upload_blob(stream.read(),overwrite=True)