import json
import unittest
from urllib.request import urlopen

import jsonschema
from azure.ai.textanalytics import TextDocumentInput

from customtextcla.main import (
    get_request_schema,
    map_dict_to_text_input,
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
