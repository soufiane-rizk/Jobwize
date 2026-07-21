# Contributing to JobWize

First of all, thank you for your interest in contributing to **JobWize**! ❤️

JobWize is an open-source Career Management Platform designed to help job seekers organize, track, and optimize their job search while demonstrating modern Software Engineering and DevOps best practices.

Whether you're fixing a typo, improving documentation, implementing a new feature, optimizing infrastructure, or reporting a bug, every contribution is welcome and appreciated.

---

# Our Philosophy

JobWize is developed as if it were a production software product.

We believe great software is built through:

- Clean and maintainable code
- Clear documentation
- Professional Git workflows
- Code reviews
- Continuous Integration & Continuous Deployment (CI/CD)
- Infrastructure as Code (IaC)
- Automated testing
- Continuous improvement

> **Quality is always more important than speed.**

---

# Core Maintainers

JobWize is currently maintained by two core contributors, each leading different areas of the project.

---

## 👨‍💻 Soufiane Rizk

### DevOps Engineer • Open Source Maintainer • Project Governance

Responsible for:

### Project & Repository

- Project governance
- Repository management
- Development workflow
- Engineering standards
- Open Source management
- Documentation management
- Release management

### DevOps

- GitHub & GitLab administration
- Git workflow
- CI/CD pipelines
- GitHub Actions
- GitLab CI/CD

### Infrastructure

- Docker
- Docker Compose
- Kubernetes (K3s)
- Terraform
- Ansible
- Infrastructure as Code (IaC)
- Azure Cloud
- Server administration
- Container Registry

### Database Operations

- Database administration
- Database deployment
- Database backup & restore
- Database monitoring
- Database maintenance
- Performance monitoring
- High availability strategy

### Monitoring & Observability

- Prometheus
- Grafana
- Loki
- AlertManager
- Logging
- Metrics
- Alerting

### Security

- Secrets management
- Infrastructure security
- DevSecOps
- Backup strategy
- Disaster Recovery
- SSL/TLS management

### Automation

- Python Workers infrastructure
- Development environment
- Deployment automation
- Environment configuration

---

## 👨‍💻 Ahmed Amine Abahmane

### Software Architect • Full Stack Developer

Responsible for:

### Software Architecture

- Solution Architecture
- Software Architecture
- Clean Architecture
- Domain-Driven Design (DDD)
- Modular Monolith architecture
- System design

### Backend Development

- ASP.NET Core Web API
- Business Logic
- Domain layer
- Application layer
- Infrastructure layer
- Dependency Injection
- MediatR
- FluentValidation

### Frontend Development

- Blazor WebAssembly
- MudBlazor
- UI Components
- State Management
- User Experience

### Database Engineering

- Database Architecture
- PostgreSQL
- Entity Framework Core
- Database modeling
- Database migrations
- Query optimization
- Performance tuning

### API

- REST API Design
- Authentication
- Authorization
- Identity
- API Versioning
- API Documentation

### Quality

- Unit Testing
- Integration Testing
- Performance Optimization
- Code Refactoring
- Code Quality

### Automation

- Python Workers development
- AI Integration
- Background Services

---

# Development Workflow

Every contribution follows the same workflow.

```text
Jira Issue
      │
      ▼
Create Branch
      │
      ▼
Develop
      │
      ▼
Commit
      │
      ▼
Push
      │
      ▼
Open Pull Request
      │
      ▼
Code Review
      │
      ▼
Approval
      │
      ▼
Squash & Merge
      │
      ▼
Close Jira Issue
```

Direct pushes to protected branches are not allowed.

---

# Jira Workflow

JobWize uses **Jira** as its official project management platform following the Agile philosophy and the Scrum framework.

## Project Hierarchy

```text
Epic
    ↓
Story
    ↓
Task
    ↓
Sub-task (optional)
```

## Rules

- Project work is organized and tracked in Jira.
- Jira uses the hierarchy Epic, Story, Task, and optional Sub-task.
- Repository changes must be submitted through a Pull Request.
- A Jira Issue is marked as **Done** after the related work has been completed and merged into `develop`.

---

# Branch Strategy

We follow a Git Flow-inspired branching strategy.

