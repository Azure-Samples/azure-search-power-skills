import json
import os
import unittest

import azure.functions as func
from custom_ner.main import main


class TestFunction(unittest.TestCase):
    def setUp(self) -> None:
        self.maxDiff = None
        env_path = ".env"
        with open(env_path, 'r') as f:
            env = dict(tuple(line.replace('\n', '').split('=')) for line in f.readlines() if not line.startswith('#'))
            for item in env.items():
                os.environ[item[0]] = item[1]

        tests_path = "tests/test_custom_ner_samples.json"
        with open(tests_path, 'r') as f:
            self.tests = json.load(f)

    def test_normal(self):
        req = func.HttpRequest(
            method='GET',
            body=json.dumps(self.tests["tests"]["test_normal"]["body"]).encode('utf8'),
            url='/api/custom_ner'
        )

        resp = main(req)

        self.assertEqual(resp.status_code, self.tests["tests"]["test_normal"]["statusCode"])

        self.assertEqual(
            json.loads(resp.get_body()),
            self.tests["tests"]["test_normal"]["response"]
        )

    def test_missing_id(self):
        req = func.HttpRequest(
            method='GET',
            body=json.dumps(self.tests["tests"]["test_missing_id"]["body"]).encode('utf8'),
            url='/api/custom_ner'
        )

        resp = main(req)

        self.assertEqual(resp.status_code, self.tests["tests"]["test_missing_id"]["statusCode"])

    def test_zero_inputs(self):
        req = func.HttpRequest(
            method='GET',
            body=json.dumps(self.tests["tests"]["test_missing_id"]["body"]).encode('utf8'),
            url='/api/custom_ner'
        )

        resp = main(req)

        self.assertEqual(resp.status_code, self.tests["tests"]["test_zero_inputs"]["statusCode"])

    def test_lang_optional(self):
        req = func.HttpRequest(
            method='GET',
            body=json.dumps(self.tests["tests"]["test_lang_optional"]["body"]).encode('utf8'),
            url='/api/custom_ner'
        )

        resp = main(req)

        self.assertEqual(resp.status_code, self.tests["tests"]["test_lang_optional"]["statusCode"])

        self.assertEqual(
            json.loads(resp.get_body()),
            self.tests["tests"]["test_lang_optional"]["response"]
        )

if __name__ == '__main__':
    unittest.main()
