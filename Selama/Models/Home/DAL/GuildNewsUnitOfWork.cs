﻿using BattleNetApi.Api;
using BattleNetApi.Objects.WoW;
using Selama.Areas.Forums.Models.DAL;
using Selama.Common.ExtensionMethods;
using Selama.Common.Utility;
using Selama.ViewModels.Home;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Selama.Models.Home.DAL
{
    public class GuildNewsUnitOfWork: IDisposable
    {
        private BattleNetApiClient _battleNetClient { get; set; }
        private ForumsUnitOfWork _forumsDb { get; set; }

        public GuildNewsUnitOfWork()
        {
            _battleNetClient = new BattleNetApiClient(Util.BattleNetApiClientId);
            _forumsDb = new ForumsUnitOfWork();
        }

        public async Task<List<GuildNewsViewModel>> GetGuildNews()
        {
            Task<List<GuildNewsViewModel>> battleNetNews = GetBattleNetNews();

            List<GuildNewsViewModel> result = new List<GuildNewsViewModel>();
            battleNetNews.Wait();
            result.AddRange(battleNetNews.Result);

            result.Sort();
            return result;
        }

        private async Task<List<GuildNewsViewModel>> GetBattleNetNews()
        {
            Guild guildProfile = await _battleNetClient.WowCommunityApi.GetGuildProfileAsync(Util.WowRealmName, Util.WowGuildName, "news");

            return guildProfile.News.ToListOfDifferentType(GuildNewsViewModel.BuildModelFromBattleNetGuildNews);
        }



        public void Dispose()
        {
            _forumsDb.Dispose();
        }
    }
}