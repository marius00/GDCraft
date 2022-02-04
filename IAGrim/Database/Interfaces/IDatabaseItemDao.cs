using System.Collections.Generic;
using IAGrim.Database.Dto;
using IAGrim.Database.Model;
using IAGrim.Parsers.GameDataParsing.Model;

namespace IAGrim.Database.Interfaces {
    public interface IDatabaseItemDao : IBaseDao<DatabaseItem> {
        Dictionary<string, string> GetTagDictionary();
        void Save(ICollection<DatabaseItem> items, ProgressTracker progressTracker);
        DatabaseItem FindByRecord(string record);
        IList<string> ListAllRecords();

        long GetRowCount();

        DatabaseItemDto FindDtoByRecord(string record);
        List<DatabaseItemDto> GetCraftableItems();
        List<DatabaseItemDto> GetByClass(string itemClass);

        List<DatabaseItemDto> GetNameMapping();
    }
}