using System;
using TenantSteward.Scoring;
using Xunit;

namespace TenantSteward.Scoring.Tests
{
    public class RiskScorerScoreTests
    {
        // --- Baseline ---------------------------------------------------------------

        [Fact]
        public void NoSignals_ScoresZero_AndClassifiesLow()
        {
            int score = RiskScorer.Score(new ExposureSignals());

            Assert.Equal(0, score);
            Assert.Equal(Severity.Low, RiskScorer.Classify(score));
        }

        [Fact]
        public void FiveAnonymousLinksAlone_ScoreSixty_AndClassifyHigh()
        {
            int score = RiskScorer.Score(new ExposureSignals { AnonymousLinkCount = 5 });

            Assert.Equal(60, score);
            Assert.Equal(Severity.High, RiskScorer.Classify(score));
        }

        // --- Per-signal weights and caps ---------------------------------------------

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 12)]
        [InlineData(2, 24)]
        [InlineData(3, 36)]
        [InlineData(4, 48)]
        [InlineData(5, 60)]
        public void EachAnonymousLink_AddsTwelve_UpToTheCap(int links, int expected)
        {
            Assert.Equal(expected, RiskScorer.Score(new ExposureSignals { AnonymousLinkCount = links }));
        }

        [Theory]
        [InlineData(6)]
        [InlineData(50)]
        [InlineData(int.MaxValue)]
        public void AnonymousLinks_AreCappedAtFive(int links)
        {
            Assert.Equal(60, RiskScorer.Score(new ExposureSignals { AnonymousLinkCount = links }));
        }

        [Theory]
        [InlineData(10)]
        [InlineData(11)]
        [InlineData(1000)]
        [InlineData(int.MaxValue)]
        public void OrganizationLinks_AreCappedAtTen_ForFifteenPoints(int links)
        {
            Assert.Equal(15, RiskScorer.Score(new ExposureSignals { OrganizationLinkCount = links }));
        }

        [Theory]
        [InlineData(10)]
        [InlineData(11)]
        [InlineData(1000)]
        [InlineData(int.MaxValue)]
        public void ExternalGuests_AreCappedAtTen_ForFifteenPoints(int guests)
        {
            Assert.Equal(15, RiskScorer.Score(new ExposureSignals { ExternalGuestCount = guests }));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(180, 0)]   // exactly 180 days is not yet stale
        [InlineData(181, 5)]
        [InlineData(int.MaxValue, 5)]
        public void StaleSites_AddFive_OnlyAfter180Days(int days, int expected)
        {
            Assert.Equal(expected, RiskScorer.Score(new ExposureSignals { DaysSinceLastActivity = days }));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1000, 0)]  // exactly 1000 items is not yet "large"
        [InlineData(1001, 5)]
        [InlineData(int.MaxValue, 5)]
        public void LargeSites_AddFive_OnlyAboveOneThousandItems(int items, int expected)
        {
            Assert.Equal(expected, RiskScorer.Score(new ExposureSignals { ItemCount = items }));
        }

        // --- Defensive behavior ------------------------------------------------------

        [Fact]
        public void NegativeInputs_BehaveAsZero()
        {
            var negative = new ExposureSignals
            {
                AnonymousLinkCount = -3,
                OrganizationLinkCount = -1,
                ExternalGuestCount = -5,
                DaysSinceLastActivity = -10,
                ItemCount = -100
            };

            Assert.Equal(0, RiskScorer.Score(negative));
            Assert.Equal(RiskScorer.Score(new ExposureSignals()), RiskScorer.Score(negative));
        }

        [Fact]
        public void MaximumSignals_ScoreExactlyOneHundred_AndClassifyCritical()
        {
            var max = new ExposureSignals
            {
                AnonymousLinkCount = 5,
                OrganizationLinkCount = 10,
                ExternalGuestCount = 10,
                DaysSinceLastActivity = 181,
                ItemCount = 1001
            };

            int score = RiskScorer.Score(max);

            Assert.Equal(100, score);
            Assert.Equal(Severity.Critical, RiskScorer.Classify(score));
        }

        [Fact]
        public void ExtremeValues_NeverExceedOneHundred_OrOverflow()
        {
            var extreme = new ExposureSignals
            {
                AnonymousLinkCount = int.MaxValue,
                OrganizationLinkCount = int.MaxValue,
                ExternalGuestCount = int.MaxValue,
                DaysSinceLastActivity = int.MaxValue,
                ItemCount = int.MaxValue
            };

            Assert.Equal(100, RiskScorer.Score(extreme));
        }

        [Fact]
        public void NullSignals_ThrowArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => RiskScorer.Score(null));
            Assert.Equal("signals", ex.ParamName);
        }

        // --- Determinism and rounding ------------------------------------------------

        [Fact]
        public void Score_IsDeterministic_AndDoesNotMutateInput()
        {
            var signals = new ExposureSignals
            {
                AnonymousLinkCount = 2,
                OrganizationLinkCount = 3,
                ExternalGuestCount = 4,
                DaysSinceLastActivity = 200,
                ItemCount = 5000
            };

            int first = RiskScorer.Score(signals);
            int second = RiskScorer.Score(signals);

            Assert.Equal(first, second);
            Assert.Equal(2, signals.AnonymousLinkCount);
            Assert.Equal(3, signals.OrganizationLinkCount);
            Assert.Equal(4, signals.ExternalGuestCount);
            Assert.Equal(200, signals.DaysSinceLastActivity);
            Assert.Equal(5000, signals.ItemCount);
        }

        // Org and guest links are worth 1.5 points each, so odd counts land on .5.
        // Midpoints must round up consistently (1.5 -> 2, 4.5 -> 5, 10.5 -> 11),
        // not to the nearest even number, or two near-identical sites score unevenly.
        [Theory]
        [InlineData(1, 2)]    // 1.5  -> 2
        [InlineData(2, 3)]    // 3.0  -> 3
        [InlineData(3, 5)]    // 4.5  -> 5
        [InlineData(4, 6)]    // 6.0  -> 6
        [InlineData(5, 8)]    // 7.5  -> 8
        [InlineData(6, 9)]    // 9.0  -> 9
        [InlineData(7, 11)]   // 10.5 -> 11
        public void FractionalWeights_RoundHalfUp(int orgLinks, int expected)
        {
            Assert.Equal(expected, RiskScorer.Score(new ExposureSignals { OrganizationLinkCount = orgLinks }));
        }

        [Fact]
        public void MoreOrganizationLinks_NeverLowerTheScore()
        {
            int previous = -1;
            for (int links = 0; links <= 12; links++)
            {
                int score = RiskScorer.Score(new ExposureSignals { OrganizationLinkCount = links });
                Assert.True(score >= previous, "Score dropped at " + links + " links");
                previous = score;
            }
        }

        // --- End-to-end boundaries through Score -------------------------------------

        [Fact]
        public void ScoreOf24_IsLow()
        {
            int score = RiskScorer.Score(new ExposureSignals { AnonymousLinkCount = 2 });

            Assert.Equal(24, score);
            Assert.Equal(Severity.Low, RiskScorer.Classify(score));
        }

        [Fact]
        public void ScoreOf25_IsMedium()
        {
            // 12 (1 anonymous) + 1.5 + 1.5 + 5 (stale) + 5 (large) = 25
            int score = RiskScorer.Score(new ExposureSignals
            {
                AnonymousLinkCount = 1,
                OrganizationLinkCount = 1,
                ExternalGuestCount = 1,
                DaysSinceLastActivity = 181,
                ItemCount = 1001
            });

            Assert.Equal(25, score);
            Assert.Equal(Severity.Medium, RiskScorer.Classify(score));
        }
    }

    public class RiskScorerClassifyTests
    {
        [Theory]
        [InlineData(0, Severity.Low)]
        [InlineData(24, Severity.Low)]
        [InlineData(25, Severity.Medium)]
        [InlineData(49, Severity.Medium)]
        [InlineData(50, Severity.High)]
        [InlineData(74, Severity.High)]
        [InlineData(75, Severity.Critical)]
        [InlineData(100, Severity.Critical)]
        public void Boundaries_ClassifyIntoExpectedSeverity(int score, Severity expected)
        {
            Assert.Equal(expected, RiskScorer.Classify(score));
        }

        [Theory]
        [InlineData(-1, Severity.Low)]
        [InlineData(int.MinValue, Severity.Low)]
        [InlineData(101, Severity.Critical)]
        [InlineData(int.MaxValue, Severity.Critical)]
        public void OutOfRangeScores_ClampToTheNearestSeverity(int score, Severity expected)
        {
            Assert.Equal(expected, RiskScorer.Classify(score));
        }
    }
}
