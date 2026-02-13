// <copyright file="HideInGameOrEditorConditionAttribute.cs" company="dymanoid">
//     Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace SkyTools.UI
{
    using System;

    /// <summary>
    /// An attribute specifying if the configuration item should be hidden when the game is running or the editor is active.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class HideInGameOrEditorConditionAttribute : HideConditionAttribute
    {
        /// <summary>check if the configuration item is hidden or not.</summary>
        /// <returns>bool.</returns>
        public override bool IsHidden()
        {
            return SimulationManager.exists && SimulationManager.instance.m_metaData != null && SimulationManager.instance.m_metaData.m_updateMode != SimulationManager.UpdateMode.Undefined;
        }
    }
}
