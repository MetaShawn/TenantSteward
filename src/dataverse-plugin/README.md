# Dataverse plug-in and custom API

Responsibilities:

- Pre-operation plug-in on `Finding`: compute `RiskScore` and `Severity` using `RiskScoring`.
- Plug-in enforcing `Remediation` status transitions (see `docs/data-model.md`).
- Custom API `ts_ApproveRemediation` implemented by a plug-in.

## First steps

Check `pac plugin --help` for the current scaffolding command, create the project here, and
confirm in Microsoft Learn how to ship the shared `RiskScoring` assembly with a plug-in package.
Register with the Plug-in Registration Tool or the pac CLI.

Keep business logic in testable classes that take simple inputs, and keep the plug-in class
itself as thin glue over `IPluginExecutionContext` and the organization service.
