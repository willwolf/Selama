﻿using BattleNetApi.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleNetApi.Objects.WoW
{
    public class GuildNews
    {
        #region Properties
        public string Type { get; private set; }

        public string CharacterName { get; private set; }

        public DateTime Timestamp { get; private set; }

        public string Context { get; private set; }

        public List<int> BonusLists { get; private set; }

        #endregion

        internal static GuildNews ParseGuildNews(JObject guildNewsJson)
        {
            string context = guildNewsJson["type"].Value<string>();
            switch (context)
            {
                case "itemLoot":
                case "itemPurchase":
                case "itemCraft":
                    return GuildNewsPlayerItem.BuildPlayerItemNews(guildNewsJson);
                case "playerAchievement":
                    return GuildNewsPlayerAchievement.BuildPlayerAchievement(guildNewsJson);
                default:
                    return new GuildNews(guildNewsJson);
            }
        }

        protected GuildNews(JObject guildNewsJson)
        {
            Type = guildNewsJson["type"].Value<string>();
            Context = guildNewsJson["context"].Value<string>();
            Timestamp = Util.BuildUnixTimestamp(guildNewsJson["timestamp"].Value<long>());
            // TODO: Watch for character not being present in json
            CharacterName = guildNewsJson["character"].Value<string>();

            ParseBonusLists(guildNewsJson["bonusLists"]);
        }

        private void ParseBonusLists(JToken bonusListJson)
        {
            BonusLists = new List<int>();
            foreach (var bonus in bonusListJson.AsEnumerable())
            {
                BonusLists.Add(bonus.Value<int>());
            }
        }
    }
}