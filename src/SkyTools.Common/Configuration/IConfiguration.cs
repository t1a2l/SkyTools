// <copyright file="IConfiguration.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace SkyTools.Configuration
{
    /// <summary>
    /// An interface for configuration container objects.
    /// </summary>
    public interface IConfiguration
    {
        /// <summary>Validates this instance and corrects possible invalid property values.</summary>
        void Validate();

        /// <summary>Checks the inner state of the object and migrates it to the latest version when necessary.</summary>
        void MigrateWhenNecessary();
    }
}
