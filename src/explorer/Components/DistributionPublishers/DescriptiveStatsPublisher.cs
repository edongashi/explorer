namespace Explorer.Components
{
    using System.Collections.Generic;

    using Explorer.Metrics;

    public class DescriptiveStatsPublisher : PublisherComponent
    {
        private readonly ResultProvider<NumericDistribution> distributionProvider;

        public DescriptiveStatsPublisher(ResultProvider<NumericDistribution> distributionProvider)
        {
            this.distributionProvider = distributionProvider;
        }

        public async IAsyncEnumerable<ExploreMetric> YieldMetrics()
        {
            var distribution = await distributionProvider.ResultAsync;

            yield return new UntypedMetric(
                name: "descriptive_stats",
                metric: distribution);
        }
    }
}