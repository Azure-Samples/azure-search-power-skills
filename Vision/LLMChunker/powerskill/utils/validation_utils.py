from models.skill_input import RequestProcess

class ValidationUtils:

    VALID_EXTENSIONS = ['.pptx', '.ppt', '.docx', '.doc', '.pdf']

    @staticmethod
    def validate_process_request(input: RequestProcess):  
        valuesLength = len(input.values)

        if valuesLength > 1:  
            raise Exception("Only one file is supported with this skill.")
        
        if  valuesLength == 0:
            raise Exception("You must specify one file.")
        
        url = input.values[0].data.blobUrl

        if not any(url.endswith(ext) for ext in ValidationUtils.VALID_EXTENSIONS):
            raise Exception(f"Only the following file types are supported: {', '.join(ValidationUtils.VALID_EXTENSIONS)}")