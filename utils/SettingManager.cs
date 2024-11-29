using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace QQBotCodePlugin.utils
{
    class SettingManager
    {
        private const string ConfigFileName = "config.qq";
        private Dictionary<string, object> _settings;
        private string _configFilePath;

        public SettingManager()
        {
            _settings = new Dictionary<string, object>
            {
                { "QQBotPath", @"D:\QQBot"},
                { "Color","跟随系统" },
                { "BackGround","Acrylic(Thin)" },
                { "HTTP","http://valley.skyman.cloud/" },
                { "ChromePath",@"C:\Program Files\Google\Chrome\Application\chrome.exe" }
            };
            _configFilePath = Path.Combine(Directory.GetCurrentDirectory(), ConfigFileName);
            EnsureConfigFileExists();
            LoadSettings();
        }

        private void EnsureConfigFileExists()
        {
            if (!File.Exists(_configFilePath))
            {
                using (var writer = new StreamWriter(_configFilePath))
                {
                    foreach (var pair in _settings)
                    {
                        writer.WriteLine($"{pair.Key}={pair.Value}");
                    }
                }
            }
        }

        public void LoadSettings()
        {
            if (File.Exists(_configFilePath))
            {
                string content = File.ReadAllText(_configFilePath);
                var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var keyValue = line.Split('=');
                    if (keyValue.Length == 2)
                    {
                        string key = keyValue[0].Trim();
                        string value = keyValue[1].Trim();
                        if (_settings.ContainsKey(key))
                        {
                            _settings[key] = value;
                        }
                        else
                        {
                            _settings.Add(key, value);
                        }
                    }
                }
            }
        }

        public void SaveSettings()
        {
            using (StreamWriter writer = new StreamWriter(_configFilePath))
            {
                foreach (var pair in _settings)
                {
                    writer.WriteLine($"{pair.Key}={pair.Value}");
                }
            }
        }

        public void SetValue<T>(string key, T value)
        {
            if (_settings.ContainsKey(key))
            {
                _settings[key] = value.ToString();
            }
            else
            {
                _settings.Add(key, value.ToString());
            }
            SaveSettings();
            LoadSettings();
        }

        public T GetValue<T>(string key)
        {
            LoadSettings();
            if (_settings.TryGetValue(key, out object value))
            {
                if (value is T typedValue)
                {
                    return typedValue;
                }
                else if (value is string stringValue)
                {
                    try
                    {
                        return (T)Convert.ChangeType(value, typeof(T));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Conversion error: {ex.Message}");
                        return default(T);
                    }
                }
            }
            return default(T);
        }

        public bool ContainsKey(string key)
        {
            return _settings.ContainsKey(key);
        }
    }
}
