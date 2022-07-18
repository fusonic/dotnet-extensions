# Fusonic blog example
Example for the blog post [Fast unit tests with databases](https://www.fusonic.net/de/blog/fast-unit-tests-with-databases-part-1) on [fusonic.net](https://fusonic.net).

To get the tests running, you need at least
- .NET6 SDK
- Docker 20.10

Then start 
- `1_run_postgres`, which starts a postgres server in docker
- `2_create_testdb`, which creates a test database template on the server

You can either run the tests directly from your IDE (VisualStudio, VS Code, Rider, ...) or run them using the `3_run_tests` script.

Note: 
When running the tests, the postgres service logs tons of error messages

FATAL:  terminating connection due to administrator command

This is fine. The tests clean up after them and force drop the test database when finished.