| Branch | Purpose |
|---------|---------|
| `main` | Production-ready code |
| `develop` | Active development |
| `feature/*` | New features |
| `bugfix/*` | Bug fixes |
| `hotfix/*` | Critical production fixes |
| `docs/*` | Documentation |
| `infra/*` | Infrastructure & DevOps |
| `ci/*` | CI/CD improvements |

## Examples

```text
feature/authentication
feature/dashboard
feature/company-management

bugfix/login-validation

docs/update-readme
docs/update-contributing-guide

infra/docker-compose
infra/kubernetes

ci/gitlab-pipeline
```

---

# Commit Messages

We use the **Conventional Commits** specification.

## Format

```text
type(scope): short description
```

## Examples

```text
feat(identity): add authentication module

fix(identity): resolve login validation issue

docs(contributing): update contribution guidelines

ci(gitlab): add deployment pipeline

build(docker): update backend image

chore(terraform): create project structure

test(identity): add login integration tests

perf(dashboard): optimize dashboard queries
```

## Commit Types

| Type | Description |
|------|-------------|
| `feat` | New feature |
| `fix` | Bug fix |
| `docs` | Documentation |
| `refactor` | Code improvement |
| `test` | Tests |
| `ci` | Continuous Integration |
| `build` | Build system, Docker, or dependencies |
| `perf` | Performance |
| `chore` | Maintenance |

---

# Pull Requests

Every change must be submitted through a Pull Request.

A Pull Request should:

- Focus on a single topic
- Have a meaningful title
- Include a clear description
- Reference the related Jira Issue when applicable
- Pass CI checks
- Be reviewed before merging

Small Pull Requests are easier to review and maintain.

---

# Code Reviews

Code reviews improve software quality and knowledge sharing.

General rules:

- Authors do not approve their own Pull Requests.
- Reviews should be constructive and respectful.
- Discussions should focus on improving the project.
- Every Pull Request must be reviewed before merging.

Current review ownership:

| Area | Primary Reviewer |
|------|------------------|
| Backend & Frontend | Ahmed Amine Abahmane |
| Infrastructure & DevOps | Soufiane Rizk |
| Documentation | Both maintainers |

---

# Merge Strategy

JobWize uses **Squash and Merge**.

Benefits:

- Clean Git history
- Easier release management
- Better changelog generation
- One commit per Pull Request

The final squash commit should follow the Conventional Commits specification.

Example:

```text
docs(contributing): update contribution guidelines
```

---

# Coding Standards

Before opening a Pull Request:

- Follow the existing project conventions.
- Write clean and readable code.
- Use meaningful names.
- Remove unused code.
- Update documentation when necessary.
- Test your changes whenever possible.

---

# Security

Never commit:

- Passwords
- API Keys
- Secrets
- Tokens
- Certificates
- Private SSH Keys
- Production configuration
- Real `.env` files
- Terraform state files
- Sensitive Terraform variable files

Use:

- Environment Variables
- GitHub Secrets
- GitLab CI/CD Variables
- Kubernetes Secrets
- Azure Key Vault (future)

---

# Reporting Bugs

Please include:

- Operating System
- Browser (if applicable)
- Steps to reproduce
- Expected behavior
- Actual behavior
- Screenshots
- Logs (if applicable)

---

# Suggesting Features

Feature requests should explain:

- The problem
- The proposed solution
- Possible alternatives
- Additional context

---

# Questions

If you have questions or suggestions:

- Open a GitHub Discussion
- Create an Issue
- Contact one of the maintainers

---

# Project Leadership

| Name | Role |
|------|------|
| **Soufiane Rizk** | DevOps Engineer • Open Source Maintainer • Project Governance |
| **Ahmed Amine Abahmane** | Software Architect • Full Stack Developer |

Together, we are building JobWize as a production-grade open-source platform while applying modern Software Engineering, DevOps, Cloud Native, and Infrastructure as Code practices.

---

# Thank You

Thank you for contributing to JobWize.

Every contribution—whether it's code, documentation, infrastructure, testing, bug reports, or ideas—helps improve the project for everyone.

**Happy Coding! 🚀**
