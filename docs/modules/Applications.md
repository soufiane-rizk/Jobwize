# Applications Module

## Overview

The Applications module lets a candidate record and track the roles and spontaneous CV submissions they manage during a job search.

The module owns application data in the `applications` database schema. It does not access Identity tables directly; candidate ownership comes from the authenticated user context.

## Current Capability

A candidate can create, list, and view only their own applications. They can also add timeline notes, move an application through its lifecycle, record one or more CV submissions, schedule interviews, edit scheduled interviews, record interview results, and manage application reminders.

Each application records:

- Company identifier and optional company-location identifier
- Optional role title
- Application type: a specific job posting or a spontaneous CV submission
- Current status
- Applied-on date
- Optional source URL and notes
- Activity timeline
- Scheduled and historical interviews, including one or more company-contact or manual participants, immutable participant snapshots, format, duration, location, and preparation notes
- CV submissions with sent time, channel, notes, immutable document metadata, and an optional immutable recipient snapshot
- Reminders related to a CV submission, an interview, or a custom application task

The current API is protected by authentication:

- `GET /api/applications` lists the current candidate's applications.
- `POST /api/applications` creates an application for the current candidate.
- `GET /api/applications/{id}` returns an application, its activities, interviews, and allowed next statuses.
- `POST /api/applications/{id}/notes` adds a timeline note.
- `PATCH /api/applications/{id}/status` changes lifecycle status.
- `POST /api/applications/{id}/interviews` schedules an interview.
- `PUT /api/applications/{applicationId}/interviews/{interviewId}` edits a scheduled interview.
- `POST /api/applications/{applicationId}/interviews/{interviewId}/result` records a completed, cancelled, or postponed result.
- `POST /api/applications/{id}/cv-submissions` records a first-class CV submission.
- `POST /api/applications/{id}/reminders` creates an application reminder.
- `PATCH /api/applications/{applicationId}/reminders/{reminderId}/state` completes or dismisses an open reminder.
- `GET /api/applications/agenda?from={utc}&to={utc}` returns the current candidate's interviews and open reminders in a date range.

Commands publish integration events for application creation, status changes, notes, CV submissions, interview scheduling, interview results, reminder creation, and reminder state changes.

Domain invariants are represented by `DomainErrors` in the Applications Domain namespace. Domain Models throw `BusinessRuleException` with those errors; the shared exception behavior maps them to failed results. Application errors remain reserved for request orchestration, authorization, missing aggregates, and cross-module projection checks.

## CV Submissions

The application details screen can record a submission using one or more active documents from the candidate's Files library. Job-posting applications default to the job-portal channel; spontaneous applications default to email. Both application kinds support all channels and an optional company contact. The recipient picker searches contacts for the application’s existing company; when no suitable contact exists, the candidate can create a private contact in place and use it immediately.

Applications validates document ownership and availability through a Files internal module query. It stores immutable document metadata and contact details so later file, contact, approval, or catalogue changes do not rewrite history. The underlying file identifiers remain owned by Files, which creates permanent owner-only submission bindings from the published `JobApplicationCvSubmitted` event.

## Interviews and Participants

An interview can include multiple contacts from the application’s company and any number of manual participants. Contact selection reads only the Applications contact projection: for an application with a selected location, the candidate can choose contacts at that location and company-wide contacts; an application without a location can choose all selectable contacts for the company. An in-place contact form is available when the desired person is absent, defaulting to the application location while still allowing a company-wide contact.

At scheduling and scheduled-interview edit time, Applications validates that every selected contact is active, non-rejected, visible to the candidate, belongs to the application company, and is valid for the application location. It snapshots the selected contact identifier, location label, name, role, email, and phone into the interview aggregate. Manual participant names and roles are also stored in that aggregate. Contact changes, later review, disabling, or removal therefore cannot rewrite interview history; editing a still-scheduled interview intentionally creates a fresh snapshot from the contacts selected then.

