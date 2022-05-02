using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace kitchenview.Helper.Extensions
{
    public static class ObservableCollectionExtensions
    {
        public static int ItemAt<T>(this ObservableCollection<T> collection, T item, IEqualityComparer<T> comparer)
        {
            for (var i = 0; i < collection.Count; i++)
            {
                if (comparer.Equals(collection[i], item))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}