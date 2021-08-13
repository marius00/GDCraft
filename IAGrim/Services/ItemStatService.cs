using IAGrim.Database;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Caching;
using NHibernate;
using NHibernate.Transform;
using IAGrim.Services.Dto;
using log4net;
using IAGrim.Database.Interfaces;
using IAGrim.Database.DAO.Dto;
using IAGrim.Database.Model;
using MoreLinq;

namespace IAGrim.Services {
    class ItemStatService {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ItemStatService));
        private readonly IDatabaseItemStatDao _databaseItemStatDao;
        private readonly IItemSkillDao _itemSkillDao;
        private bool _displaySkills => Properties.Settings.Default.DisplaySkills;
        private readonly Dictionary<string, ISet<DBSTatRow>> _xpacSkills;




        public ItemStatService(
            IDatabaseItemStatDao databaseItemStatDao, 
            IItemSkillDao itemSkillDao
        ) {
            this._databaseItemStatDao = databaseItemStatDao;
            this._itemSkillDao = itemSkillDao;
            _xpacSkills = _databaseItemStatDao.GetExpacSkillModifierSkills();
        }



        private static List<string> GetRecordsForItem(BaseItem item) {
            List<string> records = new List<string>();
            if (!string.IsNullOrEmpty(item.BaseRecord)) {
                records.Add(item.BaseRecord);
            }
            if (!string.IsNullOrEmpty(item.PrefixRecord)) {
                records.Add(item.PrefixRecord);
            }
            if (!string.IsNullOrEmpty(item.SuffixRecord)) {
                records.Add(item.SuffixRecord);
            }
            if (!string.IsNullOrEmpty(item.MateriaRecord)) {
                records.Add(item.MateriaRecord);
            }
            if (!string.IsNullOrEmpty(item.PetRecord)) {
                records.Add(item.PetRecord);
            }

            return records;
        }


        private static List<string> GetRecordsForItems(List<PlayerHeldItem> items) {
            return items.Select(m => m.BaseRecord).Distinct().ToList();
        }

        private bool IsPlayerItem(PlayerHeldItem item) {
            return string.IsNullOrEmpty(item.Stash) && !item.IsRecipe;
        }

        private bool IsBuddyItem(PlayerHeldItem item) {
            return !string.IsNullOrEmpty(item.Stash) && !item.IsRecipe;
        }

        private List<PlayerItem> GetPlayerItems(List<PlayerHeldItem> items) {
            var result = new List<PlayerItem>();
            foreach (PlayerHeldItem item in items.Where(IsPlayerItem)) {
                result.Add(item as PlayerItem);
            }

            return result;
        }

        private void ApplyStatsToRecipeItems(List<PlayerHeldItem> items) {

            var records = GetRecordsForItems(items);
            Dictionary<string, List<DBSTatRow>> statMap = _databaseItemStatDao.GetStats(records, StatFetch.RecipeItems);

            foreach (PlayerHeldItem phi in items) {
                List<DBSTatRow> stats = new List<DBSTatRow>();
                if (statMap.ContainsKey(phi.BaseRecord))
                    stats.AddRange(Filter(statMap[phi.BaseRecord]));

                var statsWithText = Filter(stats.Where(m => !string.IsNullOrEmpty(m.TextValue)));
                List<DBSTatRow> statsWithNumerics = stats.Where(m => string.IsNullOrEmpty(m.TextValue))
                    .GroupBy(r => r.Stat)
                    .Select(g => new DBSTatRow {
                        Record = g.FirstOrDefault()?.Record,
                        TextValue = g.FirstOrDefault()?.TextValue,
                        Stat = g.FirstOrDefault()?.Stat,
                        Value = g.Sum(v => v.Value)
                    })
                    .ToList();

                statsWithNumerics.AddRange(statsWithText);

                phi.Tags = new HashSet<DBSTatRow>(statsWithNumerics);
            }

            Logger.Debug($"Applied stats to {items.Count()} items");
        }

        private void ApplyMythicalBonuses(List<PlayerHeldItem> items) {
            var itemsWithXpacStat = items.Where(m => m.Tags.Any(s => s.Stat == "modifiedSkillName1"));
            foreach (var item in itemsWithXpacStat) {
                var affectedSkill = item.Tags.FirstOrDefault(m => m.Stat == "modifiedSkillName1");
                var recordForStats = item.Tags.FirstOrDefault(m => m.Stat == "modifierSkillName1")?.TextValue;

                if (recordForStats == null || !_xpacSkills.ContainsKey(recordForStats)) {
                    Logger.Warn($"Could not find stats for the skill {recordForStats}");
                    continue;
                }

                var name = affectedSkill?.TextValue;
                if (!string.IsNullOrEmpty(name)) {
                    name = _databaseItemStatDao.GetSkillName(name);
                }
                item.ModifiedSkills.Add(new SkillModifierStat {
                    Tags = _xpacSkills[recordForStats],
                    Name = name
                });
            }
        }



        private HashSet<DBSTatRow> process(List<DBSTatRow> stats) {

            var statsWithText = Filter(stats.Where(m => !string.IsNullOrEmpty(m.TextValue)));
            List<DBSTatRow> statsWithNumerics = stats.Where(m => string.IsNullOrEmpty(m.TextValue))
                .GroupBy(r => r.Stat)
                .Select(g => new DBSTatRow {
                    Record = g.FirstOrDefault().Record,
                    TextValue = g.FirstOrDefault().TextValue,
                    Stat = g.FirstOrDefault().Stat,
                    Value = g.Sum(v => v.Value),
                })
                .ToList();

            statsWithNumerics.AddRange(statsWithText);
            return new HashSet<DBSTatRow>(statsWithNumerics);
        }

        private IEnumerable<DBSTatRow> Filter(IEnumerable<DBSTatRow> stats) {
            return stats.GroupBy(r => r.Stat)
                    .Select(g => g.OrderByDescending(m => {
                        return m.Value;
                    }).First());
        }
        
    }
}
