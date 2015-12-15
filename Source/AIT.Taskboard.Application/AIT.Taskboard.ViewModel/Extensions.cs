using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIT.Taskboard.ViewModel
{
    public static class Extensions
    {
        public static IEnumerable<T> FlattenHierarchy<T>(this T node, Func<T, IEnumerable<T>> getChildEnumerator)
        {
            yield return node;
            if (getChildEnumerator(node) != null)
            {
                foreach (var child in getChildEnumerator(node))
                {
                    foreach (var childOrDescendant in child.FlattenHierarchy(getChildEnumerator))
                    {
                        yield return childOrDescendant;
                    }
                }
            }
        }
    }
}
