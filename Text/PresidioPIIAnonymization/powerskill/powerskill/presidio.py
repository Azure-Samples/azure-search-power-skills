from presidio_analyzer import AnalyzerEngine
from presidio_anonymizer import AnonymizerEngine
from presidio_anonymizer.entities.engine import OperatorConfig


class Presidio:
    def __init__(self):
        self.analyzer = AnalyzerEngine()
        self.anonymizer = AnonymizerEngine()

    def analyze_and_anonymize(self, text) -> str:
        analyzer_results = self.analyzer.analyze(text=text, language='en')
        operators = {"DEFAULT": OperatorConfig("redact")}
        anonymizer_results = self.anonymizer.anonymize(text=text,
                                                       analyzer_results=analyzer_results,
                                                       operators=operators)

        return anonymizer_results.text
