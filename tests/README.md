# Tests

## RiskScoring (write first)

Create an xUnit project here referencing `src/RiskScoring`. Cases to cover:

- No signals scores 0 and classifies Low.
- Five or more anonymous links alone score 60 and classify High.
- Anonymous links are capped at 5; more links do not raise the score.
- Negative inputs behave as zero.
- Maximum signals score exactly 100 and classify Critical.
- Boundary scores 24/25, 49/50, 74/75 classify into the expected severity.
- Null signals throw `ArgumentNullException`.

## Plug-ins

Test the logic classes that take plain inputs. Keep `IServiceProvider`-dependent glue thin.
