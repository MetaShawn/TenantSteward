# TenantSteward

A governance assistant for Microsoft 365 oversharing. It scans SharePoint for risky sharing,
scores and stores the findings, lets a reviewer triage and approve remediation, and lets an
agent answer "why is this site risky and what should we do?"

> **Status: scaffold.** The scoring library is real; everything else is a documented stub with
> the first commands to run. Nothing here has been compiled or deployed yet.

## Architecture

```
Microsoft Graph (SharePoint)
        |
        v
Scanner (Azure Function, C#) --writes--> Dataverse: ScanRun, Finding, Remediation
        ^                                   |  plug-in: risk scoring, status rules
        |                                   |  custom API: ApproveRemediation
   OpenAPI spec --> custom connector        |
                          |                 v
                          +--> Power Automate: approval flow (child flows, retries)
                          +--> Copilot Studio agent / MCP server: Q&A over findings
                                            |
                          Reviewer UI: PCF risk control + React code app
```

## Repo layout

| Path | What lives there |
|---|---|
| `src/RiskScoring/` | Pure C# scoring library shared by the Function and the plug-in (real code) |
| `src/scanner-function/` | Azure Function (isolated worker) that scans via Graph |
| `src/dataverse-plugin/` | Dataverse plug-in and custom API |
| `src/pcf/` | PCF control for the finding form |
| `src/code-app/` | Power Apps code app (React/TypeScript) |
| `src/mcp-server/` | Stretch goal: small MCP server over findings |
| `solutions/` | Unpacked Dataverse solution (source of truth for the Power Platform parts) |
| `tests/` | Unit test specs and projects |
| `docs/` | Architecture, data model, roadmap |
| `.github/workflows/` | CI (solution export is a manual starting point) |

## Prerequisites

.NET SDK, Azure Functions Core Tools, Node.js LTS, Power Platform CLI (`pac`), Git, and a
Power Platform developer environment.

## Ground rules

- Scan only tenants you own or are authorized to assess.
- Samples and tests use synthetic data. Never commit secrets, tenant IDs, or real findings.
- Built on a Power Apps Developer Plan: build and test only. Running it for real business use
  needs appropriately licensed users and environments.

## License

MIT. See `LICENSE`.
