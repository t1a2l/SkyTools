// <copyright file="IContainerViewItem.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace SkyTools.UI
{
    using ICities;

    /// <summary>An interface for a view item that acts like a container.</summary>
    /// <seealso cref="IViewItem"/>
    public interface IContainerViewItem : IViewItem
    {
        /// <summary>Gets this object's container game object.</summary>
        UIHelperBase Container { get; }
    }
}