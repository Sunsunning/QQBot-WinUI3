
using QQBotCodePlugin.QQBot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using static PluginDLL.Class1;

namespace QQBotCodePlugin.Plugin
{
    public class PluginHost
    {
        private ILoggerService loggerService;
        private readonly Logger logger;
        private readonly Bot bot;
        private List<IPlugin> plugins = new List<IPlugin>();

        public PluginHost(Logger logger, Bot bot)
        {
            loggerService = new HostLoggerService(logger);
            this.bot = bot;
            this.logger = logger;
            logger.Info("Loading Plugins……");
            bot.GroupReceived += Bot_GroupReceived;
        }

        public List<IPlugin> GetPlugins() => plugins;

        private void Bot_GroupReceived(object sender, MessageEvent e)
        {
            foreach (var plugin in plugins)
            {
                plugin.Execute(e);
            }
        }

        public void RegisterPlugin(IPlugin plugin)
        {
            Debug.WriteLine($"register {plugin}");
            plugin.SetHostService(loggerService, bot.Service, bot.configService);
            plugin.OnEnable();
            plugins.Add(plugin);
        }

        public void LoadPlugins(string pluginDirectory)
        {
            var pluginFiles = Directory.GetFiles(pluginDirectory, "*.dll");
            if (pluginFiles.Length == 0)
            {
                logger.Warning($"No plugins found in the plugin directory. Skipping plugin loading.");
                return;
            }

            foreach (var file in pluginFiles)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFile(file);
                    var pluginTypes = assembly.GetTypes().Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface);
                    foreach (var type in pluginTypes)
                    {
                        logger.Info($"Found plugin type: {type.FullName}");
                        try
                        {
                            var pluginInstance = Activator.CreateInstance(type) as IPlugin;
                            if (pluginInstance != null)
                            {
                                RegisterPlugin(pluginInstance);
                                logger.Info($"Registered plugin: {pluginInstance}");
                            }
                            else
                            {
                                logger.Error($"Failed to create instance of {type.FullName}");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"Error creating instance of {type.FullName}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Error loading plugin {Path.GetFileName(file)}: {ex.Message}");
                    logger.Error(ex.StackTrace);
                }
            }

        }
    }
}
