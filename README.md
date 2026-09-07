RaceDay – Part 1: System Planning and Database
About RaceDay

RaceDay is a full-stack event management platform built for South Africa's road running, walking, and cycling community. The platform allows Event Organisers to create and manage events, define participation categories, and capture results, while Participants can browse upcoming events, enrol in events of their choice, and track their personal race history.

This repository covers Part 1 of the RaceDay Portfolio of Evidence: planning the system's database and API structure before any application code is written. Two later parts build directly on this planning — Part 2 implements the API, and Part 3 builds the MVC web front end that consumes it.

Roles

RaceDay supports two distinct user roles:

Organiser – can create, edit, and delete events, define age or distance categories for their events, view all participants enrolled in their events, and capture finishing times and positions once an event concludes.
Participant – can register an account, browse upcoming events, enrol in an event by selecting a category, view their own enrolment status, and track their personal results across every event they've completed.

Role-based access is planned at the API level in this part, and will be enforced when the API is built in Part 2.

What's in this repository

The /docs folder contains all Part 1 planning deliverables:

RaceDay_ERD.png – Entity Relationship Diagram covering all six entities (Roles, Users, Events, Categories, Enrolments, Results), with primary keys, foreign keys, and cardinality shown for every relationship.
RaceDay_Endpoint_Plan.md – a full table of every planned API endpoint, covering Authentication, User Profile, Events, Categories, Event Enrolments, and Results, including HTTP method, route, description, required role, request body, and expected response for each.
RaceDay_Database.sql – a SQL Server script that creates the full database schema matching the ERD exactly, including all constraints, and seeds the database with sample data (2 Organisers, 2 Participants, 3 Events, categories for each event, and sample enrolments and results).
CI/CD

A GitHub Actions workflow validates the repository structure on every push, confirming the /docs folder exists and contains the required planning files.

Build status:

Video Walkthrough

An unlisted YouTube video walking through the ERD design decisions, the endpoint plan, and a live run of the SQL script in SSMS is available here:

YouTube Video Link – Part 1 Walkthrough

(Replace this link with your actual unlisted YouTube URL once uploaded.)

Running the SQL script
Open SQL Server Management Studio (SSMS) and connect to a local SQL Server instance.
Open docs/RaceDay_Database.sql.
Execute the script (F5). It is safe to re-run — existing tables are dropped and recreated automatically.
Confirm all six tables are created and seeded with no errors in the Messages pane.
