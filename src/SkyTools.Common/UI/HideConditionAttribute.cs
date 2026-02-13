// <copyright file="HideConditionAttribute.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace SkyTools.UI
{
    using System;

    /// <summary>
    /// A base class for the attributes that define if the configuration item should be hidden on the
    /// configuration page.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class HideConditionAttribute : Attribute
    {
        /// <summary>When implemented in derived classes, check if the configuration item is hidden or not.</summary>
        /// <returns>bool.</returns>
        public abstract bool IsHidden();
    }
}
