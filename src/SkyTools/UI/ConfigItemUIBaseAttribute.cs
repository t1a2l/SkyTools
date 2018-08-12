// <copyright file="ConfigItemUIBaseAttribute.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace SkyTools.UI
{
    using System;

    /// <summary>
    /// A base class for the attributes that define the configuration items layout on the mod's
    /// configuration page.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class ConfigItemUIBaseAttribute : Attribute
    {
    }
}