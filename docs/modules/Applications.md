# Applications Module

## Overview

The Applications module lets a candidate record and track the roles and spontaneous CV submissions they manage during a job search.

The module owns application data in the `applications` database schema. It does not access Identity tables directly; candidate ownership comes from the authenticated user context.

## Current Capability

A candidate can create and list only their own applications.

Each application records:

- Company name
- Optional role title
- Application type: a specific job posting or a spontaneous CV submission
- Current status
- Applied-on date
- Optional source URL and notes

The current API is protected by authentication:

- `GET /api/applications` lists the current candidate's applications.
- `POST /api/applications` creates an application for the current candidate.

Creating an application publishes the `JobApplicationCreated` integration event.

## Statuses

| Status | Meaning |
| --- | --- |
| Draft | An incomplete record that is not yet ready for action. |
| Planned | A real opportunity the candidate intends to pursue but has not submitted. |
| Applied | A role application or CV has been sent; `AppliedOn` is required. |
| In Process | The company is actively considering the candidate. |
| Offer Received | The company has made an offer. |
| Accepted / Declined | The candidate's final offer decision. |
| Rejected / Withdrawn / Archived | A closed application. |

## Invariants

- An application belongs to exactly one candidate.
- Candidates can only list their own applications.
- An application whose status is `Applied` must have an `AppliedOn` date.

The applied-date invariant is enforced by both request validation and the domain factory.

## Deferred Work

The initial tracker intentionally does not yet include:

- A shared company catalogue, company search, or candidate company suggestions
- Application editing, interview completion, rescheduling, cancellation, assessments, or offers
- Follow-up and interview reminders
- Documents, CV versions, or cover letters
