# Companies Module

## Current capabilities

The Companies module owns the company catalogue and its locations. Candidates can search companies that are shared across the application, together with private companies they created themselves. They can also create a private company, including one or more locations.

Companies have a visibility state of `Shared`, `Private`, or `PendingReview`. Candidate-created companies are private by default. The module stores ownership and review metadata, but moderation commands, administrative UI, promotion/rejection events, company detail pages, reusable contacts, and Applications snapshot integration are deferred to the following planned work.

## Module boundary

Companies owns its `companies` schema, persistence, contracts, domain model, and candidate-facing endpoints. Other modules must use these public endpoints/contracts and must not query the Companies database directly.

Applications will later store company and location display snapshots, rather than synchronously querying this module for application lists or details.
