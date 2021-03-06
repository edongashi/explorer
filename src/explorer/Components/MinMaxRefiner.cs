namespace Explorer.Components
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using Diffix;
    using Explorer.Common;
    using Explorer.Metrics;
    using Explorer.Queries;

    public class MinMaxRefiner : ExplorerComponent<MinMaxRefiner.Result>, PublisherComponent
    {
        private const int MaxIterations = 10;
        private readonly DConnection conn;
        private readonly ExplorerContext ctx;

        public MinMaxRefiner(DConnection conn, ExplorerContext ctx)
        {
            this.ctx = ctx;
            this.conn = conn;
        }

        protected override async Task<Result> Explore()
        {
            return new Result(await RefinedMinEstimate(), await RefinedMaxEstimate());
        }

        private async Task<decimal> RefinedMinEstimate()
        {
            // initial unconstrained min or max
            var result = await GetMinEstimate(null);

            // limit the number of iterations
            for (var i = 0; i < MaxIterations; i++)
            {
                // If we have a zero result, it can't be improved upon anyway.
                if (result == decimal.Zero)
                {
                    break;
                }

                // Constrained query to get an improved estimate
                var estimate = await GetMinEstimate(result);

                // If there are no longer enough values in the constrained range to compute an anonymised min/max,
                // the query will return `null` => we can't improve further on the result.
                // Same thing if the results start to diverge (second part of if condition).
                if ((!estimate.HasValue) || (estimate >= result))
                {
                    break;
                }
                result = estimate;
            }

            Debug.Assert(result.HasValue, "Unexpected null result when refining Min estimate.");

            return result.Value;
        }

        private async Task<decimal> RefinedMaxEstimate()
        {
            // initial unconstrained min or max
            var result = await GetMaxEstimate(null);

            // limit the number of iterations
            for (var i = 0; i < MaxIterations; i++)
            {
                // Constrained query to get an improved estimate
                var estimate = await GetMaxEstimate(result);

                // If there are no longer enough values in the constrained range to compute an anonymised min/max,
                // the query will return `null` => we can't improve further on the result.
                // Same thing if the results start to diverge (second part of if condition).
                if ((!estimate.HasValue) || (estimate <= result))
                {
                    break;
                }
                result = estimate;
            }

            Debug.Assert(result.HasValue, "Unexpected null result when refining Max estimate.");

            return result.Value;
        }

        private async Task<decimal?> GetMinEstimate(decimal? upperBound)
        {
            var minQ = await conn.Exec<Min.Result<decimal>>(
                new Min(ctx.Table, ctx.Column, upperBound));
            return minQ.Rows.Single().Min;
        }

        private async Task<decimal?> GetMaxEstimate(decimal? lowerBound)
        {
            var maxQ = await conn.Exec<Max.Result<decimal>>(
                new Max(ctx.Table, ctx.Column, lowerBound));
            return maxQ.Rows.Single().Max;
        }

        public async IAsyncEnumerable<ExploreMetric> YieldMetrics()
        {
            var result = await ResultAsync;

            yield return new UntypedMetric("refined_max", result.Max);
            yield return new UntypedMetric("refined_min", result.Min);
        }

        public class Result
        {
            public Result(decimal min, decimal max)
            {
                Max = max;
                Min = min;
            }

            public decimal Min { get; }

            public decimal Max { get; }
        }
    }
}