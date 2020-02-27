namespace Explorer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal class ColumnExplorer
    {
        private readonly List<Task> childTasks;

        private readonly List<ExplorerImpl> childExplorers;

        public ColumnExplorer(IEnumerable<ExplorerImpl> explorerImpls)
        {
            ExplorationGuid = Guid.NewGuid();

            childTasks = new List<Task>();
            childExplorers = new List<ExplorerImpl>();

            foreach (var impl in explorerImpls)
            {
                Spawn(impl);
            }

            Completion = Task.WhenAll(childTasks);
        }

        public Guid ExplorationGuid { get; }

        public IEnumerable<IExploreMetric> ExploreMetrics =>
            childExplorers.SelectMany(explorer => explorer.Metrics);

        public Task Completion { get; }

        public TaskStatus Status => Completion.Status;

        private void Spawn(ExplorerImpl explorerImpl)
        {
            var exploreTask = Task.Run(explorerImpl.Explore);
            childExplorers.Add(explorerImpl);
            childTasks.Add(exploreTask);
        }
    }
}