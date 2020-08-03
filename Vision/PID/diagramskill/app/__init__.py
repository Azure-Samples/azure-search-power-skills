
############################################################################################
#Permission is hereby granted, free of charge, to any person obtaining a copy of
#this software and associated documentation files (the "Software"), to deal in
#the Software without restriction, including without limitation the rights to
#use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
#of the Software, and to permit persons to whom the Software is furnished to do
#so, subject to the following conditions:

#The above copyright notice and this permission notice shall be included in all
#copies or substantial portions of the Software.

#THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
#IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
#FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
#AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
#LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
#OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
#SOFTWARE.
###########################################################################################

import logging 
from __app__.textblock import TextBlock
import azure.functions as func
import cv2 
from PIL import Image
import numpy as np
import json
import requests
from io import BytesIO
from azure.storage.blob import BlobServiceClient
import re
import time
import pytesseract
import os
import base64
from datetime import datetime


blob_connection =  os.environ['BLOB_CONNECTION_STRING']
container = os.environ['CONTAINER']

## Function Entry Point
def main(req: func.HttpRequest) -> func.HttpResponse:
    version = "0.0.8"
    logging.info("Version: %s" % version)
    
    start_time_total = time.time()
    debug = False
    max_circle_ocr = 500

    if 'debug' in req.params:
        debug = (req.params.get('debug')=="True")

    if 'circles' in req.params:
        circles = int(req.params.get('debug'))

    logging.info("Debug: %s" % str(debug))
    logging.info("Circle Param: %s" % str(max_circle_ocr))

    jsn = req.get_json()

    # Build Function Response
    results = {}
    results["values"] = []

    values = jsn['values']

    logging.info("Values: " + str(len(values)))

    for value in values:
        process_normalized_image(value,results,debug,max_circle_ocr)


    jsn = json.dumps(results,ensure_ascii=False)    

    logging.info("All Processes --- %s seconds ---" % (time.time() - start_time_total))

    return func.HttpResponse(jsn, mimetype="application/json")



