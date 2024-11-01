using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace QQBotCodePlugin.QQBot.utils.wife
{
    public class WifeManager
    {
        private string FileName;
        private WifeListData _data;

        public WifeManager(Logger logger, Bot bot)
        {
            FileName = Path.Combine(bot.getConfig(), ".wife");
            try
            {
                if (File.Exists(FileName))
                {
                    using (var stream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var json = new StreamReader(stream).ReadToEnd();
                        _data = JsonConvert.DeserializeObject<WifeListData>(json);
                    }
                }
                if (_data == null)
                {
                    _data = new WifeListData();
                }
            }
            catch (IOException ex)
            {
                logger.Error($"Error reading file: {ex.Message}");
            }
        }

        public void AddOrUpdateWife(long groupId, long sourceQQ, long targetQQ)
        {
            if (_data.Time.Date != DateTime.Now.Date)
            {
                foreach (var group in _data.Wife)
                {
                    group.Value.Clear();
                }
                _data.Time = DateTime.Now;
            }

            if (!_data.Wife.ContainsKey(groupId))
            {
                _data.Wife[groupId] = new Dictionary<long, long>();
            }

            _data.Wife[groupId][sourceQQ] = targetQQ;

            SaveData();
        }

        public long GetTargetQQ(long groupId, long sourceQQ)
        {
            if (_data.Time.Date == null)
            {
                _data.Time = DateTime.Now;
            }

            if (_data.Time.Date != DateTime.Now.Date)
            {
                foreach (var group in _data.Wife)
                {
                    group.Value.Clear();
                }
                _data.Time = DateTime.Now;
            }

            if (_data.Wife.TryGetValue(groupId, out var groupWife) &&
                groupWife.TryGetValue(sourceQQ, out long targetQQ))
            {
                return targetQQ;
            }
            return -1;
        }


        private void SaveData()
        {
            string json = JsonConvert.SerializeObject(_data, Formatting.Indented);
            File.WriteAllText(FileName, json);
        }
    }
}
