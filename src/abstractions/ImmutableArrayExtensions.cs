namespace System.Collections.Immutable
{
    public static class ImmutableArrayExtensions
    {
        public static ImmutableArray<T> ToImmutableArray<T>(this ReadOnlySpan<T> span)
        {
            var array = ImmutableArray.CreateBuilder<T>(span.Length);

            foreach (var value in span)
            {
                array.Add(value);
            }

            return array.MoveToImmutable();
        }
    }
}