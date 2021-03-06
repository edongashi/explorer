namespace Diffix.Values
{
    /// <summary>
    /// Represents an unsuppressed NULL column value.
    /// </summary>
    /// <typeparam name="T">The expected type of the column value.</typeparam>
    internal sealed class NullValue<T> : DValue<T>
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static readonly DValue<T> Instance = new NullValue<T>();

        private NullValue()
        {
        }

        /// <summary>
        /// Gets a value indicating whether the column value was suppressed.
        /// Always returns false because the column has not been suppressed by Diffix anonymization.
        /// </summary>
        public bool IsSuppressed => false;

        /// <summary>
        /// Gets a value indicating whether the column value was NULL.
        /// Always returns true because the returned value is NULL.
        /// </summary>
        public bool IsNull => true;

        /// <summary>
        /// Gets a value indicating whether the column contained a valid value.
        /// Always returns false since the returned value is NULL.
        /// </summary>
        public bool HasValue => false;

        /// <summary>
        /// Gets the wrapped value.
        /// </summary>
        /// <remarks>
        /// Throws an exception, since accessing the value is an invalid operation.
        /// </remarks>
        public T Value => throw new System.InvalidOperationException("Do not use NullValue.Value.");
    }
}
