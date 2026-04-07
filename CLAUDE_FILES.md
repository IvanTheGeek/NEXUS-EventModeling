# CLAUDE.md Files — Reference and Tutorial

## What They Are

`CLAUDE.md` files are instruction files that Claude Code automatically loads into the context window at the start of every session. They tell Claude how to behave, what conventions to follow, and what context matters for a given project or scope. They are the primary way to give Claude persistent, durable instructions without repeating yourself every session.

---

## Hierarchy — Broadest to Narrowest

| File | Scope | Version Controlled |
|---|---|---|
| `/etc/claude-code/CLAUDE.md` (Linux) | All users, all projects | N/A — managed by IT/DevOps |
| `~/.claude/CLAUDE.md` | You, all projects | No — personal global |
| `/path/to/project/CLAUDE.md` or `.claude/CLAUDE.md` | Team, this project | Yes — commit it |
| `/path/to/project/CLAUDE.local.md` | You, this project only | No — gitignored |
| `.claude/rules/*.md` | File-path-scoped (conditional) | Yes or No — your choice |

### How Load Order Works

Claude Code walks **up** the directory tree from your working directory and loads every `CLAUDE.md` and `CLAUDE.local.md` it finds, from root down to your current directory. All discovered files are **concatenated** into context in order — they do not override each other.

For a session opened at `/home/ivan/nexus/NEXUS-EventModeling`, the load order is:

1. `/etc/claude-code/CLAUDE.md` (managed policy, if present)
2. `~/.claude/CLAUDE.md` (personal global)
3. `/home/ivan/nexus/CLAUDE.md` (workspace-level)
4. `/home/ivan/nexus/NEXUS-EventModeling/CLAUDE.md` (project-level)
5. Any `CLAUDE.local.md` files found at each of those levels

**Subdirectory `CLAUDE.md` files are lazy-loaded** — they only load when Claude opens a file inside that subdirectory, not at session start.

---

## Concatenation, Not Override

All loaded files are merged. If two files give contradictory instructions, the result is ambiguous — Claude may follow either. Write files to **complement** each other across scopes, not conflict. Use narrower files (project, local) to add specificity, not to fight broader files.

---

## `CLAUDE.local.md` — Personal Project Overrides

`CLAUDE.local.md` sits next to `CLAUDE.md` in a project directory. It is:

- Loaded after `CLAUDE.md` at the same scope level (so it is the last thing read at that level)
- Gitignored by default — personal, not shared with the team
- The right place for preferences that are yours alone: editor paths, personal style preferences, local machine context

Use it when you want project-level instructions that aren't appropriate to commit.

---

## `.claude/rules/*.md` — Path-Scoped Rules

Individual `.md` files placed in `.claude/rules/` load **conditionally** — only when Claude opens a file matching a path pattern. Use `paths` frontmatter to define the pattern:

```markdown
---
paths:
  - "EventModeling/**/*.fs"
---

When working in F# files in the EventModeling library, always check compile
order before adding a new type.
```

Rules without a `paths` frontmatter load at session start like a regular `CLAUDE.md`.

Use path-scoped rules to keep project-level `CLAUDE.md` lean — move rules that only apply to specific file areas into scoped rule files.

---

## `@filename` — Inline File Imports

Any `CLAUDE.md` can import another file using `@filename` syntax:

```markdown
@EVENT_MODELING.md
@package.json
@README.md
```

The referenced file is **expanded inline** at session start by the Claude Code CLI — it is not a tool call and does not happen during the conversation. The content appears in context as if it were written directly in `CLAUDE.md`.

### `@filename` vs Condensed Inline vs Read Tool

| Approach | Token cost | Tool call? | Always in context? |
|---|---|---|---|
| `@filename` | Full file, every session | No | Yes |
| Condensed inline in `CLAUDE.md` | Only what you write | No | Yes |
| Read tool during conversation | Only when called | Yes | No |

**Rule of thumb**: if the content is almost always relevant, put a condensed version inline in `CLAUDE.md`. Use `@filename` when you need the full content every session and can afford the tokens. Use the Read tool for deep reference that only matters occasionally.

