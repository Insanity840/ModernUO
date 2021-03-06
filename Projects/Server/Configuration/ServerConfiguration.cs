/*************************************************************************
 * ModernUO                                                              *
 * Copyright (C) 2019-2020 - ModernUO Development Team                   *
 * Email: hi@modernuo.com                                                *
 * File: ServerConfiguration.cs                                          *
 * Created: 2019/10/04 - Updated: 2020/05/09                             *
 *                                                                       *
 * This program is free software: you can redistribute it and/or modify  *
 * it under the terms of the GNU General Public License as published by  *
 * the Free Software Foundation, either version 3 of the License, or     *
 * (at your option) any later version.                                   *
 *                                                                       *
 * This program is distributed in the hope that it will be useful,       *
 * but WITHOUT ANY WARRANTY; without even the implied warranty of        *
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
 * GNU General Public License for more details.                          *
 *                                                                       *
 * You should have received a copy of the GNU General Public License     *
 * along with this program.  If not, see <http://www.gnu.org/licenses/>. *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using Server.Json;

namespace Server
{
  public static class ServerConfiguration
  {
    private const string m_RelPath = "Configuration/modernuo.json";
    private static readonly string m_FilePath = Path.Join(Core.BaseDirectory, m_RelPath);
    private static ServerSettings m_Settings;
    private static bool m_Mocked;

    public static List<string> DataDirectories => m_Settings.dataDirectories;

    public static string GetSetting(string key, string defaultValue)
    {
      m_Settings.settings.TryGetValue(key, out string value);
      return value == "(-null-)" ? null : value ?? defaultValue;
    }

    public static int GetSetting(string key, int defaultValue)
    {
      m_Settings.settings.TryGetValue(key, out string strValue);
      return int.TryParse(strValue, out int value) ? value : defaultValue;
    }

    public static bool GetSetting(string key, bool defaultValue)
    {
      m_Settings.settings.TryGetValue(key, out string strValue);
      return bool.TryParse(strValue, out bool value) ? value : defaultValue;
    }

    public static T GetSetting<T>(string key, T defaultValue) where T : struct, Enum
    {
      m_Settings.settings.TryGetValue(key, out string strValue);
      return Enum.TryParse(strValue, out T value) ? value : defaultValue;
    }

    public static string GetOrUpdateSetting(string key, string defaultValue)
    {
      if (m_Settings.settings.TryGetValue(key, out string value))
        return value;

      SetSetting(key, value = defaultValue);
      return value;
    }

    public static int GetOrUpdateSetting(string key, int defaultValue)
    {
      int value;

      if (m_Settings.settings.TryGetValue(key, out string strValue))
        value = int.TryParse(strValue, out value) ? value : defaultValue;
      else
        SetSetting(key, (value = defaultValue).ToString());

      return value;
    }

    public static bool GetOrUpdateSetting(string key, bool defaultValue)
    {
      bool value;

      if (m_Settings.settings.TryGetValue(key, out string strValue))
        value = bool.TryParse(strValue, out value) ? value : defaultValue;
      else
        SetSetting(key, (value = defaultValue).ToString());

      return value;
    }

    public static TimeSpan GetOrUpdateSetting(string key, TimeSpan defaultValue)
    {
      TimeSpan value;

      if (m_Settings.settings.TryGetValue(key, out string strValue))
        value = TimeSpan.TryParse(strValue, out value) ? value : defaultValue;
      else
        SetSetting(key, (value = defaultValue).ToString());

      return value;
    }

    public static T GetOrUpdateSetting<T>(string key, T defaultValue) where T : struct, Enum
    {
      T value;

      if (m_Settings.settings.TryGetValue(key, out string strValue))
        value = Enum.TryParse(strValue, out value) ? value : defaultValue;
      else
        SetSetting(key, (value = defaultValue).ToString());

      return value;
    }

    public static void SetSetting(string key, string value)
    {
      m_Settings.settings[key] = value;
      Save();
    }

    public static T GetMetadata<T>(string key) where T : class
    {
      m_Settings.metadata.TryGetValue(key, out object value);
      return value as T;
    }

    public static void SetMetadata(string key, object value)
    {
      m_Settings.metadata[key] = value;
    }

    // If mock is enabled we skip the console readline.
    public static void Load(bool mocked = false)
    {
      m_Mocked = mocked;
      bool updated = false;

      if (File.Exists(m_FilePath))
      {
        Console.Write($"Core: Reading configuration from {m_RelPath}...");
        m_Settings = JsonConfig.Deserialize<ServerSettings>(m_FilePath);

        if (m_Settings == null)
        {
          Utility.PushColor(ConsoleColor.Red);
          Console.WriteLine("failed");
          Utility.PopColor();
          throw new Exception("Core: Server configuration failed to deserialize.");
        }

        Console.WriteLine("done");
      }
      else
      {
        updated = true;
        m_Settings = new ServerSettings();
      }

      if (mocked)
        return;

      if (m_Settings.dataDirectories.Count == 0)
      {
        updated = true;
        Console.WriteLine("Core: Server configuration is missing data directories.");
        m_Settings.dataDirectories.Add(GetDataDirectory());
      }

      if (updated)
      {
        Save();
        Utility.PushColor(ConsoleColor.Green);
        Console.WriteLine($"Core: Configuration saved to {m_RelPath}.");
        Utility.PopColor();
      }
    }

    internal class ServerSettings
    {
      [JsonPropertyName("dataDirectories")]
      public List<string> dataDirectories { get; set; } = new List<string>();

      [JsonPropertyName("settings")]
      public Dictionary<string, string> settings { get; set; } = new Dictionary<string, string>();

      [JsonExtensionData]
      public Dictionary<string, object> metadata { get; set; } = new Dictionary<string, object>();
    }

    private static string GetDataDirectory()
    {
      Console.WriteLine("Please enter the Ultima Online directory:");

      string directory;
      do
      {
        Console.Write("> ");
        directory = Console.ReadLine();
      } while (!Directory.Exists(directory));

      return directory;
    }

    public static void Save()
    {
      if (m_Mocked) return;

      JsonConfig.Serialize(m_FilePath, m_Settings);
    }
  }
}
