from models.skill_output import ResponseData, ResponseProcess, ResponseValue

class ConversionUtils:
    
    @staticmethod
    def create_response_markdown(responseData: ResponseData):  
        return ResponseProcess(
            values=[
                ResponseValue(
                    data=responseData,
                    recordId="0",
                    errors=None,
                    warnings=None
                )
            ]
        )
    
    @staticmethod
    def create_response_exception(ex: Exception) -> ResponseProcess:
        error_response = ResponseProcess(
            values=[
                ResponseValue(
                    data=None,
                    recordId="0",
                    errors=[str(ex)],
                    warnings=None
                )
            ]
        )      
        return error_response