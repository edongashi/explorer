namespace Explorer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Aircloak.JsonApi;
    using Explorer.Api.Models;
    using Explorer.Queries;

    internal class BoolColumnExplorer : ColumnExplorer
    {
        public BoolColumnExplorer(JsonApiClient apiClient, ExploreParams exploreParams)
            : base(apiClient, exploreParams)
        {
            ExploreMetrics = Array.Empty<ExploreResult.Metric>();
        }

        public IEnumerable<ExploreResult.Metric> ExploreMetrics { get; set; }

        public override async Task Explore()
        {
            LatestResult = new ExploreResult(ExplorationGuid, status: Status.Processing);

            var distinctValues = await ResolveQuery<DistinctColumnValues.BoolResult>(
                new DistinctColumnValues(ExploreParams.TableName, ExploreParams.ColumnName),
                timeout: TimeSpan.FromMinutes(2));

            var suppressedValueCount = distinctValues.ResultRows.Sum(row =>
                    row.DistinctData.IsSuppressed ? row.Count : 0);

            var totalValueCount = distinctValues.ResultRows.Sum(row => row.Count);

            // This shouldn't happen, but check anyway.
            if (totalValueCount == 0)
            {
                LatestResult = new ExploreError(
                    ExplorationGuid,
                    $"Cannot explore table/column: value count is zero.");
                return;
            }

            var suppressedValueRatio = (double)suppressedValueCount / totalValueCount;

            var distinctValueCounts =
                from row in distinctValues.ResultRows
                where !row.DistinctData.IsSuppressed
                orderby row.Count descending
                select new
                {
                    row.DistinctData.Value,
                    row.Count,
                };

            ExploreMetrics = ExploreMetrics
                .Append(new ExploreResult.Metric(name: "top_distinct_values", value: distinctValueCounts))
                .Append(new ExploreResult.Metric(name: "total_count", value: totalValueCount))
                .Append(new ExploreResult.Metric(name: "suppressed_values", value: suppressedValueCount));

            LatestResult = new ExploreResult(
                ExplorationGuid,
                status: Status.Complete,
                metrics: ExploreMetrics);

            return;
        }
    }
}
