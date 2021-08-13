﻿using IAGrim.Database.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IAGrim.Services.Dto;
using NHibernate;
using IAGrim.Database.DAO.Dto;
using IAGrim.Parsers.GameDataParsing.Model;

namespace IAGrim.Database.Synchronizer {
    class DatabaseItemStatRepo : BasicSynchronizer<DatabaseItemStat>, IDatabaseItemStatDao {
        private readonly IDatabaseItemStatDao repo;
        public DatabaseItemStatRepo(ThreadExecuter threadExecuter, ISessionCreator sessionCreator) : base(threadExecuter, sessionCreator) {
            this.repo = new DatabaseItemStatDaoImpl(sessionCreator);
            this.BaseRepo = repo;
        }


        public Dictionary<string, string> MapItemBitmaps(List<string> records) {
            return ThreadExecuter.Execute(
                () => repo.MapItemBitmaps(records)
            );
        }

        public void Save(IEnumerable<DatabaseItemStat> objs, ProgressTracker progressTracker) {
            ThreadExecuter.Execute(
                () => repo.Save(objs, progressTracker)
            );
        }


        public string GetSkillName(string skillRecord) {
            return ThreadExecuter.Execute(
                () => repo.GetSkillName(skillRecord)
            );
        }
    }
}
