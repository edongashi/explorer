﻿namespace Diffix
{
    using System.Text.Json;

    /// <summary>
    /// Interface for Aircloak query submission and parsing.
    /// </summary>
    /// <typeparam name="TRow">A type representing a result row of the query.</typeparam>
    public interface DQuery<TRow>
    {
        /// <summary>
        /// Gets the query statement that will generate rows that can be read into instances of <c>TRow</c>.
        /// </summary>
        /// <value>The query string to submit.</value>
        public string QueryStatement { get; }

        /// <summary>
        /// Parses a row instance.
        /// </summary>
        /// <param name="reader">The <see cref="Utf8JsonReader"/> instance to use for parsing the result.</param>
        /// <returns>The parsed value.</returns>
        TRow ParseRow(ref Utf8JsonReader reader);
    }
}
