# TenantSteward: Chapter Guide

A build-along guide. Each chapter produces something committed to this repo, and together they cover the AB-400 (Power Platform Developer Associate) skills-measured areas. Work them in order; later chapters depend on earlier ones.

**Rules of the road**
- One chapter = one branch or one tagged commit (`ch03-plugin-done`). The history becomes your portfolio story.
- Every chapter ends with a "Done when" check. If you can't tick it, the chapter isn't done.
- Use AI to move faster, but be able to explain every line you commit. Write tests for plug-ins and Functions.
- The AB-400 study guide is the source of truth for exam scope: https://aka.ms/AB400-StudyGuide
- Items marked **Verify** are open questions; check them against current Microsoft docs before relying on them.

**Progress**

| # | Chapter | Status |
|---|---|---|
| 0 | Repo setup | [X] |
| 1 | RiskScoring library and tests | [X] |
| 2 | Dataverse data model | [ ] |
| 3 | Plug-in and custom API | [ ] |
| 4 | Scanner Azure Function | [ ] |
| 5 | Custom connector | [ ] |
| 6 | PCF control | [ ] |
| 7 | Code app | [ ] |
| 8 | Approval flow | [ ] |
| 9 | MCP server and Copilot Studio agent | [ ] |
| 10 | ALM and CI/CD | [ ] |
| 11 | Polish and exam readiness | [ ] |

---

## Chapter 0: Repo setup
**AB-400 area:** none (foundation)
**Goal:** A clean public repo you're comfortable showing people.
**Steps**
1. Replace "YOUR NAME" in `LICENSE`.
2. Run `git init` from the repo folder (outside any synced-folder lock issues, if OneDrive misbehaves), commit, and push to a new GitHub repo.
3. Skim `docs/architecture.md` and the milestone file so you know the map.
**Done when:** The repo is on GitHub, and a fresh clone builds nothing yet but looks tidy.

