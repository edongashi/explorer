namespace Explorer.Common
{
    using System.Threading.Tasks;

    public static class Utilities
    {
        internal static async Task<(T1, T2, T3)> WhenAll<T1, T2, T3>(Task<T1> task1, Task<T2> task2, Task<T3> task3)
        {
            await Task.WhenAll(task1, task2, task3);

            return (task1.Result, task2.Result, task3.Result);
        }
    }
}