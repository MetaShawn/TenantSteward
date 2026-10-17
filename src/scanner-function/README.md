# Scanner Function

Azure Function (C#, isolated worker) that enumerates SharePoint sites through Microsoft Graph,
builds `ExposureSignals` per site, and writes `Finding` rows to Dataverse.

## First steps

```
func init . --worker-runtime dotnet-isolated
func new --name ScanSites --template "Timer trigger"
func new --name GetFinding --template "HTTP trigger"
```

Then add a project reference to `../RiskScoring/RiskScoring.csproj`.

## To build here

- Graph access: app registration with certificate, or managed identity once an Azure
  subscription exists. Document the exact least-privilege application permissions in
  `docs/architecture.md` after verifying them against Graph docs.
- Dataverse access: application user for the same identity.
- Handle Graph throttling (429 with Retry-After) and Dataverse service protection limits.
- Expose the HTTP endpoints with an OpenAPI definition for the custom connector.
- Add Application Insights.
