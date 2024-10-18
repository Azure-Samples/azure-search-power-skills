import base64
import math
import os
from matplotlib.dviread import Page
from PIL import Image

class ImageUtils:

    def encode_image(image_path):  
        with open(image_path, "rb") as image_file:  
            return base64.b64encode(image_file.read()).decode("utf-8")  
        
    def calculate_dpi_from_page(page: Page, image_quality: str):
        rect = page.rect
        width, height = rect.width, rect.height
        max_side = max(width, height)

        max_resolution = 512

        if image_quality == "low":
            max_resolution = 512

        if image_quality == "high_720p":
            max_resolution = 720
        
        if image_quality == "high_1024p":
            max_resolution = 1024
        
        if image_quality == "high_1920p":
            max_resolution = 1920

        scale_factor = max_resolution/max_side
        dpi = math.ceil(scale_factor * 72)  # 72 DPI is the default resolution
        return dpi
    
    def resize_image(image_path: str, image_quality: str) -> str:
        max_resolution = 512

        if image_quality == "low":
            max_resolution = 512

        if image_quality == "high_720p":
            max_resolution = 720
        
        if image_quality == "high_1024p":
            max_resolution = 1024
        
        if image_quality == "high_1920p":
            max_resolution = 1920

        # Open the image
        with Image.open(image_path) as img:
            # Convert to PNG if the image is JPG
            if img.format == 'JPEG':
                img = img.convert('RGB')
                image_path = os.path.splitext(image_path)[0] + '.png'
            
            # Calculate new dimensions while maintaining aspect ratio
            width, height = img.size
            if width > height:
                new_width = max_resolution
                new_height = int((max_resolution / width) * height)
            else:
                new_height = max_resolution
                new_width = int((max_resolution / height) * width)
            
            # Resize the image
            img = img.resize((new_width, new_height))
            
            # Save the image in PNG format
            img.save(image_path, 'PNG')
            return image_path