def process_normalized_image(value,results,debug,max_circle_ocr):
    """
    Process Values from request JSON

    Parameters::
      a(JSON): value -- Value record containing normalized image and bounding box
      b(JSON): results -- Return Values to append
      c(bool) debug -- Flag used to indicate debug (true,false)
      d(max_circels) -- Maximun number of Hough Circles to OCR (workaround for compact tabular documents)

    Returns:
        Prcess results (JSON): results for processes Value records
    """

    text_array = []
    block = ""
    tags = ""
    textblocks = []

    try:

        recordId = value['recordId']
        textlayout = value['data']['layoutText']

        # Capture  PDF Bytes
        start_time = time.time()
        image_string = value['data']['file_data']['data']        
        image_string_b = image_string.encode('utf-8')       
        image_bytes = base64.b64decode(image_string_b)
        logging.info("Image Bytes Len: " + str(len(image_bytes)))

        pil_image = Image.open(BytesIO(image_bytes))
        open_cv_image = np.array(pil_image)	        
        img = open_cv_image[:, :, ::-1].copy()   
        logging.info("Image Read--- %s seconds ---" % (time.time() - start_time))        


        if debug:
            if len(blob_connection) >0 and len(container) > 0:
                logging.info("Writing Debug Image...")

                now  = datetime.now()
                lst = list(map(str,[now.day,now.hour,now.minute,now.second]))

                file_name =  'debug_' + '_'.join(lst)
                logging.info("Calling get_stream(img)")    
                stream = get_stream(img)

                try:
                    blob_service_client = BlobServiceClient.from_connection_string(blob_connection)
                    container_client = blob_service_client.get_container_client(container)
                    blob_client = container_client.get_blob_client(file_name+"/img.jpeg")
                    blob_client.upload_blob(stream.read(),overwrite=True)

                    blob_service_client = BlobServiceClient.from_connection_string(blob_connection)
                    container_client = blob_service_client.get_container_client(container)
                    blob_client = container_client.get_blob_client(file_name+"/text.json")
                    blob_client.upload_blob(json.dumps(textlayout),overwrite=True)

                except Exception as ex:
                    template = "Debug Exception - An exception of type {0} occurred. Arguments:\n{1!r}"
                    message = template.format(type(ex).__name__, ex.args)
                    logging.error(message)
        

        start_time = time.time()
        circles = contour_match(img) # Retrieve Hough Circles    
        logging.info("Contour Match--- %s seconds ---" % (time.time() - start_time))
        
        start_time = time.time()
        if circles is not None:
            circle_size = np.size(circles)
            if circle_size <= max_circle_ocr:
                
                logging.info("Found %s Circles" % str(circle_size))
                text_array.append(ocr_circles(img,circles))               
        else:
            logging.info("Did not find any circles!!")

        logging.info("Contour OCR--- %s seconds ---" % (time.time() - start_time))
               

        start_time = time.time()
       
        for line in textlayout["lines"]:
            if len(line) >0:
                textblocks.append(TextBlock(line['text'],line['boundingBox']))

        logging.info("Load Bounding Boxes--- %s seconds ---" % (time.time() - start_time))

        logging.info("%s textblocks" % str(len(textblocks)))


        if len(textblocks) > 0:
            start_time = time.time()
            boxes,matches,singles = match_boxes(textblocks)
            boxes = match_singles(boxes,matches,singles)
            logging.info("Group Text--- %s seconds ---" % (time.time() - start_time))

            text_array.append(get_text(boxes))

        start_time = time.time()
        text = ' '.join(text_array)    

        text = text.replace("- ","-") 
        text = text.replace(" -","-") 
        strings = text.split(" ")
        if len(strings) >0:
            block_array = [ x for x in strings if "-" not in x]
            if len(block_array) > 0: block = ' '.join(block_array)
            tags_array = [ x for x in strings if "-"  in x and len(x)>1]
            if len(tags_array) > 0: tags = ' '.join(tags_array)
        else:
            block = ""
            tags = ""  

        logging.info("Finalize Text--- %s seconds ---" % (time.time() - start_time))


    except Exception as ex:
        template = "An exception of type {0} occurred. Arguments:\n{1!r}"
        message = template.format(type(ex).__name__, ex.args)
        logging.error(message)


    results['values'].append({"recordId": recordId,"data": { "textblocks": block,"tags":tags} })
    
    

 
def get_stream(img,format='JPEG'):

    """
    Convert CV2 Image to ByteIO Stream

    Parameters::
      (CV2 Image): img -- OpenCV Image
      (String) : format -- image format (Default JPEG)

    Returns:
       ByteIO Steam
    """
    logging.info("calling get_stream")
    pil_image2 = Image.fromarray(cv2.cvtColor(img,cv2.COLOR_BGR2RGB))
    buf = BytesIO()
    pil_image2.save(buf, format=format)
    buf.seek(0)
    return buf


def get_text(boxes):
    """
    Merge text from boxes (Textblock)

    Parameters::
      (TextBlock[]): boxes -- OCR boundingbox grouping to retreive test from 

    Returns:
        TextS (String): concat textblock text
    """
    text = ""
    for b in boxes:
        text = text + " " +  b.text 
    return text



def match_singles(boxes,matches,singles):
    """
    Convert CV2 Image to ByteIO Stream

    Parameters::
      (CV2 Image): img -- OpenCV Image
      (String) : format -- image format (Default JPEG)

    Returns:
       ByteIO Steam
    """
    for s in singles:
        cnt = 0
        for m in matches:
            if(matches[m].intersect(singles[s]) == True):
                cnt = 0
                break
            else:
                cnt = 1
        
        if cnt >0:
         boxes.append(singles[s])

    return boxes



