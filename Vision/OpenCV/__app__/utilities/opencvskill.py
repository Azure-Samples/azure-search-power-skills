import cv2
from . import webapiskill
import os

class opencvskill:
    def __init__(self, debug_override=False):
        if debug_override or ("debugMode" in os.environ and os.environ["debugMode"] == "true"):
            self.debug_state = []
            self.debug_mode = True
    
    def __add_debug_step(self, debug_step):
        if (self.debug_mode):
            self.debug_state.append(debug_step)

    def get_output_debug_steps(self):
        result = []
        for debug_step in self.debug_state:
            is_success, result_image = cv2.imencode(".png", debug_step["image"])
            result_debug_step = { "image": { "$type": "file", "name": "output.png", "data": webapiskill.encode_image_bytes(result_image) } }
            for key, value in debug_step.items():
                if (key == "image"):
                    continue
                result_debug_step[key] = value
            result.append(result_debug_step)
        return result
    
    def imdecode(self, buf, flags):
        dst = cv2.imdecode(buf, flags)
        self.__add_debug_step({ "image": dst, "function": "imdecode", "flags": flags })
        return dst
    
    def cvtColor(self, src, code):
        dst = cv2.cvtColor(src, code)
        self.__add_debug_step({ "image": dst, "function": "cvtColor", "code": code })
        return dst

    def bilateralFilter(self, src, d, sigmaColor, sigmaSpace):
        dst = cv2.bilateralFilter(src, d, sigmaColor, sigmaSpace)
        self.__add_debug_step({ "image": dst, "function": "bilateralFilter", "d": d, "sigmaColor": sigmaColor, "sigmaSpace": sigmaSpace })
        return dst

    def Canny(self, image, threshold1, threshold2):
        image = cv2.Canny(image, threshold1, threshold2)
        self.__add_debug_step({ "image": image, "function": "Canny", "threshold1": threshold1, "threshold2": threshold2 })


