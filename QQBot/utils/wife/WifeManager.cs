using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQBotCodePlugin.QQBot.utils.wife
{
    public class WifeManager
    {
        private string FileName = Path.Combine(Directories.ConfigPath, ".wife");
        private WifeListData _data;

        public WifeManager()
        {
            if (File.Exists(FileName))
            {
                string json = File.ReadAllText(FileName);
                _data = JsonConvert.DeserializeObject<WifeListData>(json);
            }

            if (_data == null)
            {
                _data = new WifeListData();
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
