import json
import os
import unittest

import azure.functions as func
from customtextcla.main import main, result_to_json
from azure.ai.textanalytics import SingleCategoryClassifyResult, ClassificationCategory


class TestFunction(unittest.TestCase):
    def setUp(self) -> None:
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
        result = [page]
        self.maxDiff = None
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
                                "text": "We consider a scalar balance law with a strict convex flux. In this paper, we study L-2 stability to a shock for a scalar balance law up to a shift function, which is based on the relative entropy. This result generalizes Leger's works [18] and provides more a simple proof than Leger's proof.",
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
                            "class": [
                                {
                                    "category": "Mechanical_engineering",
                                    "confidence_score": 1.0,
                                }
                            ]
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
                                "text": "We consider a scalar balance law with a strict convex flux. In this paper, we study L-2 stability to a shock for a scalar balance law up to a shift function, which is based on the relative entropy. This result generalizes Leger's works [18] and provides more a simple proof than Leger's proof.",
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
                                "lang": "en",
                                "text": "We consider a scalar balance law with a strict convex flux. In this paper, we study L-2 stability to a shock for a scalar balance law up to a shift function, which is based on the relative entropy. This result generalizes Leger's works [18] and provides more a simple proof than Leger's proof.",
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
                            "class": [
                                {
                                    "category": "Mechanical_engineering",
                                    "confidence_score": 1.0,
                                }
                            ]
                        },
                        "warnings": [],
                    }
                ]
            },
        )


if __name__ == "__main__":
    unittest.main()
