// <copyright file="StorageBase.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace SkyTools.Storage
{
    using System;
    using System.IO;
    using ICities;
    using SkyTools.Tools;

    /// <summary>
    /// A base class for the game component that helps to load and store the mod's private data in a save game.
    /// </summary>
    public abstract class StorageBase : SerializableDataExtensionBase
    {
        private readonly string storageDataPrefix;

        /// <summary>Initializes a new instance of the <see cref="StorageBase"/> class.</summary>
        protected StorageBase()
        {
            storageDataPrefix = GetType().Assembly.GetName().Name + ".";
        }

        /// <summary>
        /// Occurs when the current game level is about to be saved to a save game.
        /// </summary>
        public event EventHandler GameSaving;

        /// <summary>
        /// Gets an instance of the <see cref="StorageBase"/> class that is used with the current game level.
        /// This is not a singleton instance, and is allowed to be null.
        /// </summary>
        public static StorageBase CurrentLevelStorage { get; private set; }

        /// <summary>
        /// Called when the level is being saved.
        /// </summary>
        public override void OnSaveData() => GameSaving?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Called when an instance of this class is being initialized by the game engine.
        /// </summary>
        ///
        /// <param name="serializableData">An instance of the <see cref="ISerializableData"/> service.</param>
        public override void OnCreated(ISerializableData serializableData)
        {
            base.OnCreated(serializableData);
            CurrentLevelStorage = this;
        }

        /// <summary>
        /// Called when this game object is released by the game engine.
        /// </summary>
        public override void OnReleased()
        {
            base.OnReleased();
            CurrentLevelStorage = null;
        }

        /// <summary>
        /// Serializes the data described by the specified <paramref name="data"/> to this level's storage.
        /// </summary>
        ///
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        ///
        /// <param name="data">An <see cref="IStorageData"/> instance that describes the data to serialize.</param>
        public void Serialize(IStorageData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            string dataKey = storageDataPrefix + data.StorageDataId;

            try
            {
                using (var stream = new MemoryStream())
                {
                    data.StoreData(stream);
                    serializableDataManager.SaveData(dataKey, stream.ToArray());
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to save the mod's data (key {dataKey}), error message: {ex}");
            }
        }

        /// <summary>
        /// Deserializes the data described by the specified <paramref name="data"/> from this level's storage.
        /// </summary>
        ///
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        ///
        /// <param name="data">An <see cref="IStorageData"/> instance that describes the data to deserialize.</param>
        public void Deserialize(IStorageData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            string dataKey = storageDataPrefix + data.StorageDataId;

            try
            {
                byte[] rawData = serializableDataManager.LoadData(dataKey);
                if (rawData == null)
                {
                    return;
                }

                using (var stream = new MemoryStream(rawData))
                {
                    data.ReadData(stream);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load the mod's data (key {dataKey}), error message: {ex}");
            }
        }
    }
}
