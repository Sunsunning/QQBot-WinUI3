﻿using Newtonsoft.Json.Linq;
using PuppeteerSharp.Input;
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
        private readonly Dictionary<string, object> rawdata = new Dictionary<string, object>
            {
                { "QQBotPath", @"D:\QQBot"},
                { "BackGround","Acrylic(Thin)" },
                { "HTTP","http://valley.skyman.cloud/" },
                { "ChromePath",@"C:\Program Files\Google\Chrome\Application\chrome.exe" }
            };
        private List<string> folders = new List<string>() { "Bots","logs" };
        private List<string> files = new List<string>();

        public SettingManager()
        {
            _settings = new Dictionary<string, object>(rawdata);
            _configFilePath = Path.Combine(Directory.GetCurrentDirectory(), ConfigFileName);
            EnsureConfigFileExists();
            LoadSettings();
        }

        public void InitializeFolder()
        {
            string RootPath = this.GetValue<string>("QQBotPath");
            if (string.IsNullOrEmpty(RootPath))
            {
                return;
            }
            if (!Directory.Exists(RootPath))
            {
                Directory.CreateDirectory(RootPath);
            }
            try
            {
                foreach (var folder in folders)
                {
                    string path = Path.Combine(RootPath, folder);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
                foreach (var file in files)
                {
                    string path = Path.Combine(RootPath, file);
                    if (!File.Exists(path))
                    {
                        File.Create(path);
                    }
                }
            }
            catch (IOException)
            {
                return;
            }
            catch (Exception)
            {
                return;
            }
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

        public void ResetSettings()
        {
            _settings = new Dictionary<string, object>(rawdata);
            SaveSettings();
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
                        App.GetAppLogger().Log($"Error: {ex.Message}", false);
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
