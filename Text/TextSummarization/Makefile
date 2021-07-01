help: ## show this help
	@grep -E '^[a-zA-Z0-9_-]+:.*?## .*$$' $(MAKEFILE_LIST) \
	| awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%s\033[0m|%s\n", $$1, $$2}' \
	| column -t -s '|'

local-API: ## deploy the app locally
	./scripts/setup-local.sh

deploy-base: ## deploy base services
	./scripts/deploy-base.sh

build-skill: ## build docker image for custom skill
	docker build -t text_summarization_extractor ./powerskill

push-skill: build-skill ## build docker image for custom skill
	./scripts/tag-and-push-image.sh

run-skill: build-skill ## build and run the skill API locally
	./scripts/docker-run.sh

deploy-skill: ## deploy skill services
	./scripts/deploy-skill.sh

deploy: deploy-base push-skill deploy-skill