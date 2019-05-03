// <copyright file="ConfigurationProvider.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace SkyTools.Configuration
{
    using System;
    using System.IO;
    using System.Xml.Serialization;
    using SkyTools.Storage;
    using SkyTools.Tools;

    /// <summary>
    /// A static class that loads and stores the configuration objects of type <typeparamref name="T"/>.
    /// An XML file in the default (game) directory is used as the default configuration storage.
    /// Using the <see cref="IStorageData"/> interface, this object can store and read the configuration
    /// to/from the save games.
    /// </summary>
    /// <typeparam name="T">The type of the configuration container object. This has to be a class that implements
    /// the <see cref="IConfiguration"/> interface and provides a public parameterless constructor.</typeparam>
    public sealed class ConfigurationProvider<T> : IStorageData
        where T : class, IConfiguration, new()
    {
        private readonly string storageId;
        private readonly string modName;
        private readonly string defaultsFileName;
        private readonly Func<T> configurationFactory;

        /// <summary>Initializes a new instance of the <see cref="ConfigurationProvider{T}"/> class.</summary>
        /// <param name="storageId">The storage ID to use for storing the configuration data in the game files.</param>
        /// <param name="modName">The name of the mod that will also be used as the default configuration file name.</param>
        /// <param name="configurationFactory">A method that creates new instances of the <typeparamref name="T"/> objects.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        public ConfigurationProvider(string storageId, string modName, Func<T> configurationFactory)
        {
            this.storageId = storageId ?? throw new ArgumentNullException(nameof(storageId));
            this.modName = modName ?? throw new ArgumentNullException(nameof(modName));
            this.configurationFactory = configurationFactory ?? throw new ArgumentNullException(nameof(configurationFactory));

            if (storageId.Length == 0)
            {
                throw new ArgumentException("The storage ID cannot be an empty string", nameof(storageId));
            }

            if (modName.Length == 0)
            {
                throw new ArgumentException("The mod name cannot be an empty string", nameof(modName));
            }

            defaultsFileName = modName + ".xml";
        }

        /// <summary>Occurs when the <see cref="Configuration"/> instance changes.</summary>
        public event EventHandler Changed;

        /// <summary>Gets the current configuration.</summary>
        public T Configuration { get; private set; }

        /// <summary>Gets a value indicating whether the <see cref="Configuration"/> instance is a default configuration.</summary>
        public bool IsDefault { get; private set; }

        /// <summary>Gets an unique ID of this storage data set.</summary>
        string IStorageData.StorageDataId => storageId;

        /// <summary>
        /// Loads the default configuration object from the serialized storage. If no storage is available,
        /// returns a new <typeparamref name="T"/> object that will be created using the object factory
        /// specified when creating this <see cref="ConfigurationProvider{T}"/> instance.
        /// </summary>
        public void LoadDefaultConfiguration()
        {
            try
            {
                if (File.Exists(defaultsFileName))
                {
                    using (var stream = new FileStream(defaultsFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        Configuration = Deserialize(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"The '{modName}' mod cannot load its default configuration, error message: {ex}");
            }

            IsDefault = true;
            if (Configuration == null)
            {
                Configuration = configurationFactory();
            }

            OnChanged();
        }

        /// <summary>
        /// Stores the current configuration object to the storage as a default configuration.
        /// </summary>
        public void SaveDefaultConfiguration()
        {
            if (Configuration == null)
            {
                Configuration = configurationFactory();
            }

            try
            {
                using (var stream = new FileStream(defaultsFileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    Serialize(Configuration, stream);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"The '{modName}' mod cannot save its default configuration, error message: {ex}");
            }
        }

        /// <summary>Reads the data set from the specified <see cref="Stream" />.</summary>
        /// <param name="source">A <see cref="Stream" /> to read the data set from.</param>
        void IStorageData.ReadData(Stream source)
        {
            Configuration = Deserialize(source);
            if (Configuration == null)
            {
                LoadDefaultConfiguration();
            }
            else
            {
                OnChanged();
            }

            IsDefault = false;
        }

        /// <summary>Stores the data set to the specified <see cref="Stream" />.</summary>
        /// <param name="target">A <see cref="Stream" /> to write the data set to.</param>
        void IStorageData.StoreData(Stream target)
        {
            if (Configuration != null)
            {
                Serialize(Configuration, target);
            }
        }

        private T Deserialize(Stream source)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using (var sr = new StreamReader(source))
                {
                    var result = (T)serializer.Deserialize(sr);
                    result.MigrateWhenNecessary();
                    result.Validate();
                    return result;
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"The '{modName}' mod has encountered an error while trying to load the configuration, error message: {ex}");
                return null;
            }
        }

        private void Serialize(T config, Stream target)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T), SerializationTools.IgnoreObsoleteProperties(config));
                using (var sw = new StreamWriter(target))
                {
                    serializer.Serialize(sw, config);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"The '{modName}' mod cannot save its configuration, error message: {ex}");
            }
        }

        private void OnChanged() => Changed?.Invoke(this, EventArgs.Empty);
    }
}