def match_boxes(textblocks):
    boxes = []
    matches = {}
    ids=[]
    singles = {}
    maxSegment = 95
    leftAlignSensitivity = 5
    rightAlignSensitivity = 5
    centerAlignSensitivty = 15
 
    for i in range(len(textblocks)):
        tb = textblocks[i].copy()
        count=0
        for j in range(len(textblocks)):
            if (i != j) and (i not in ids) and (len(textblocks[j].text) < maxSegment and len(tb.text) < maxSegment) and (tb.dist_y(textblocks[j]) <= 18):
                if tb.dist_left_x(textblocks[j]) <= leftAlignSensitivity:  
                    ids.append(j)
                    tb = tb.merge(textblocks[j])
                    count = count + 1  
                else:
                    if tb.dist_right_x(textblocks[j]) <= rightAlignSensitivity:
                        ids.append(j)
                        tb = tb.merge(textblocks[j])
                        count = count + 1
                    else:
                        if tb.dist_mean_x(textblocks[j]) <= centerAlignSensitivty:
                            ids.append(j)
                            tb = tb.merge(textblocks[j])
                            count = count + 1
        if(count > 0):
            matches.update({i:tb})
        else:
            singles.update({i:tb})

    boxes = list(matches.values())
        
    return boxes,matches,singles



def get_text_from_img(img):

    #--oem 1 --psm 3 -l eng --oem 1 ''
    txt = pytesseract.image_to_string(img,lang='eng',config='--psm 6')

    ### old if re.search("^[a-zA-Z]+\\n[0-9_]+$",txt) != None:
    txt = txt.replace("\n","-") ## set-up tags from instrument contours
    removeSpecialChars = txt.translate({ord(c): " " for c in "!@#$%^&*()[]{};:,./<>?\|`~=_+\"\\"})

    return removeSpecialChars



def contour_match(img):
    
    img2 = img.copy()
   
    kernel = np.ones((2,2), np.uint8) 

    img2 = cv2.GaussianBlur(img2, (5, 5), 5)

    img2 = cv2.dilate(img2, kernel, iterations=2)
    ret, img2 = cv2.threshold(img2, 220, 255, cv2.THRESH_TOZERO)
    gray = cv2.cvtColor(img2, cv2.COLOR_BGR2GRAY)

    circles = cv2.HoughCircles(gray, cv2.HOUGH_GRADIENT, dp=3, minDist=10,
                            param1=50, param2=90,
                            minRadius=10, maxRadius=20)


    if circles is not None:
        circles = np.uint16(np.around(circles))

    return circles



def ocr_circles(img, circles):

    kernel = np.ones((3,3), np.uint8) 
    kernel_sharpening=np.array([[-1,-1,-1], [-1, 15,-1],[-1,-1,-1]])

    text = ""
    buffer=0
    i = 0

    for c1 in circles[0, :]:     
        r = c1[2]+buffer
        x = c1[0]-r
        y= c1[1]-r
        x2= c1[0]+r
        y2= c1[1]+r
        src_cp = img[y:y2, x:x2]    

   
        h_ratio = 110/src_cp.shape[0]
        w_ratio=110/src_cp.shape[1]

        src_cp = cv2.resize(src_cp,(int(src_cp.shape[1] * w_ratio),int(src_cp.shape[0] * h_ratio)))

        src_cp = cleancircle(src_cp)
        src_cp = cv2.dilate(src_cp, kernel, iterations=1)

        ret, src_cp = cv2.threshold(src_cp, 210, 255, cv2.THRESH_TOZERO)
        src_cp = cv2.fastNlMeansDenoisingColored(src_cp,None,6,6,7,21)
        sharpened=cv2.filter2D(src_cp,-1,kernel_sharpening)
        sharpened = remove_horizontal_lines(sharpened)
        
        text = text + " " + get_text_from_img(sharpened) 
        
        i = i + 1
       
    return text




