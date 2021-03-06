namespace Explorer.Components
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Diffix;
    using Explorer.Common;
    using Explorer.Metrics;
    using Explorer.Queries;

    public class SimpleStats<T> : ExplorerComponent<SimpleStats<T>.Result>, PublisherComponent
    {
        private readonly DConnection conn;

        private readonly ExplorerContext ctx;

        public SimpleStats(DConnection conn, ExplorerContext ctx)
        {
            this.conn = conn;
            this.ctx = ctx;
        }

        public async IAsyncEnumerable<ExploreMetric> YieldMetrics()
        {
            var result = await ResultAsync;

            yield return new UntypedMetric("count", result.Count);
            yield return new UntypedMetric("naive_min", result.Min!);
            yield return new UntypedMetric("naive_max", result.Max!);
        }

        protected override async Task<SimpleStats<T>.Result> Explore()
        {
            var statsQ = await conn.Exec(new BasicColumnStats<T>(ctx.Table, ctx.Column));

            return new Result(statsQ.Rows.Single());
        }

        public class Result
        {
            internal Result(BasicColumnStats<T>.Result stats)
            {
                Stats = stats;
            }

            public BasicColumnStats<T>.Result Stats { get; }

            public long Count { get => Stats.Count; }

            public T Min { get => Stats.Min; }

            public T Max { get => Stats.Max; }
        }
    }
}