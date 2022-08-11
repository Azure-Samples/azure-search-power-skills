import json
import os
import unittest
from copy import deepcopy

import azure.functions as func
import jsonschema
from azure.ai.textanalytics import (
    SingleCategoryClassifyResult,
    ClassificationCategory,
    DocumentError,
    TextAnalyticsError,
)

from customtextcla.main import main, result_to_json


class TestFunction(unittest.TestCase):
    def __init__(self, methodName="runTest"):
        super().__init__(methodName)
        self.req_body = {
            "values": [
                {
                    "recordId": "0",
                    "data": {
                        "lang": "en",
                        "text": "We consider a scalar balance law with a strict convex flux. In this paper, we study "
                        "L-2 stability to a shock for a scalar balance law up to a shift function, "
                        "which is based on the relative entropy. This result generalizes Leger's works [18] "
                        "and provides more a simple proof than Leger's proof. ",
                    },
                },
                {
                    "recordId": "1",
                    "data": {
                        "lang": "en",
                        "text": "A gradient-statistic-based diagnostic measure is developed in the context of the "
                        "generalized linear mixed models. Its performance is assessed by some real examples "
                        "and simulation studies, in terms of ability in detecting influential data structures "
                        "and of concordance with the most used influence measures. ",
                    },
                },
            ]
        }

        self.resp_body = {
            "values": [
                {
                    "recordId": "0",
                    "data": {
                        "class": [
                            {
                                "category": "Mechanical_engineering",
                                "confidence_score": 1.0,
                            }
                        ]
                    },
                    "warnings": [],
                },
                {
                    "recordId": "1",
                    "data": {
                        "class": [
                            {"category": "Computer_science", "confidence_score": 1.0}
                        ]
                    },
                    "warnings": [],
                },
            ]
        }

        self.response_schema = {
            "$schema": "http://json-schema.org/draft-04/schema#",
            "type": "object",
            "properties": {
                "values": {
                    "type": "array",
                    "items": {
                        "oneOf": [
                            {
                                "type": "object",
                                "properties": {
                                    "recordId": {"type": "string"},
                                    "data": {
                                        "type": "object",
                                        "properties": {
                                            "class": {
                                                "type": "array",
                                                "minLength": 1,
                                                "maxLength": 1,
                                                "items": {
                                                    "type": "object",
                                                    "properties": {
                                                        "category": {"type": "string"},
                                                        "confidence_score": {
                                                            "type": "number"
                                                        },
                                                    },
                                                    "required": [
                                                        "category",
                                                        "confidence_score",
                                                    ],
                                                },
                                            }
                                        },
                                        "required": ["class"],
                                    },
                                    "warnings": {
                                        "type": "array",
                                    },
                                },
                            },
                            {
                                "type": "object",
                                "properties": {
                                    "recordId": {"type": "string"},
                                    "data": {
                                        "type": "object",
                                        "additionalProperties": False,
                                        "properties": {},
                                    },
                                    "errors": {
                                        "type": "array",
                                        "minLength": 1,
                                        "items": {
                                            "type": "object",
                                            "properties": {
                                                "message": {"type": "string"}
                                            },
                                            "required": ["message"],
                                        },
                                    },
                                },
                                "required": ["errors"],
                            },
                        ],
                        "required": ["recordId", "data"],
                    },
                }
            },
            "required": ["values"],
        }

    def setUp(self):
        env_path = "single.env"
        with open(env_path, "r") as f:
            env = dict(
                tuple(line.replace("\n", "").split("="))
                for line in f.readlines()
                if not line.startswith("#")
            )
            for item in env.items():
                os.environ[item[0]] = item[1]

    def test_result_formatting_single(self):
        page = [
            SingleCategoryClassifyResult(
                id="1",
                classification=ClassificationCategory(
                    category="Drama", confidence_score=0.5
                ),
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
            json.loads(result_to_json("single", result)),
            {
                "values": [
                    {
                        "recordId": "1",
                        "data": {
                            "class": [
                                {"category": "Drama", "confidence_score": 0.5},
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

    def test_normal(self):
        req = func.HttpRequest(
            method="GET",
            body=json.dumps(self.req_body).encode("utf8"),
            url="/api/customtextcla",
        )

        resp = main(req)

        self.assertEqual(resp.status_code, 200)

        body: dict = json.loads(resp.get_body())

        try:
            jsonschema.validate(
                instance=body,
                schema=self.response_schema,
            )
        except jsonschema.exceptions.ValidationError:
            self.fail("Response doesn't match schema")

        self.assertEqual(len(body["values"]), 2)

        records = body["values"]

        for correct_record in self.resp_body["values"]:
            matching_records = list(
                filter(lambda r: r["recordId"] == correct_record["recordId"], records)
            )
            self.assertEqual(len(matching_records), 1)

            data = matching_records[0]["data"]

            self.assertTrue("class" in data.keys())
            classes = data["class"]

            for correct_class in correct_record["data"]["class"]:
                matching_classes = list(
                    filter(
                        lambda e: e["category"] == correct_class["category"], classes
                    )
                )
                self.assertEqual(len(matching_classes), 1)
                tested_class = matching_classes[0]
                self.assertAlmostEqual(
                    correct_class["confidence_score"],
                    tested_class["confidence_score"],
                    delta=0.2,
                )

    def test_missing_id(self):
        body = deepcopy(self.req_body)
        del body["values"][0]["recordId"]
        req = func.HttpRequest(
            method="GET",
            body=json.dumps(body).encode("utf8"),
            url="/api/custom_ner",
        )

        resp = main(req)

        self.assertNotEqual(resp.status_code, 200)

    def test_zero_inputs(self):
        body = deepcopy(self.req_body)
        body["values"] = []
        req = func.HttpRequest(
            method="GET",
            body=json.dumps(body).encode("utf8"),
            url="/api/custom_ner",
        )

        resp = main(req)

        self.assertNotEqual(resp.status_code, 200)

    def test_too_long(self):
        body = deepcopy(self.req_body)
        body["values"][0]["data"]["text"] = "very_long_input" * 125000
        req = func.HttpRequest(
            method="GET",
            body=json.dumps(body).encode("utf8"),
            url="/api/customtextcla",
        )

        resp = main(req)

        self.assertNotEqual(resp.status_code, 200)

    def test_lang_optional(self):
        body = deepcopy(self.req_body)
        del body["values"][0]["data"]["lang"]
        req = func.HttpRequest(
            method="GET",
            body=json.dumps(self.req_body).encode("utf8"),
            url="/api/customtextcla",
        )

        resp = main(req)

        self.assertEqual(resp.status_code, 200)

        body: dict = json.loads(resp.get_body())

        try:
            jsonschema.validate(
                instance=body,
                schema=self.response_schema,
            )
        except jsonschema.exceptions.ValidationError:
            self.fail("Response doesn't match schema")

        self.assertEqual(len(body["values"]), 2)

        records = body["values"]

        for correct_record in self.resp_body["values"]:
            matching_records = list(
                filter(lambda r: r["recordId"] == correct_record["recordId"], records)
            )
            self.assertEqual(len(matching_records), 1)

            data = matching_records[0]["data"]

            self.assertTrue("class" in data.keys())
            classes = data["class"]

            for correct_class in correct_record["data"]["class"]:
                matching_classes = list(
                    filter(
                        lambda e: e["category"] == correct_class["category"], classes
                    )
                )
                self.assertEqual(len(matching_classes), 1)
                tested_class = matching_classes[0]
                self.assertAlmostEqual(
                    correct_class["confidence_score"],
                    tested_class["confidence_score"],
                    delta=0.2,
                )

    def test_document_error(self):
        body = deepcopy(self.req_body)
        body["values"][0]["data"]["lang"] = "not_a_language"
        erroneous_recordid = body["values"][0]["recordId"]
        req = func.HttpRequest(
            method="GET",
            body=json.dumps(body).encode("utf8"),
            url="/api/customtextcla",
        )

        resp = main(req)

        self.assertEqual(resp.status_code, 200)

        body: dict = json.loads(resp.get_body())

        try:
            jsonschema.validate(
                instance=body,
                schema=self.response_schema,
            )
        except jsonschema.exceptions.ValidationError:
            self.fail("Response doesn't match schema")

        self.assertEqual(len(body["values"]), 2)

        records = body["values"]

        for correct_record in self.resp_body["values"]:
            matching_records = list(
                filter(lambda r: r["recordId"] == correct_record["recordId"], records)
            )
            self.assertEqual(len(matching_records), 1)

            record = matching_records[0]

            if correct_record["recordId"] == erroneous_recordid:
                # the error doesn't really matter
                # the fact that the "errors" field exists means that the record matches
                # the error branch of the "oneOf" field in the schema
                self.assertTrue("errors" in record.keys())
            else:
                data = record["data"]

                self.assertTrue("class" in data.keys())
                classes = data["class"]

                for correct_class in correct_record["data"]["class"]:
                    matching_classes = list(
                        filter(
                            lambda e: e["category"] == correct_class["category"],
                            classes,
                        )
                    )
                    self.assertEqual(len(matching_classes), 1)
                    tested_class = matching_classes[0]
                    self.assertAlmostEqual(
                        correct_class["confidence_score"],
                        tested_class["confidence_score"],
                        delta=0.2,
                    )
