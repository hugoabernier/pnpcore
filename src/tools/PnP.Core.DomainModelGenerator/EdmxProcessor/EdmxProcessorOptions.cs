﻿using System.Collections.Generic;

namespace PnP.M365.DomainModelGenerator
{
    /// <summary>
    /// Defines a collection of credential settings for secure connections to the target resources
    /// </summary>
    public class EdmxProcessorOptions
    {
        /// <summary>
        /// Defines the path of the JSON mapping file
        /// </summary>
        public string MappingFilePath { get; set; }

        /// <summary>
        /// Defines the settings for the EDMX metadata providers
        /// </summary>
        public List<EdmxProviderOptions> EdmxProviders { get; set; } = new List<EdmxProviderOptions>();
    }
}