Recording a submission on a `Draft` or `Planned` application automatically moves it to `Applied` and sets `AppliedOn` from the actual sent date. Submission history preserves that sent time, while the status-change and CV-submission timeline activities record when the candidate entered the action in JobWize. This keeps backdated submissions chronologically clear. Archived documents are not selectable for new submissions, while previously submitted and subsequently archived documents remain downloadable by their owner from submission history.

## Reminders and Agenda

Reminders belong to the job application aggregate. A reminder is either related to exactly one CV submission, related to exactly one interview, or custom with no related activity. Related submission and interview identifiers are validated against the owning application before the reminder is created.

New reminders start open and can be completed or dismissed. Closed reminders remain visible in application details as history but are excluded from the agenda. Reminder creation and state changes publish integration events so a future notification module can react without accessing the Applications database. The candidate agenda combines all interviews with open reminders for the selected week and resolves company names from Applications' local company projection. The frontend sends UTC range boundaries derived from the candidate's local week so events near midnight remain in the correct displayed day.

The initial agenda deliberately uses the existing MudBlazor components and provides weekly navigation. Notification delivery, preferences, recurring reminders, and a drag-and-drop month calendar are deferred.

## Company Selection and Local Projections

Company ownership remains in the Companies module. The Applications module does not query the `companies` schema.

Applications maintains a local, minimal read projection of company and location identifiers, display labels, visibility, candidate ownership, and active state. The application form reads this projection to show shared companies and the current candidate's private companies. It validates the selected company and location against the same local data before an application is created.

Applications also maintains a local contact projection. `GET /api/applications/company-contacts` returns contacts filtered by optional company, location, and search criteria. It exposes active shared contacts to all candidates and active, unreviewed private contacts only to their creator; rejected contacts are excluded. The projection retains inactive contact data instead of deleting it, preserving a safe basis for future historical activity snapshots.

The application form provides a searchable company selector. If the search has no result, the candidate can open an in-place private-company dialog prefilled with the searched name and optionally add one or more locations. The applications list displays the selected location beneath the company name.

New applications store only `CompanyId` and an optional `CompanyLocationId`; they do not duplicate the company name. Application lists and details resolve the current display name from the local projection. Existing records created before the company link retain their old `CompanyName` database value as a nullable legacy fallback.

The Companies module publishes company-created, company-promoted, company-catalogue-updated, contact-created, and contact-reviewed events. Applications handles these events by synchronizing its local projections through an internal module query. The projections retain visibility, creator ownership, and active state. A SuperAdmin-only recovery endpoint, `POST /api/admin/applications/company-projections/rebuild`, performs a full idempotent rebuild. It marks projections absent from the Companies source inactive instead of deleting them, so historical application links remain intact.

`GET /api/applications?companyId={companyId}` returns only the current candidate's applications for the selected company. The candidate-facing company details page composes this endpoint with `GET /api/companies/{id}` from the Companies module.

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
- CV submissions and their document snapshots are owned by the job application aggregate.
- A CV submission requires at least one unique, active candidate-owned document.
- Submission recipient data is copied from an active selectable contact in the Applications projection.
- Interview participant data is copied from active selectable contacts in the Applications projection, or recorded manually, and remains immutable for historical interviews.
- A postponed interview creates a copied replacement interview in the `Scheduled` state with the supplied new date and time.
- A CV-submission reminder references exactly one submission owned by the same application.
- An interview reminder references exactly one interview owned by the same application.
- A custom reminder does not reference a submission or interview.
- Only an open reminder can be completed or dismissed.

The applied-date invariant is enforced by both request validation and the domain factory.

The Applications unit tests cover company and location availability, CV-submission domain rules, document validation behavior, recipient selection, automatic status transitions, snapshots, reminder relations and state transitions, and integration events.

## Deferred Work

The initial tracker intentionally does not yet include:

- Company removal events and removal policy
- Completed-interview outcomes such as awaiting feedback, next round, offer expected, or rejected
- Assessments and offer details
- Reminder notification delivery, recurrence, preferences, and external calendar synchronization
- Cover-letter-specific behavior and document categorization beyond candidate documents
