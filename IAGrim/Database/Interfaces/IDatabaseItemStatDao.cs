using System.Collections.Generic;
using IAGrim.Database.DAO.Dto;
using IAGrim.Parsers.GameDataParsing.Model;
using IAGrim.Services.Dto;
using NHibernate;

namespace IAGrim.Database.Interfaces {
    public interface IDatabaseItemStatDao : IBaseDao<DatabaseItemStat> {

        Dictionary<string, string> MapItemBitmaps(List<string> records);
        void Save(IEnumerable<DatabaseItemStat> objs, ProgressTracker progressTracker);

        string GetSkillName(string skillRecord);
    }
}