def cleancircle(img):
  kernel = np.ones((2,2), np.uint8) 

  src_cp2 = img.copy()

  src_cp2 = cv2.GaussianBlur(src_cp2, (5, 5), 5)

  src_cp2 = cv2.dilate(src_cp2, kernel, iterations=2)
  ret, src_cp2 = cv2.threshold(src_cp2, 220, 255, cv2.THRESH_TOZERO)
  gray = cv2.cvtColor(src_cp2, cv2.COLOR_BGR2GRAY)


  c = cv2.HoughCircles(gray, cv2.HOUGH_GRADIENT, dp=3, minDist=50,
               param1=80, param2=90,
               minRadius=40, maxRadius=56) 
  
  ret,src_cp2 = cv2.threshold(gray, 220, 255, cv2.THRESH_BINARY_INV+ cv2.THRESH_OTSU)

  mask= np.zeros(src_cp2.shape[:2], dtype="uint8") 

  if c is not None:
    c = np.uint16(np.around(c))
    for i in c[0, :]:
      center = (i[0], i[1])
      # circle outline
      radius = i[2]
      cv2.circle(mask, center, radius-2, (255,255,255), -1)

  img = cv2.bitwise_and(img,img,mask=mask)

  img = img * 255
  img = cv2.bitwise_not(img)
  img = cv2.erode(img, kernel, iterations=1) 
  img = cv2.dilate(img, kernel, iterations=2) 
  
  return img


def remove_horizontal_lines(img):

    #img = img.cvtColor(img,cv2.COLOR_GRAY2BGR)
    gray = cv2.cvtColor(img,cv2.COLOR_BGR2GRAY)
    thresh = cv2.threshold(gray, 0, 255, cv2.THRESH_BINARY_INV + cv2.THRESH_OTSU)[1]
    horizontal_kernel = cv2.getStructuringElement(cv2.MORPH_RECT, (40,1))
    remove_horizontal = cv2.morphologyEx(thresh, cv2.MORPH_OPEN, horizontal_kernel, iterations=2)
    cnts = cv2.findContours(remove_horizontal, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
    cnts = cnts[0] if len(cnts) == 2 else cnts[1]
    for c in cnts:
        cv2.drawContours(img, [c], -1, (255,255,255), 5)
    return img



def cleanuplines(image, h=False):
  
    gray = cv2.cvtColor(image,cv2.COLOR_BGR2GRAY)

    if h==True:
        kernel = np.ones((2,2), np.uint8) 
        #gray = cv2.dilate(gray, kernel, iterations=2)    
        thresh= cv2.GaussianBlur(gray,(0,1),cv2.BORDER_ISOLATED)
        thresh = cv2.threshold(thresh, 150, 255, cv2.THRESH_BINARY )[1]  

    else:
        #gray = cv2.GaussianBlur(gray,(0,1),cv2.BORDER_DEFAULT)
        thresh = cv2.threshold(gray, 0, 255, cv2.THRESH_BINARY_INV + cv2.THRESH_OTSU)[1]


    result = image.copy()
    # Remove horizontal lines
    horizontal_kernel = cv2.getStructuringElement(cv2.MORPH_RECT, (40,1))
    remove_horizontal = cv2.morphologyEx(thresh, cv2.MORPH_OPEN, horizontal_kernel, iterations=2)
    cnts = cv2.findContours(remove_horizontal, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
    cnts = cnts[0] if len(cnts) == 2 else cnts[1]
    
    for c in cnts:
        cv2.drawContours(result, [c], -1, (255,255,255), 5)

    if h:
        gray = cv2.cvtColor(image,cv2.COLOR_BGR2GRAY)
        thresh = cv2.threshold(gray, 0, 255, cv2.THRESH_BINARY_INV + cv2.THRESH_OTSU)[1]

    # Remove vertical lines
    vertical_kernel = cv2.getStructuringElement(cv2.MORPH_RECT, (1,40))
    remove_vertical = cv2.morphologyEx(thresh, cv2.MORPH_OPEN, vertical_kernel, iterations=2)
    cnts = cv2.findContours(remove_vertical, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
    cnts = cnts[0] if len(cnts) == 2 else cnts[1]
    for c in cnts:
        cv2.drawContours(result, [c], -1, (255,255,255), 5)

        
    return result


    
