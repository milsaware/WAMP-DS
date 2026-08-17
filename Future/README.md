# WAMP-DS Future

This folder contains ideas for where WAMP-DS could go in the future.

Unlike the [Development History](../History/README.md), which documents what has already happened, this section is about possibilities.

Some of these ideas may eventually become features.

Some may change considerably before they are implemented.

Some may never be built at all.

The purpose of this folder is simply to **capture ideas before they disappear**.

---

## Future Ideas

### 01 - Deployment & FTP

**Status:** ✓ Idea documented

An eventual deployment system allowing projects to be published directly from WAMP-DS.

Potential support could include FTP, SFTP, deployment profiles and remote directory configuration.

[Read the full idea →](./01.md)

---

### 02 - Cloud Deployment

**Status:** ○ Idea

Exploring ways for WAMP-DS to interact directly with cloud and hosting services.

This could eventually allow projects to move from local development to online deployment without leaving the WAMP-DS workflow.

---

### 03 - Project Backup & Rollback

**Status:** ○ Idea

A project backup system designed around development rather than simply copying files.

Potentially including automatic backups, snapshots and the ability to roll a project back to an earlier working state.

---

### 04 - Deployment Profiles

**Status:** ○ Idea

A way to store multiple deployment configurations for a project.

For example:

    Development
    Staging
    Production

Each profile could contain its own destination, credentials and deployment settings.

---

### 05 - Expanded Runtime Support

**Status:** ○ Idea

Continuing to expand the runtimes and services that WAMP-DS can manage.

The long-term goal could be to make adding and managing additional development runtimes as straightforward as installing the core WAMP-DS environment.

---

### 06 - Additional Database Support

**Status:** ○ Idea

Exploring support for additional database systems beyond MySQL.

This could allow WAMP-DS to become useful for a wider range of PHP and web development projects without requiring developers to maintain separate local environments.

---

### 07 - AI-Assisted Development

**Status:** ○ Idea

Exploring future integration with AI development assistants such as GitHub Copilot.

Potential functionality could include AI-assisted code completion, code generation, explanation, refactoring and debugging directly within the WAMP-DS editor.

The goal would be to make AI assistance part of the existing development workflow rather than requiring the developer to constantly switch between applications.


---

## Ideas, Not Promises

The entries in this folder should not be considered a fixed roadmap.

WAMP-DS is being developed through actual use, and that means priorities can change.

An idea may become more important after using the application.

Another may turn out not to be worth pursuing.

A completely different idea may appear while solving an unrelated problem.

That's part of the development process.

The purpose of this folder is to give those ideas somewhere to live.

---

## From Idea To Development

When an idea eventually becomes something worth building, it can move naturally into the development history.

The process may look something like:

    Future Idea
        ↓
    Experiment
        ↓
    Implementation
        ↓
    Real-World Testing
        ↓
    Development History

This keeps the distinction between **what WAMP-DS might become** and **what WAMP-DS actually became**.

---

## A Living Document

This folder will grow alongside WAMP-DS.

Some entries may remain simple ideas.

Others may eventually become detailed technical plans.

And some may eventually disappear entirely because development takes the project somewhere better.

That's fine.

The important thing is to record the ideas while they exist.

The history documents the journey behind WAMP-DS.

This folder documents some of the places the journey might go next.

---

## Status Legend

| Status | Meaning |
|---|---|
| ○ Idea | The idea has been captured but has not yet been documented fully |
| ✓ Idea documented | A dedicated entry exists describing the idea in more detail |
| ⚙ In development | The idea is actively being implemented |
| ✓ Implemented | The idea has become part of WAMP-DS |

These statuses describe the **state of the idea**, not a promise that it will eventually be implemented.