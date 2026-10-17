# Architecture

## Flow

1. A timer-triggered Function enumerates sites through Microsoft Graph and gathers sharing
   signals per site.
2. It writes a `ScanRun` and one `Finding` per site to Dataverse.
3. A pre-operation plug-in computes `RiskScore` and `Severity` (shared `RiskScoring` library).
4. A reviewer works the queue in the model-driven or code app and requests a `Remediation`.
5. A flow routes approval; the `ApproveRemediation` custom API applies the status change.
6. An agent answers questions over findings through a custom connector or an MCP server.

## Identity

- Function to Graph: managed identity if an Azure subscription exists; otherwise an app
  registration with a certificate. Least-privilege application permissions only.
- Function to Dataverse: application user for the same identity.
- No secrets in the repo.

## Portability

The design uses the Dataverse connector and a custom connector, which many enterprise tenants
block through DLP. Fallback: the Function calls the Dataverse Web API directly and flows call the
Function through a standard HTTP action. Document which mode a deployment uses.

## Open questions

- Exact Graph permissions per signal.
- How to ship the shared scoring assembly with the plug-in.
- Environment variable and connection reference inventory.
