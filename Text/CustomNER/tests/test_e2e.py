import json
import os
import unittest
from copy import deepcopy

import azure.functions as func
import jsonschema

from custom_ner.main import (
    main,
)


class TestFunction(unittest.TestCase):
    def __init__(self, methodName="runTest"):
        super().__init__(methodName)
        self.req_body = {
            "values": [
                {
                    "recordId": "0",
                    "data": {
                        "text": "Date 10/18/2019\n\nThis is a Loan agreement between the two individuals mentioned "
                        "below in the parties section of the agreement.\n\nI. Parties of agreement\n\n- Casey "
                        "Jensen with a mailing address of 2469 Pennsylvania Avenue, City of New Brunswick, "
                        'State of New Jersey (the "Borrower")\n- Hollie Rees with a mailing address of 42 '
                        'Gladwell Street, City of Memphis, State of Tennessee (the "Lender")\n\nII. '
                        "Amount\nThe loan amount given by lender to borrower is one hundred ninety-two "
                        'thousand nine hundred eighty-nine Dollars ($192,989.00) ("The Note")\n\nIII. '
                        "Interest\nThe Note shall bear interest five percent (5%) compounded annually.\n\nIV. "
                        'Payment\nThe amount mentioned in this agreement (the "Note"), including the '
                        "principal and any accrued interest, is\n\nV. Payment Terms\nAny delay in payment is "
                        "subject to a fine with a flat amount of $50 for every week the payment is "
                        "delayed.\nAll payments made by the Borrower shall be go into settling the the "
                        "accrued interest  and any late fess and then into the payment of the principal "
                        "amount.\n\nVI. Prepayment\nThe borrower is able to pay back the Note in full at any "
                        "time, thus terminating this agreement.\nThe borrower also can make additional "
                        "payments at any time and this will take of from the amount of the latest "
                        "installments. \n\nVII. Acceleration.\nIn case of Borrower's failure to pay any part "
                        "of the principal or interest as and when due under this Note; or Borrower's "
                        "becoming insolvent or not paying its debts as they become due. The lender has the "
                        'right to declare an "Event of Acceleration" in which case the Lender has the right '
                        "to to declare this Note immediately due and payable \n\nIX. Succession\nThis Note "
                        "shall outlive the borrower and/or the lender in the even of their death. This note "
                        "shall be binging to any of their successors.",
                        "lang": "en",
                    },
                },
                {
                    "recordId": "1",
                    "data": {
                        "text": "Date 6/28/2019\n\nThis is a Loan agreement between the two individuals mentioned "
                        "below in the parties section of the agreement.\n\nI. Parties of agreement\n\n- Andre "
                        "Lawson with a mailing address of 4759 Reel Avenue, City of Las Cruces, State of New "
                        'Mexico (the "Borrower")\n- Malik Barden with a mailing address of 3093 Keyser '
                        'Ridge Road, City of Greensboro, State of North Carolina (the "Lender")\n\nII. '
                        "Amount\nThe loan amount given by lender to borrower is eight two hundred "
                        'thirty-eight thousand eight hundred sixty-nine Dollars ($238,869.00) ("The '
                        'Note")\n\nIII. Interest\nThe Note shall bear interest five percent (7%) compounded '
                        'annually.\n\nIV. Payment\nThe amount mentioned in this agreement (the "Note"), '
                        "including the principal and any accrued interest, is\n\nV. Payment Terms\nAny delay "
                        "in payment is subject to a fine with a flat amount of $50 for every week the payment "
                        "is delayed.\nAll payments made by the Borrower shall be go into settling the the "
                        "accrued interest  and any late fess and then into the payment of the principal "
                        "amount.\n\nVI. Prepayment\nThe borrower is able to pay back the Note in full at any "
                        "time, thus terminating this agreement.\nThe borrower also can make additional "
                        "payments at any time and this will take of from the amount of the latest "
                        "installments. \n\nVII. Acceleration.\nIn case of Borrower's failure to pay any part "
                        "of the principal or interest as and when due under this Note; or Borrower's becoming "
                        "insolvent or not paying its debts as they become due. The lender has the right to "
                        'declare an "Event of Acceleration" in which case the Lender has the right to to '
                        "declare this Note immediately due and payable \n\nIX. Succession\nThis Note shall "
                        "outlive the borrower and/or the lender in the even of their death. This note shall "
                        "be binging to any of their successors.",
                        "lang": "en",
                    },
                },
            ]
        }

        self.resp_body = {
            "values": [
                {
                    "recordId": "0",
                    "data": {
                        "entities": [
                            {
                                "text": "10/18/2019",
                                "category": "Date",
                                "subcategory": None,
                                "length": 10,
                                "offset": 5,
                                "confidence_score": 1.0,
                            },
                            {
                                "text": "Casey Jensen",
                                "category": "BorrowerName",
                                "subcategory": None,
                                "length": 12,
                                "offset": 155,
                                "confidence_score": 1.0,
                            },
                            {
                                "text": "2469 Pennsylvania Avenue",
                                "category": "BorrowerAddress",
                                "subcategory": None,
                                "length": 24,
                                "offset": 194,
                                "confidence_score": 0.99,
                            },
                            {
                                "text": "New Brunswick",
                                "category": "BorrowerCity",
                                "subcategory": None,
                                "length": 13,
                                "offset": 228,
                                "confidence_score": 0.95,
                            },
                            {
                                "text": "New Jersey",
                                "category": "BorrowerState",
                                "subcategory": None,
                                "length": 10,
                                "offset": 252,
                                "confidence_score": 0.81,
                            },
                            {
                                "text": "Hollie Rees",
                                "category": "LenderName",
                                "subcategory": None,
                                "length": 11,
                                "offset": 282,
                                "confidence_score": 1.0,
                            },
                            {
                                "text": "42 Gladwell Street",
                                "category": "LenderAddress",
                                "subcategory": None,
                                "length": 18,
                                "offset": 320,
                                "confidence_score": 1.0,
                            },
                            {
                                "text": "Memphis",
                                "category": "LenderCity",
                                "subcategory": None,
                                "length": 7,
                                "offset": 348,
                                "confidence_score": 1.0,
                            },
                            {
                                "text": "Tennessee",
                                "category": "LenderState",
                                "subcategory": None,
                                "length": 9,
                                "offset": 366,
                                "confidence_score": 1.0,
                            },
                            {
                                "text": "one hundred ninety-two thousand nine hundred eighty-nine Dollars",
                                "category": "LoanAmountWords",
                                "subcategory": None,
                                "length": 64,
                                "offset": 450,
                                "confidence_score": 1.0,
                            },
                            {
                                "text": "$192,989.00",
                                "category": "LoanAmountNumbers",
                                "subcategory": None,
                                "length": 11,
                                "offset": 516,
                                "confidence_score": 1.0,
                            },
                            {
                                "text": "5%",
                                "category": "Interest",
                                "subcategory": None,
                                "length": 2,
                                "offset": 600,
                                "confidence_score": 1.0,
                            },
                        ]
                    },
                    "warnings": [],
                },
                {
                    "recordId": "1",
                    "data": {
                        "entities": [
                            {
                                "text": "6/28/2019",
                                "category": "Date",
                                "subcategory": None,
                                "length": 9,
                                "offset": 5,
                                "confidence_score": 1.0,
                            },
                            {
                                "text": "Andre Lawson",
                                "category": "BorrowerName",
                                "subcategory": None,
                                "length": 12,
                                "offset": 154,
                                "confidence_score": 1.0,
                            },
                            {
                                "text": "4759 Reel Avenue",
                                "category": "BorrowerAddress",
                                "subcategory": None,
                                "length": 16,
                                "offset": 193,
                                "confidence_score": 0.98,
                            },
                            {
                                "text": "Las Cruces",
                                "category": "BorrowerCity",
                                "subcategory": None,
                                "length": 10,
                                "offset": 219,
                                "confidence_score": 0.99,
                            },
                            {
                                "text": "New Mexico",
                                "category": "BorrowerState",
                                "subcategory": None,
                                "length": 10,
                                "offset": 240,
                                "confidence_score": 0.99,
                            },
                            {
                                "text": "Malik Barden",
                                "category": "LenderName",
                                "subcategory": None,
                                "length": 12,
                                "offset": 270,
                                "confidence_score": 1.0,
                            },
                            {
                                "text": "3093 Keyser Ridge Road",
                                "category": "LenderAddress",
                                "subcategory": None,
                                "length": 22,
                                "offset": 309,
                                "confidence_score": 1.0,
                            },
                            {
                                "text": "Greensboro",
                                "category": "LenderCity",
                                "subcategory": None,
                                "length": 10,
                                "offset": 341,
                                "confidence_score": 1.0,
                            },
                            {
                                "text": "North Carolina",
                                "category": "LenderState",
                                "subcategory": None,
                                "length": 14,
                                "offset": 362,
                                "confidence_score": 0.98,
                            },
                            {
                                "text": "eight two hundred thirty-eight thousand eight hundred sixty-nine Dollars",
                                "category": "LoanAmountWords",
                                "subcategory": None,
                                "length": 72,
                                "offset": 451,
                                "confidence_score": 1.0,
                            },
                            {
                                "text": "$238,869.00",
                                "category": "LoanAmountNumbers",
                                "subcategory": None,
                                "length": 11,
                                "offset": 525,
                                "confidence_score": 1.0,
                            },
                            {
                                "text": "7%",
                                "category": "Interest",
                                "subcategory": None,
                                "length": 2,
                                "offset": 609,
                                "confidence_score": 1.0,
                            },
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
                                            "entities": {
                                                "type": "array",
                                                "items": {
                                                    "type": "object",
                                                    "properties": {
                                                        "text": {"type": "string"},
                                                        "category": {"type": "string"},
                                                        "subcategory": {
                                                            "type": ["string", "null"]
                                                        },
                                                        "length": {"type": "integer"},
                                                        "offset": {"type": "integer"},
                                                        "confidence_score": {
                                                            "type": "number"
                                                        },
                                                    },
                                                    "required": [
                                                        "text",
                                                        "category",
                                                        "subcategory",
                                                        "length",
                                                        "offset",
                                                        "confidence_score",
                                                    ],
                                                },
                                            }
                                        },
                                        "required": ["entities"],
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
        self.maxDiff = None
        env_path = ".env"
        with open(env_path, "r") as f:
            env = dict(
                tuple(line.replace("\n", "").split("="))
                for line in f.readlines()
                if not line.startswith("#")
            )
            for item in env.items():
                os.environ[item[0]] = item[1]

    def test_normal(self):
        req = func.HttpRequest(
            method="GET",
            body=json.dumps(self.req_body).encode("utf8"),
            url="/api/custom_ner",
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

            self.assertTrue("entities" in data.keys())
            entities = data["entities"]

            for correct_entity in correct_record["data"]["entities"]:
                matching_entities = list(
                    filter(lambda e: e["text"] == correct_entity["text"], entities)
                )
                self.assertEqual(len(matching_entities), 1)
                tested_entity = matching_entities[0]
                self.assertEqual(correct_entity["category"], tested_entity["category"])
                self.assertEqual(
                    correct_entity["subcategory"], tested_entity["subcategory"]
                )
                self.assertEqual(correct_entity["length"], tested_entity["length"])
                self.assertEqual(correct_entity["offset"], tested_entity["offset"])
                self.assertAlmostEqual(
                    correct_entity["confidence_score"],
                    tested_entity["confidence_score"],
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

    def test_lang_optional(self):
        body = deepcopy(self.req_body)
        del body["values"][0]["data"]["lang"]
        req = func.HttpRequest(
            method="GET",
            body=json.dumps(body).encode("utf8"),
            url="/api/custom_ner",
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

            self.assertTrue("entities" in data.keys())
            entities = data["entities"]

            for correct_entity in correct_record["data"]["entities"]:
                matching_entities = list(
                    filter(lambda e: e["text"] == correct_entity["text"], entities)
                )
                self.assertEqual(len(matching_entities), 1)
                tested_entity = matching_entities[0]
                self.assertEqual(correct_entity["category"], tested_entity["category"])
                self.assertEqual(
                    correct_entity["subcategory"], tested_entity["subcategory"]
                )
                self.assertEqual(correct_entity["length"], tested_entity["length"])
                self.assertEqual(correct_entity["offset"], tested_entity["offset"])
                self.assertAlmostEqual(
                    correct_entity["confidence_score"],
                    tested_entity["confidence_score"],
                    delta=0.2,
                )

    def test_too_long(self):
        body = deepcopy(self.req_body)
        body["values"][0]["data"]["text"] = "very_long_input" * 125000
        req = func.HttpRequest(
            method="GET",
            body=json.dumps(body).encode("utf8"),
            url="/api/custom_ner",
        )

        resp = main(req)

        self.assertNotEqual(resp.status_code, 200)

    def test_document_error(self):
        body = deepcopy(self.req_body)
        body["values"][0]["data"]["lang"] = "not_a_language"
        erroneous_recordid = body["values"][0]["recordId"]
        req = func.HttpRequest(
            method="GET",
            body=json.dumps(body).encode("utf8"),
            url="/api/custom_ner",
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

                self.assertTrue("entities" in data.keys())
                entities = data["entities"]

                for correct_entity in correct_record["data"]["entities"]:
                    matching_entities = list(
                        filter(lambda e: e["text"] == correct_entity["text"], entities)
                    )
                    if len(matching_entities) != 1:
                        print("lol")
                    self.assertEqual(len(matching_entities), 1)
                    tested_entity = matching_entities[0]
                    self.assertEqual(
                        correct_entity["category"], tested_entity["category"]
                    )
                    self.assertEqual(
                        correct_entity["subcategory"], tested_entity["subcategory"]
                    )
                    self.assertEqual(correct_entity["length"], tested_entity["length"])
                    self.assertEqual(correct_entity["offset"], tested_entity["offset"])
                    self.assertAlmostEqual(
                        correct_entity["confidence_score"],
                        tested_entity["confidence_score"],
                        delta=0.2,
                    )
