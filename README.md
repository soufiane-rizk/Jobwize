<p align="center">
  <img src="docs/assets/jobwize-banner.svg" alt="JobWize Banner" width="100%">
</p>

<p align="center">
  <strong>Open Source Career Management Platform</strong>
</p>

<p align="center">
  Organize • Track • Optimize • Succeed
</p>

<p align="center">
  <img src="https://img.shields.io/github/license/soufiane-rizk/jobwize" alt="License" />
  <img src="https://img.shields.io/github/stars/soufiane-rizk/jobwize" alt="Stars" />
  <img src="https://img.shields.io/github/issues/soufiane-rizk/jobwize" alt="Issues" />
  <img src="https://img.shields.io/github/issues-pr/soufiane-rizk/jobwize" alt="Pull Requests" />
  <img src="https://img.shields.io/badge/.NET-10-512BD4" alt=".NET 10" />
  <img src="https://img.shields.io/badge/Blazor-WebAssembly-5C2D91" alt="Blazor" />
  <img src="https://img.shields.io/badge/PostgreSQL-17-336791" alt="PostgreSQL" />
  <img src="https://img.shields.io/badge/status-active-success" alt="Project Status"/>
</p>

<p align="center">

JobWize is an open-source platform that helps job seekers organize, track, and optimize their entire job search from a single application.

Instead of relying on spreadsheets, emails, bookmarks, and scattered notes, JobWize centralizes every step of the job search—from companies and contacts to applications, interviews, follow-ups, documents, and analytics.

Beyond being a productivity tool, JobWize is also built as a professional open-source project that demonstrates modern Software Engineering, DevOps, and Cloud Native practices.

</p>
## 💡 Why JobWize?

Managing a job search shouldn't require multiple spreadsheets, browser bookmarks, email folders, and handwritten notes.

JobWize provides a centralized workspace where candidates can:

-   Track job applications
-   Manage companies and recruiters
-   Schedule interviews and follow-ups
-   Store resumes and supporting documents
-   Monitor progress with dashboards and analytics

## At the same time, JobWize serves as a real-world open-source project built using modern Software Engineering, DevOps, and Cloud Native practices.

## 📚 Table of Contents

-   [✨ Features](#-features)
-   [🏗️ Architecture](#-architecture)
-   [⚙️ Technology Stack](#-technology-stack)
-   [🚀 Getting Started](#-getting-started)
-   [📖 Documentation](#-documentation)
-   [🗺️ Roadmap](#-roadmap)
-   [🤝 Contributing](#-contributing)
-   [📜 License](#-license)

## 🎯 Mission

JobWize has two primary goals:

1. Build a production-ready application that helps job seekers manage their career opportunities.
2. Learn, apply, and showcase professional Software Engineering and DevOps practices.

This is **not** a tutorial project.

Every feature is designed, documented, reviewed, tested, automated, deployed, and monitored as if it were part of a production SaaS application.

---

## ✨ Features

### Version 1 – MVP

-   🔐 Authentication & Authorization
-   📊 Dashboard
-   🏢 Companies Management
-   👥 Contacts Management
-   💼 Job Applications Tracking
-   📅 Interview Management
-   🔔 Follow-ups & Reminders
-   📄 Document Management
-   📈 Statistics & Analytics
-   🔔 Notifications

---

## 🏗 Architecture

```
                Blazor WebAssembly
                        │
                        ▼
              ASP.NET Core Web API
                        │
                        ▼
                  PostgreSQL Database
                        │
        ┌───────────────┴───────────────┐
        │                               │
      Redis                          MinIO
     (Cache)                     (Documents)
```

The project follows a **Modular Monolith** architecture built on **Clean Architecture** principles to keep the codebase maintainable, testable, and ready for future evolution.

---

## ⚙ Technology Stack

### Frontend

-   Blazor WebAssembly
-   MudBlazor
-   .NET

### Backend

-   ASP.NET Core Web API
-   Entity Framework Core
-   PostgreSQL
-   Custom Module Runtime
-   FluentValidation

### Infrastructure

-   Docker
-   Docker Compose
-   Kubernetes (K3s)
-   Terraform
-   Ansible

### DevOps

-   GitHub
-   GitLab CI/CD
-   Prometheus
-   Grafana
-   Loki
-   AlertManager

---

## 📂 Repository Structure

```text
jobwize/
│
├── frontend/
├── backend/
├── workers/
├── database/
├── docker/
├── infra/
├── k8s/
├── monitoring/
├── scripts/
├── docs/
└── .github/
```

---

## 🚀 Development Workflow

```text
Feature Branch
        │
        ▼
Pull Request
        │
        ▼
Code Review
        │
        ▼
Merge into develop
        │
        ▼
GitLab CI/CD
        │
        ▼
Deploy to Staging
        │
        ▼
Validation
        │
        ▼
Production
```

---

## 🗺 Roadmap

### Version 1 — Job Application Management

-   Authentication
-   Companies
-   Contacts
-   Applications
-   Interviews
-   Documents
-   Dashboard
-   Analytics

### Version 2 — Discovery & Automation

-   Company Discovery
-   Contact Discovery
-   Data Import
-   AI Enrichment
-   Python Workers

### Version 3 — AI Career Platform

-   Job Aggregation
-   AI Matching
-   Career Assistant
-   Interview Preparation
-   Career Analytics

---

## 🤝 Contributing

Contributions are welcome.

Please read the [CONTRIBUTING.md](CONTRIBUTING.md) guide before opening an issue or submitting a pull request.

---

## 📜 License

This project is licensed under the MIT License.

---

## 👥 Team

| Role               | Responsibilities                              |
| ------------------ | --------------------------------------------- |
| Software Developer | Frontend, Backend, Database                   |
| DevOps Engineer    | Infrastructure, CI/CD, Kubernetes, Monitoring |

---

**Built with ❤️ using modern Software Engineering and DevOps practices.**
