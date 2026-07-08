# Security Policy

Security is a shared responsibility.

At JobWize, we are committed to protecting our users, contributors, and infrastructure by following secure Software Engineering and DevOps practices.

If you discover a security vulnerability, we appreciate your help in reporting it responsibly.

---

# Supported Versions

JobWize is currently under active development.

Security updates are provided for the following branches:

| Branch | Supported |
|----------|-----------|
| `main` | ✅ |
| `develop` | ✅ |
| Older releases | ❌ |

---

# Reporting a Vulnerability

**Please do not report security vulnerabilities through public GitHub Issues.**

Instead, report them privately to one of the project maintainers.

## Core Maintainers

| Name | Responsibility |
|------|----------------|
| **Soufiane Rizk** | DevOps Engineer • Open Source Maintainer • Project Governance |
| **Ahmed Amine Abahmane** | Software Architect • Full Stack Developer |

A dedicated security contact (for example, `security@jobwize.dev`) may be introduced as the project grows.

---

# What to Include

To help us investigate efficiently, please include:

- A clear description of the vulnerability
- Steps to reproduce
- Affected component or module
- Potential impact
- Screenshots or logs (if applicable)
- Suggested mitigation or fix (optional)

The more information you provide, the faster we can validate and resolve the issue.

---

# Security Scope

Examples of security issues include, but are not limited to:

## Application Security

- Authentication vulnerabilities
- Authorization issues
- Session management problems
- Broken access control
- Sensitive data exposure
- SQL Injection (SQLi)
- Cross-Site Scripting (XSS)
- Cross-Site Request Forgery (CSRF)
- Remote Code Execution (RCE)
- API vulnerabilities

---

## Infrastructure Security

- Docker vulnerabilities
- Kubernetes misconfiguration
- Infrastructure as Code (Terraform/Ansible) issues
- CI/CD pipeline vulnerabilities
- Container Registry exposure
- Cloud infrastructure misconfiguration

---

## Secrets & Credentials

- Exposed API Keys
- Secrets committed to Git
- Tokens
- Certificates
- SSH Keys
- Database credentials

---

# Response Process

After receiving a report, we will follow this process:

1. Acknowledge receipt of the report.
2. Validate the reported vulnerability.
3. Assess the severity and impact.
4. Develop and test a fix.
5. Release the fix.
6. Notify the reporter when appropriate.
7. Publish security information if necessary.

Our goal is to respond as quickly as possible while ensuring a high-quality fix.

---

# Responsible Disclosure

We kindly ask security researchers and contributors to practice responsible disclosure.

Please:

- Give the maintainers reasonable time to investigate.
- Do not publicly disclose the vulnerability before a fix is available.
- Avoid exploiting vulnerabilities beyond what is necessary to demonstrate the issue.

Responsible disclosure helps protect both the project and its users.

---

# Security Best Practices

All contributors should follow secure development practices.

## Never commit

- Passwords
- API Keys
- Access Tokens
- Private SSH Keys
- Certificates
- Production configuration
- Real `.env` files
- Database credentials

---

## Always use

- Environment Variables
- GitHub Secrets
- GitLab CI/CD Variables
- Kubernetes Secrets
- Secure secret management solutions (future)

---

# Security in Development

When contributing to JobWize:

- Keep dependencies up to date.
- Follow secure coding practices.
- Validate user input.
- Apply the principle of least privilege.
- Keep infrastructure configurations secure.
- Review security implications during Pull Requests.

Security is everyone's responsibility.

---

# Reporting False Positives

If you believe a security warning or vulnerability report is incorrect, please provide supporting information so it can be reviewed.

---

# Questions

For security-related questions, please contact one of the project maintainers.

Do not use public GitHub Issues for reporting vulnerabilities.

---

# Acknowledgements

We appreciate everyone who helps improve the security of JobWize through responsible reporting and constructive collaboration.

Thank you for helping us build a safer and more reliable open-source project.