Up to **5 hops** of recursive imports are supported (`A.md` imports `B.md` imports `C.md` ...).

---

## Managed Policy Files

The system-level file (`/etc/claude-code/CLAUDE.md` on Linux) is controlled by IT or DevOps. It:

- Loads for all users on the machine
- Cannot be excluded or overridden via `claudeMdExcludes`
- Is the right place for organization-wide compliance rules, security policies, or tool restrictions

---

## Excluding Files — `claudeMdExcludes`

In `.claude/settings.local.json`, you can prevent specific `CLAUDE.md` files from loading:

```json
{
  "claudeMdExcludes": [
    "/some/ancestor/CLAUDE.md"
  ]
}
```

Managed policy files cannot be excluded. Everything else can be.

---

## Sync Contracts Between CLAUDE.md and Reference Docs

When a `CLAUDE.md` contains a condensed summary of a larger reference document (like `EVENT_MODELING.md` or `FSHARP.md`), add a sync contract to both files so the obligation is visible at the point of edit:

**In the reference doc** (top of file):
```markdown
> **Sync contract**: A condensed version of this document lives in `CLAUDE.md`
> under `## Section Name`. When this file is updated, update that section too.
```

**In CLAUDE.md** (above the condensed section):
```markdown
> Condensed from `REFERENCE_DOC.md`. Update that file AND this section together.
```

---

## Practical Guidelines

**Keep `CLAUDE.md` under ~150–200 lines.** It is loaded into every context window. The longer it is, the more tokens it consumes per session. Length reduces adherence — a 500-line `CLAUDE.md` is less effective than a tight 100-line one.

**Put durable rules in `CLAUDE.md`, not ephemeral state.** Current task status, in-progress work, and what changed today belong in a plan or todo list — not in `CLAUDE.md`. `CLAUDE.md` is for rules that are true across all sessions.

**Commit the project `CLAUDE.md`.** It is part of the codebase. It encodes the conventions, context, and constraints that any collaborator (human or AI) needs. Treat it like you treat a `README` — keep it accurate.

**Use `CLAUDE.local.md` for machine-specific or personal context.** Things like "my local Postgres runs on port 5433" or "I prefer verbose compiler output" belong there, not in the shared project file.

**Narrow is better than broad for rules.** A rule in a path-scoped `.claude/rules/` file only burns tokens when relevant. A rule in the global `~/.claude/CLAUDE.md` burns tokens in every project. Put rules at the narrowest scope where they apply.

---

## Reference Documents and Where They Live

Two reference documents exist outside the `CLAUDE.md` hierarchy but are linked to it via sync contracts:

| Document | Location | Scope | Condensed in |
|---|---|---|---|
| `FSHARP.md` | `/home/ivan/nexus/` | All NEXUS F# projects | `nexus/CLAUDE.md` `## F#` |
| `EVENT_MODELING.md` | `/home/ivan/nexus/NEXUS-EventModeling/` | All NEXUS projects (methodology) | `nexus/CLAUDE.md` `## Event Modeling` |

These files are **not** CLAUDE.md files and do not auto-load. They are detailed references — read them when the condensed section in `CLAUDE.md` isn't enough. When either is updated, update the corresponding condensed section in `nexus/CLAUDE.md` per the sync contract at the top of each file.

## Current Setup in This Repo

```
~/.claude/CLAUDE.md                             — personal global (style, behavior, validation)
/home/ivan/nexus/CLAUDE.md                      — workspace: Event Modeling methodology + F# reference
/home/ivan/nexus/FSHARP.md                      — full F# notes (condensed into nexus/CLAUDE.md)
/home/ivan/nexus/NEXUS-EventModeling/CLAUDE.md  — library-specific conventions only
/home/ivan/nexus/NEXUS-EventModeling/EVENT_MODELING.md  — full methodology (condensed into nexus/CLAUDE.md)
```

The workspace `CLAUDE.md` carries the condensed Event Modeling and F# sections that apply to all NEXUS projects. The project `CLAUDE.md` carries only what is specific to this library's implementation.
