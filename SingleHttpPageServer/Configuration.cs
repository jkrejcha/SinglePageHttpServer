using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace SingleHttpPageServer
{
	public class Configuration
	{
		[JsonIgnore] public Object Source { get; private set; }
		[JsonIgnore] public ConfigurationType ConfigType { get; private set; } = ConfigurationType.Memory;

		public String DefaultFilePath = String.Empty;

		public int ResponseCode = 200;

		public Dictionary<String, List<String>> FileMappings = new Dictionary<String, List<string>>();

		public bool MacrosEnabled = true;

		public Dictionary<String, String> Macros = new Dictionary<string, string>();

		/// <summary>
		/// The setting for ports will only take effect when the server is restarted.
		/// </summary>
		public List<UInt16> Ports { get; set; } = new List<UInt16>() { 80 };

		public void Save()
		{
			switch (ConfigType)
			{
				case ConfigurationType.Memory: return;
				case ConfigurationType.JsonString:
					Source = JsonConvert.SerializeObject(this);
					break;
				case ConfigurationType.JsonFile:
					try
					{
						File.WriteAllText(Source.ToString(), JsonConvert.SerializeObject(this, Formatting.Indented));
					}
					catch (IOException e)
					{
						//Program.Log.Warn("Could not save configuration file: " + e.Message);
					}
					break;
				default:
					throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Loads a configuration
		/// </summary>
		/// <param name="type">Type of configuration</param>
		/// <param name="source">Object containing the source data, dependent on implementation</param>
		/// <returns></returns>
		public static Configuration Load(ConfigurationType type, Object sourceData)
		{
			bool save = false;
			Configuration c = new Configuration();
			if (type == ConfigurationType.Memory) return c;
			String sourceString = null;
			if (type.HasFlag(ConfigurationType.File))
			{
				try
				{
					sourceString = File.ReadAllText(sourceData.ToString());
				}
				catch (FileNotFoundException)
				{
					//Program.Log.Warn("Could not find config file; creating a new one...");
					save = true;
				}
				catch (IOException e)
				{
					//Program.Log.Error("Could not read configuration file: " + e.Message);
				}
			}
			else if (type.HasFlag(ConfigurationType.String))
			{
				sourceString = sourceData.ToString();
			}
			else
			{
				throw new NotImplementedException();
			}

			if (type.HasFlag(ConfigurationType.Json))
			{
				if (sourceString == null) sourceString = "{}";
				c = JsonConvert.DeserializeObject<Configuration>(sourceString);
			}
			else
			{
				throw new NotImplementedException();
			}
			c.Source = sourceData;
			c.ConfigType = type;
			if (save) c.Save();
			return c;
		}

		public enum ConfigurationType
		{
			/// <summary>
			/// Memory config; no loading performed
			/// </summary>
			Memory = 0x00,
			/// <summary>
			/// String based configuration; combine with other values
			/// </summary>
			String = 0x01,
			/// <summary>
			/// File based configuration; combine with other values
			/// </summary>
			File = 0x02,
			/// <summary>
			/// JSON-based configuration; Combine with other values.
			/// </summary>
			Json = 0x10,
			/// <summary>
			/// JSON string
			/// </summary>
			JsonString = Json | String,
			/// <summary>
			/// JSON file
			/// </summary>
			JsonFile = Json | File,
		}
	}
}
