﻿namespace Explorer.Common
{
    using System.Text.Json;

    using Diffix;
    using Explorer.JsonExtensions;

    internal class IndexedGroupingSetsResult<TIndex, TGroupedValue> : ValueWithCount<TGroupedValue>
    {
        internal IndexedGroupingSetsResult(
            int id,
            TIndex[] labels,
            DValue<TGroupedValue> value,
            long count,
            double? countNoise)
            : base(value, count, countNoise)
        {
            GroupingId = id;
            GroupingLabels = labels;
        }

        internal IndexedGroupingSetsResult(ref Utf8JsonReader reader, TIndex[] groupingLabels)
        {
            (GroupingId, GroupingValue) = reader.ParseGroupingSet<TGroupedValue>(groupingLabels.Length);
            Count = reader.ParseCount();
            CountNoise = reader.ParseNoise();
            GroupingLabels = groupingLabels;
        }

        public int GroupingId { get; }

        public TIndex[] GroupingLabels { get; }

        public DValue<TGroupedValue> GroupingValue
        {
            get => DValue;
            set => DValue = value;
        }

        public TIndex GroupingLabel => GroupingLabels[GroupingIndex];

        public int GroupSize => GroupingLabels.Length;

        public int GroupingIndex =>
            GroupingIdConverter.GetConverter(GroupSize).SingleIndexFromGroupingId(GroupingId);

        public static IndexedGroupingSetsResult<TIndex, TGroupedValue> Create(
            ref Utf8JsonReader reader,
            TIndex[] groupingLabels)
        {
            var (groupingId, groupingValue) = reader.ParseGroupingSet<TGroupedValue>(groupingLabels.Length);
            var count = reader.ParseCount();
            var countNoise = reader.ParseNoise();
            return new IndexedGroupingSetsResult<TIndex, TGroupedValue>(
                groupingId, groupingLabels, groupingValue, count, countNoise);
        }
    }
}