## Chapter 1: RiskScoring library and tests
**AB-400 area:** groundwork for plug-in development (C#, testing)
**Goal:** A tested scoring library that the plug-in and Function will share.
**Steps**
1. Install the .NET SDK (the scaffold has never been compiled) and build the solution.
2. Implement the test cases listed in `tests/README.md` first, then make them pass.
3. Review the scoring logic as if a junior wrote it: edge cases, null handling, determinism.
**Done when:** `dotnet test` is green and you can explain each scoring rule.

## Chapter 2: Dataverse data model
**AB-400 area:** Build Power Platform solutions
**Goal:** Tables for findings, sites, reviews, and remediation actions, inside a solution.
**Steps**
1. Start from the draft data model in `docs/`.
2. Create a publisher and an unmanaged solution in your dev environment.
3. Build the tables, relationships, choice columns, and security roles.
4. Export and unpack the solution into `solution/` with the Power Platform CLI.
**Done when:** The unpacked solution is committed and re-imports cleanly into a second environment.

## Chapter 3: Plug-in and custom API
**AB-400 area:** Extend Microsoft Power Platform (largest exam weight)
**Goal:** Server-side logic that computes risk scores and enforces status transitions.
**Steps**
1. Write a plug-in on the findings table (pre-operation validation for transitions, post-operation for scoring).
2. Use pre/post images where you need prior values.
3. Add a custom API (for example, "Recalculate risk for site") backed by a plug-in.
4. Register with the Plug-in Registration Tool or pac CLI; step through with the trace log.
5. Unit-test the logic separately from the pipeline.
**Verify:** how to ship the shared RiskScoring assembly with a plug-in package (dependent assemblies vs. ILMerge-style approaches).
**Done when:** An invalid status transition is rejected, a score updates on save, and the custom API is callable from the Web API.

## Chapter 4: Scanner Azure Function
**AB-400 area:** Extend platform: Azure Functions, Web API/SDK, OAuth, API limits
**Goal:** A C# isolated-worker Function that scans SharePoint sharing via Graph and writes findings to Dataverse.
**Steps**
1. Scaffold with `func init` (isolated worker model).
2. Authenticate with managed identity (or a dev app registration locally); least-privilege Graph permissions.
3. Scan for anonymous links, broad-group access, and stale external users.
4. Write to Dataverse through the Web API or ServiceClient, with retry and 429/Retry-After handling.
5. Add Application Insights logging.
**Verify:** the exact minimum Graph permissions for each scan type.
**Done when:** A run against a dev tenant creates findings in Dataverse, and throttling is handled rather than crashing.

## Chapter 5: Custom connector
**AB-400 area:** Develop integrations
**Goal:** Expose the Function (or a custom API) to flows and agents.
**Steps**
1. Write an OpenAPI definition for the Function endpoints.
2. Create the custom connector with your chosen auth.
3. Test every action in the connector test tab and from a flow.
**Done when:** A flow calls the connector and gets a typed response.

## Chapter 6: PCF control
**AB-400 area:** Extend the user experience
**Goal:** A reusable component, such as a risk-score gauge or findings heatmap, on a model-driven form.
**Steps**
1. `pac pcf init` (TypeScript); learn the lifecycle (init, updateView, getOutputs, destroy).
2. Bind to a Dataverse column and handle read-only and disabled states.
3. Package into the solution and add it to a form.
**Done when:** The control renders live data in the app and ships in the solution.

## Chapter 7: Code app
**AB-400 area:** Extend the user experience (code apps are new to the exam)
**Goal:** A React/TypeScript reviewer front end reading Dataverse and triggering a flow.
**Steps**
1. Follow the code apps overview and prerequisites; note licensing and limits.
2. Connect to Dataverse, list findings, and show detail.
3. Trigger remediation through a flow or connector.
**Done when:** You can triage a finding from the code app end to end.

## Chapter 8: Approval flow
**AB-400 area:** Extend Microsoft Power Platform (flows)
**Goal:** A governed remediation flow: request, approve, apply, record.
**Steps**
1. Trigger on status change in Dataverse.
2. Run an approval, branch on the outcome, call the connector to remediate, and write the result back.
3. Add error handling and run-after paths.
**Done when:** An approved finding gets remediated and logged, and a rejected one is closed with a reason.

## Chapter 9: MCP server and Copilot Studio agent
**AB-400 area:** Extend platform: agents and MCP (Foundry/Copilot Studio)
**Goal:** An agent that answers "why is this site risky and what should we do?"
**Steps**
1. Build the MCP server stub in `mcp-server/` exposing read tools over findings.
2. Connect it to a Copilot Studio agent (or Foundry agent) as a tool.
3. Add grounding instructions and test with realistic questions.
**Note:** Copilot Studio trial expiry and licensing may constrain how long a live demo can run; capture a recording.
**Done when:** The agent answers from real findings and cites which ones.

## Chapter 10: ALM and CI/CD
**AB-400 area:** Build Power Platform solutions (ALM, Pipelines, Build Tools)
**Goal:** The whole solution moves dev to test automatically.
**Steps**
1. Finish the GitHub Actions workflow draft (export, unpack, pack, import).
2. Add the Function build and deploy.
3. Add environment variables and connection references; try managed vs. unmanaged.
**Verify:** action input names against the `powerplatform-actions` README (the draft was written from memory). Pipelines in Power Platform may require licensing; confirm for your tenant.
**Done when:** A push produces a deployable managed solution without manual steps.

## Chapter 11: Polish and exam readiness
**AB-400 area:** all
**Goal:** Make it presentable, and find your remaining gaps.
**Steps**
1. Write the README story: problem, architecture diagram, screenshots, how to run it.
2. Record a short demo.
3. Take the free AB-400 practice assessment; map misses back to chapters and redo the weak ones.
4. Schedule the exam.
**Done when:** A stranger could understand and run the project from the README, and your practice score tells you whether to book the exam.
