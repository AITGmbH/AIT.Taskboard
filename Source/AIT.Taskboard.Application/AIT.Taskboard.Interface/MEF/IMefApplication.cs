using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;

namespace AIT.Taskboard.Interface.MEF
{
    /// <summary></summary>
    public interface IMefApplication
    {
        /// <summary>
        ///   Called when [add catalog].
        /// </summary>
        /// <param name="catalogs">The catalogs.</param>
        void OnAddCatalog(ICollection<ComposablePartCatalog> catalogs);
    }
}