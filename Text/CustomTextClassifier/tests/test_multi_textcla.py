import json
import os
import unittest

import azure.functions as func
from customtextcla.main import main, result_to_json
from azure.ai.textanalytics import MultiCategoryClassifyResult, ClassificationCategory


class TestFunction(unittest.TestCase):
    def setUp(self) -> None:
        env_path = "multi.env"
        with open(env_path, "r") as f:
            env = dict(
                tuple(line.replace("\n", "").split("="))
                for line in f.readlines()
                if not line.startswith("#")
            )
            for item in env.items():
                os.environ[item[0]] = item[1]

    def test_result_formatting_multi(self):
        page = [
            MultiCategoryClassifyResult(
                id="1",
                classifications=[
                    ClassificationCategory(category="Drama", confidence_score=0.5),
                    ClassificationCategory(category="Action", confidence_score=0.7),
                ],
            )
        ]
        result = [page]
        self.maxDiff = None
        self.assertDictEqual(
            json.loads(result_to_json("multi", result)),
            {
                "values": [
                    {
                        "recordId": "1",
                        "data": {
                            "class": [
                                {"category": "Drama", "confidence_score": 0.5},
                                {"category": "Action", "confidence_score": 0.7},
                            ]
                        },
                        "warnings": [],
                    }
                ]
            },
        )

    def test_normal(self):
        req = func.HttpRequest(
            method="GET",
            body=json.dumps(
                {
                    "values": [
                        {
                            "recordId": "0",
                            "data": {
                                "lang": "en",
                                "text": "A young Lakota Sioux is adopted by a wealthy Jewish couple. When he ages he gets in touch with his cultural roots.",
                            },
                        }
                    ]
                }
            ).encode("utf8"),
            url="/api/customtextcla",
        )

        resp = main(req)

        self.assertEqual(resp.status_code, 200)

        self.assertEqual(
            json.loads(resp.get_body()),
            {
                "values": [
                    {
                        "recordId": "0",
                        "data": {
                            "class": [{"category": "Drama", "confidence_score": 1.0}]
                        },
                        "warnings": [],
                    }
                ]
            },
        )

    def test_missing_id(self):
        req = func.HttpRequest(
            method="GET",
            body=json.dumps(
                {
                    "values": [
                        {
                            "data": {
                                "lang": "en",
                                "text": "A young Lakota Sioux is adopted by a wealthy Jewish couple. When he ages he gets in touch with his cultural roots.",
                            }
                        }
                    ]
                }
            ).encode("utf8"),
            url="/api/customtextcla",
        )

        resp = main(req)

        self.assertNotEqual(resp.status_code, 200)

    def test_zero_inputs(self):
        req = func.HttpRequest(
            method="GET",
            body=json.dumps({"values": []}).encode("utf8"),
            url="/api/customtextcla",
        )

        resp = main(req)

        self.assertNotEqual(resp.status_code, 200)

    def test_lang_optional(self):
        req = func.HttpRequest(
            method="GET",
            body=json.dumps(
                {
                    "values": [
                        {
                            "recordId": "0",
                            "data": {
                                "text": "A young Lakota Sioux is adopted by a wealthy Jewish couple. When he ages he gets in touch with his cultural roots."
                            },
                        }
                    ]
                }
            ).encode("utf8"),
            url="/api/customtextcla",
        )

        resp = main(req)

        self.assertEqual(resp.status_code, 200)

        self.assertEqual(
            json.loads(resp.get_body()),
            {
                "values": [
                    {
                        "recordId": "0",
                        "data": {
                            "class": [{"category": "Drama", "confidence_score": 1.0}]
                        },
                        "warnings": [],
                    }
                ]
            },
        )


if __name__ == "__main__":
    unittest.main()
