# OpenCand

Open Candidate is a project that aims to provide, in a simpler way, informations about candidates in Brazilian elections. It is a web application that allows users to search for candidates by name, state, and position. The application retrieves data from the TSE (Tribunal Superior Eleitoral) API and displays it in a user-friendly format.

## Architecture

OpenCand is built using:
* .NET 8 - for parsing initial information from CSV files to the PostgreSQL database using Entity Framework.
* .NET Core 8 - for the API
* PostgreSQL - for the database
* React - for the front-end