// <copyright file="ConfigItemComboBoxAttribute.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace SkyTools.UI
{
    using System;

    /// <summary>
    /// An attribute specifying that the configuration item has to be presented as a combo box.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ConfigItemComboBoxAttribute : ConfigItemUIBaseAttribute
    {
    }
}