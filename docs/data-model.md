# Data model (draft)

Publisher prefix: `ts`.

| Table | Purpose | Key columns |
|---|---|---|
| `ts_scanrun` | One execution of the scanner | started, finished, status, sitesScanned |
| `ts_finding` | Exposure found on one site | siteUrl, anonymousLinks, orgLinks, guests, lastActivity, itemCount, riskScore, severity, status |
| `ts_remediation` | Requested cleanup for a finding | finding (lookup), action, requestedBy, approver, status, decidedOn |

## Finding status

`New -> Triaged -> RemediationRequested -> Resolved`, plus `Accepted` (risk accepted) from
`Triaged`. The plug-in rejects any other transition.

## Remediation status

`Requested -> Approved | Rejected`; `Approved -> Completed`.

## Scoring

See `src/RiskScoring/RiskScorer.cs`. Weights are illustrative; record every change here.

- 2026-10-17: Score rounds midpoints half-up (MidpointRounding.AwayFromZero). Default banker's rounding scored 3 org links as 4 instead of 5.
