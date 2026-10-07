# Companies Module

## Current capabilities

The Companies module owns the company catalogue and its locations. Candidates can search companies that are shared across the application, together with private companies they created themselves. They can also create a private company, including one or more locations.

Companies have a visibility state of `Shared`, `Private`, or `PendingReview`. Candidate-created companies are private by default.

Admins and superadmins can use the company-review queue to approve or reject candidate-created companies. Approval can correct the basic company profile (name, website, industry, and description) before promoting it to the shared catalogue. Rejection keeps the company private and records a required reason, reviewer, and review date. The module publishes promotion and rejection integration events.

Company detail pages, reusable contacts, Applications snapshot integration, and ongoing company/location management are deferred to subsequent work.

## Module boundary

Companies owns its `companies` schema, persistence, contracts, domain model, and candidate-facing endpoints. Other modules must use these public endpoints/contracts and must not query the Companies database directly.

Applications will later store company and location display snapshots, rather than synchronously querying this module for application lists or details.
