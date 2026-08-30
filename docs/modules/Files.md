# Files Module

## Overview

The Files module owns file assets, their metadata, storage, and lifecycle. It is the single authority for candidate documents, company logos, and user avatars; other modules store only `FileId` references and use Files contracts rather than its database schema.

## Current Capability

The current candidate Documents page exposes only `CandidateDocument` assets: PDF, DOC, and DOCX files up to 10 MB with validated content signatures. Logo and avatar assets use the same `FileAsset` storage model but are not displayed in that page.

Metadata is stored in the `files` schema and bytes are stored through `IFileStorage`. Development uses a local implementation configured by `Files:Storage:LocalPath`; object storage can replace that provider without changing the domain or API contract.

Files support bindings from an asset to a consuming resource and usage, such as `Company / {companyId} / Logo`, with an access policy. Candidate documents remain owner-only. Application CV submissions validate active candidate-owned documents through an internal module query, then publish a submission event that Files handles by creating permanent owner-only bindings. A company logo binding will use `ResourceViewers`, meaning its authorization follows the company-details policy. A user-avatar binding will use `OwnerAndAdministrators` until Identity supplies profile visibility. Unbound uploads are intentionally temporary and are eligible for future expiration cleanup.

`DELETE /api/files/{documentId}` archives a document and removes it from new selections. An unbound archived document is unavailable for download. If the document has an active historical binding, its owner can still download it from the application submission history. Metadata and blob content remain under Files ownership.

The module publishes document-uploaded and document-archived integration events.

## Deferred Work

- Referential integrity checks before a retention-policy-driven physical purge
- Malware scanning, asynchronous large-file processing, and cloud/object-storage providers
- Company logo and avatar upload/binding workflows
- Expiration worker for abandoned, unbound uploads
