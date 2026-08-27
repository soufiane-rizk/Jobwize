# Applications Module

## Overview

The Applications module lets a candidate record and track the roles and spontaneous CV submissions they manage during a job search.

The module owns application data in the `applications` database schema. It does not access Identity tables directly; candidate ownership comes from the authenticated user context.

## Current Capability

A candidate can create, list, and view only their own applications. They can also add timeline notes, move an application through its lifecycle, schedule interviews, edit scheduled interviews, and record interview results.

Each application records:

- Company name
- Optional role title
- Application type: a specific job posting or a spontaneous CV submission
- Current status
- Applied-on date
- Optional source URL and notes
- Activity timeline
- Scheduled and historical interviews, including interviewers, format, duration, location, and preparation notes

The current API is protected by authentication:

- `GET /api/applications` lists the current candidate's applications.
- `POST /api/applications` creates an application for the current candidate.
- `GET /api/applications/{id}` returns an application, its activities, interviews, and allowed next statuses.
- `POST /api/applications/{id}/notes` adds a timeline note.
- `PATCH /api/applications/{id}/status` changes lifecycle status.
- `POST /api/applications/{id}/interviews` schedules an interview.
- `PUT /api/applications/{applicationId}/interviews/{interviewId}` edits a scheduled interview.
- `POST /api/applications/{applicationId}/interviews/{interviewId}/result` records a completed, cancelled, or postponed result.

Commands publish integration events for application creation, status changes, notes, interview scheduling, and interview results.

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
- An application in any status other than `Draft` or `Planned` must have an `AppliedOn` date.
- Status transitions are restricted by the application lifecycle policy; closed applications can only transition to `Archived`.
- Interviews are owned by the job application aggregate.
- A postponed interview creates a copied replacement interview in the `Scheduled` state with the supplied new date and time.

The applied-date invariant is enforced by both request validation and the domain factory.

## Deferred Work

The initial tracker intentionally does not yet include:

- A shared company catalogue, company search, or candidate company suggestions
- Completed-interview outcomes such as awaiting feedback, next round, offer expected, or rejected
- Assessments and offer details
- Follow-up and interview reminders
- Documents, CV versions, or cover letters
