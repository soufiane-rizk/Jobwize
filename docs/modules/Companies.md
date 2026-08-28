# Companies Module

## Current capabilities

The Companies module owns the company catalogue and its locations. Candidates can create a private company, including one or more locations. The Applications module exposes the candidate-facing searchable selector from its local projection, while a candidate-created company is written through this module.

Companies have a visibility state of `Shared`, `Private`, or `PendingReview`. Candidate-created companies are private by default.

Admins and superadmins can use the company-review queue to approve or reject candidate-created companies. Approval can correct the basic company profile (name, website, industry, and description) before promoting it to the shared catalogue. Rejection keeps the company private and records a required reason, reviewer, and review date. The module publishes promotion and rejection integration events.

The Applications module maintains its own local company selection/display projection through internal module queries and company-created/promoted integration events. This avoids synchronous cross-module reads for normal application pages.

Candidates can open a company details page that shows the company profile, its locations, and their own applications to that company. The frontend composes this page from the Companies company-details endpoint and an Applications query filtered by company identifier.

Reusable contacts, company rename/update and removal integration events, and ongoing company/location management are deferred to subsequent work.

## Module boundary

Companies owns its `companies` schema, persistence, contracts, domain model, and candidate-facing endpoints. Other modules must use these public endpoints/contracts and must not query the Companies database directly.

Applications already maintains local company and location projections for its selection, list, and detail reads. It does not synchronously query the Companies database.
