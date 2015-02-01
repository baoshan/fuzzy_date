TESTS = test/*.test.coffee # test/6.test.coffee
REPORTER = spec
NAME    := $(shell node -e "console.log(JSON.parse(require(\
  'fs').readFileSync('package.json', 'utf8')).name)")
VERSION := $(shell node -e "console.log(JSON.parse(require(\
  'fs').readFileSync('package.json', 'utf8')).version)")
TARBALL := $(NAME)-$(VERSION).tgz

build:
	@coffee -c$(opt) -o lib src
test:
	@npm test

publish:
	@rm -Rf package
	@mkdir package
	@cp -R lib package/lib
	@cp package.json package
	@cp ../README.md package
	@tar czf $(TARBALL) package
	@rm -r package

.PHONY: test build
