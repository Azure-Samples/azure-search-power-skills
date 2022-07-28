import json
import unittest
from urllib.request import urlopen

import jsonschema
from azure.ai.textanalytics import (
    TextDocumentInput,
    RecognizeCustomEntitiesResult,
    CategorizedEntity,
    DocumentError,
    TextAnalyticsError,
)

from custom_ner.main import (
    get_request_schema,
    map_dict_to_text_input,
    result_to_json,
)


class TestFunction(unittest.TestCase):
    def test_valid_schema(self):
        try:
            url = "http://json-schema.org/draft-04/schema#"
            jsonschema.validate(
                instance=get_request_schema(), schema=json.load(urlopen(url))
            )
        except jsonschema.ValidationError:
            self.fail("Request schema validation failed")

    def test_no_text_field(self):
        with self.assertRaises(jsonschema.ValidationError):
            jsonschema.validate(
                instance={
                    "values": [
                        {"recordId": "0", "data": {"lang": "en", "txt": "some text"}}
                    ]
                },
                schema=get_request_schema(),
            )

    def test_lang_optional(self):
        jsonschema.validate(
            instance={"values": [{"recordId": "0", "data": {"text": "some text"}}]},
            schema=get_request_schema(),
        )

    def test_no_records(self):
        with self.assertRaises(jsonschema.ValidationError):
            jsonschema.validate(instance={"values": []}, schema=get_request_schema())

    def test_correct_dict_to_text_input(self):
        t = {
            "values": [
                {"recordId": "0", "data": {"lang": "en", "text": "some text"}},
                {"recordId": "1", "data": {"lang": "en", "text": "some other text"}},
            ]
        }

        self.assertEqual(
            map_dict_to_text_input(t),
            [
                TextDocumentInput(id="0", text="some text", language="en"),
                TextDocumentInput(id="1", text="some other text", language="en"),
            ],
        )

    def test_result_formatting_single(self):
        page = [
            RecognizeCustomEntitiesResult(
                id="1",
                entities=[
                    CategorizedEntity(
                        text="10/18/2019",
                        category="Date",
                        length=10,
                        offset=5,
                        confidence_score=1.0,
                    )
                ],
            )
        ]
        page2 = [
            DocumentError(
                id="2",
                error=TextAnalyticsError(
                    code="invalidRequest", message="some error message"
                ),
            )
        ]
        result = [page, page2]
        self.assertDictEqual(
            json.loads(result_to_json(result)),
            {
                "values": [
                    {
                        "recordId": "1",
                        "data": {
                            "entities": [
                                {
                                    "text": "10/18/2019",
                                    "category": "Date",
                                    "subcategory": None,
                                    "length": 10,
                                    "offset": 5,
                                    "confidence_score": 1.0,
                                }
                            ]
                        },
                        "warnings": [],
                    },
                    {
                        "recordId": "2",
                        "data": {},
                        "errors": [{"message": "some error message"}],
                    },
                ]
            },
        )
