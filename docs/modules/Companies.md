# Companies Module

## Current capabilities

The Companies module owns the company catalogue, locations, and reusable contacts. Candidates can create a private company with locations and contacts, or add a private contact to a company that is already shared. The Applications module exposes the candidate-facing searchable selector from its local projection, while candidate-created company data is written through this module.

Companies have a visibility state of `Shared`, `Private`, or `PendingReview`. Candidate-created companies are private by default.

Admins and superadmins can use the company-review queue to approve or reject candidate-created companies. During approval they can correct the company profile, edit each submitted location and contact, approve or reject each child, and add shared locations or contacts. Admin-added children become shared immediately. Rejected candidate data remains private and available to its creator; it is not deleted.

Candidate-created contacts added after a company is shared use a separate review queue. An approved contact becomes reusable by all candidates. A rejected contact remains visible only to its creator.

The admin company-catalogue screen supports ongoing edits to shared companies, locations, and contacts only. It deliberately excludes private and rejected candidate submissions; those remain visible only to their creator and the relevant review queue. Shared locations and contacts can be enabled or disabled without deleting historical data. Disabled children are not returned by candidate selection endpoints. Existing application, interview, or CV references can therefore retain their identifiers while new forms stop offering obsolete data.

Company, location, and contact review records retain the reviewer, review date, and optional approval or required rejection reason. The module publishes company creation, promotion, catalogue-update, rejection, and contact-creation integration events where downstream synchronization is required.

The Applications module maintains its own local company selection/display projection through internal module queries and company-created, promoted, and catalogue-updated integration events. Location projections include visibility, creator ownership, and active state so rejected private locations remain selectable only by their creator and disabled locations disappear from new application forms. This avoids synchronous cross-module reads for normal application pages.

Candidates can open a company details page that shows the company profile, its locations, and their own applications to that company. The frontend composes this page from the Companies company-details endpoint and an Applications query filtered by company identifier.

Contacts are currently reusable company data and may optionally reference a company location. Binding contacts to interviews, CV deliveries, recruiters, and hiring managers remains deferred to those workflows.

## Module boundary

Companies owns its `companies` schema, persistence, contracts, domain model, and candidate-facing endpoints. Other modules must use these public endpoints/contracts and must not query the Companies database directly.

Applications already maintains local company and location projections for its selection, list, and detail reads. It does not synchronously query the Companies database. The frontend may compose calls to both modules where a page owns a cross-module user workflow.
