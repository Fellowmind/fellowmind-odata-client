using System;
using System.Collections.Generic;
using System.Linq;

namespace Fellowmind.OData.Client.Core
{
    /// <summary>
    /// Contains extension methods for <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Batch the given enumerable to smaller enumerables of batchSize.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The enumerable to batch.</param>
        /// <param name="batchSize">Batch size.</param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> items, int batchSize)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            List<T> list = items.ToList();
            for (int i = 0; i < list.Count; i += batchSize)
            {
                yield return list.GetRange(i, Math.Min(batchSize, list.Count - i));
            }
        }
    }
}
