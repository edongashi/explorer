namespace Explorer.Components
{
    using System.Threading.Tasks;

    public abstract class ExplorerComponent<TResult> : ResultProvider<TResult>
    {
        private Task<TResult>? componentTask;

        public Task<TResult> ResultAsync
        {
            get => componentTask ??= Task.Run(async () => await Explore());
        }

        protected abstract Task<TResult> Explore();
    }
}