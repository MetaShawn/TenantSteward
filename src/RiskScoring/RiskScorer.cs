using System;

namespace TenantSteward.Scoring
{
    public enum Severity
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }

    /// <summary>Exposure facts gathered for one site (or other scope) during a scan.</summary>
    public sealed class ExposureSignals
    {
        public int AnonymousLinkCount { get; set; }
        public int OrganizationLinkCount { get; set; }
        public int ExternalGuestCount { get; set; }
        public int DaysSinceLastActivity { get; set; }
        public int ItemCount { get; set; }
    }

    /// <summary>
    /// Pure, dependency-free scoring so the same logic can run in the Function and in a
    /// Dataverse plug-in, and be unit tested without either. The weights are illustrative;
    /// tune them against real findings and record changes in docs/data-model.md.
    /// </summary>
    public static class RiskScorer
    {
        public static int Score(ExposureSignals signals)
        {
            if (signals == null) throw new ArgumentNullException(nameof(signals));

            double score = 0;
            score += Math.Min(Math.Max(signals.AnonymousLinkCount, 0), 5) * 12;       // up to 60
            score += Math.Min(Math.Max(signals.OrganizationLinkCount, 0), 10) * 1.5;  // up to 15
            score += Math.Min(Math.Max(signals.ExternalGuestCount, 0), 10) * 1.5;     // up to 15
            if (signals.DaysSinceLastActivity > 180) score += 5;                      // stale
            if (signals.ItemCount > 1000) score += 5;                                 // large blast radius

            // AwayFromZero: the default (ToEven) rounds 4.5 down to 4 but 1.5 up to 2, which made
            // odd link counts score unevenly. Half-up is predictable and easy to explain.
            return (int)Math.Min(100, Math.Round(score, MidpointRounding.AwayFromZero));
        }

        public static Severity Classify(int score)
        {
            if (score >= 75) return Severity.Critical;
            if (score >= 50) return Severity.High;
            if (score >= 25) return Severity.Medium;
            return Severity.Low;
        }
    }
}
