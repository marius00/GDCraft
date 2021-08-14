using IAGrim.Database.DAO.Dto;
using IAGrim.Database.Interfaces;
using IAGrim.Services.Dto;
using log4net;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using IAGrim.Database.DAO.Table;
using IAGrim.Parsers.GameDataParsing.Model;

namespace IAGrim.Database {

    public class DatabaseItemStatDaoImpl : BaseDao<DatabaseItemStat>, IDatabaseItemStatDao {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DatabaseItemStatDaoImpl));

        // Utterly useless
        private static readonly string[] SpecialIgnores = {"physicsMass",
            "actorHeight", "actorRadius", "chest", "itemCost", "medalVisible",
            "medal", "marketAdjustmentPercent", "physicsFriction", "waist", "scale",
            "sword", "sword2h",  "castsShadows", "feet",};

        // Useful text stats
        private static readonly string[] SpecialStats = { "setName", "itemSetName", "petBonusName", "Class", "itemClassification",
            "augmentMasteryLevel1", "augmentMasteryLevel2", "augmentMasteryName1", "augmentMasteryName2",
            "augmentSkillLevel1", "augmentSkillLevel2", "augmentSkillName1", "augmentSkillName2",
            "augmentMasteryLevel4", "augmentMasteryLevel3", "augmentMasteryName4", "augmentMasteryName3",
            "augmentSkillLevel4", "augmentSkillLevel3", "augmentSkillName4", "augmentSkillName3",
            "augmentAllLevel",

            "skillDownBitmapName","skillUpBitmapName","bitmap","noteBitmap","artifactFormulaBitmapName","artifactBitmap",
            "bitmapButtonDown","bitmapButtonUp","relicBitmap","shardBitmap","emptyBitmap","fullBitmap",
            "lootRandomizerName", "itemNameTag", "itemQualityTag", "itemStyleTag", "description",
            "levelRequirement", "itemSkillName", "skillDisplayName", "petSkillName", "buffSkillName",
            "characterBaseAttackSpeedTag", "conversionInType", "conversionOutType", "racialBonusRace", "itemText", "MasteryEnumeration",

            "modifiedSkillName1", "modifiedSkillName2", "modifierSkillName1", "modifierSkillName2"
        };

        public DatabaseItemStatDaoImpl(ISessionCreator sessionCreator) : base(sessionCreator) {
        }


        public void Save(IEnumerable<DatabaseItemStat> objs, ProgressTracker progressTracker) {
            progressTracker.MaxValue = objs.Count();
            using (ISession session = SessionCreator.OpenSession()) {
                using (ITransaction transaction = session.BeginTransaction()) {
                    foreach (var entry in objs) {
                        session.Save(entry);
                        progressTracker.Increment();
                    }
                    transaction.Commit();
                }
            }

            progressTracker.Finalize();
        }

        
        public string GetSkillName(string skillRecord) {

            // TODO:
            // buffSkillName => New query, nature's blessing is a buff and thus has no name on the root
            string sql = $" SELECT t.{ItemTagTable.Name} FROM {DatabaseItemTable.Table} i, {DatabaseItemStatTable.Table} s, {ItemTagTable.Table} t " +
                         $" WHERE {DatabaseItemStatTable.Stat} = 'skillDisplayName' " +
                         $" AND s.{DatabaseItemStatTable.Item} = i.{DatabaseItemTable.Id} " +
                         $" AND i.{DatabaseItemTable.Record} = :record " +
                         $" AND t.{ItemTagTable.Id} = s.{DatabaseItemStatTable.TextValue} ";

            using (var session = SessionCreator.OpenSession()) {
                string result = session.CreateSQLQuery(sql)
                    .SetParameter("record", skillRecord)
                    .UniqueResult<string>();

                return result;
            }
        }


        public Dictionary<string, string> MapItemBitmaps(List<string> records) {
            Dictionary<string, int> bitmapScores = new Dictionary<string, int> {
                ["bitmap"] = 10,
                ["relicBitmap"] = 8,
                ["shardBitmap"] = 6,
                ["artifactBitmap"] = 4,
                ["noteBitmap"] = 2,
                ["artifactFormulaBitmapName"] = 0
            };

            Dictionary<string, string> recordBitmap = new Dictionary<string, string>();
            foreach (string record in records)
                recordBitmap[record] = record;

            // Grab all the possible bitmaps for each record
            using (var session = SessionCreator.OpenSession()) {
                using (ITransaction transaction = session.BeginTransaction()) {
                    DatabaseItemStat stat = null;
                    DatabaseItem P = null;
                    var stats = session.QueryOver<DatabaseItemStat>(() => stat)
                        .JoinAlias(() => stat.Parent, () => P)
                        .Where(m => P.Record.IsIn(records))
                        .Where(m => m.Stat.IsIn(new string[] { "bitmap", "relicBitmap", "shardBitmap", "artifactBitmap", "noteBitmap", "artifactFormulaBitmapName" }))
                        .List<DatabaseItemStat>();

                        
                    // Find the best bitmap for each record
                    foreach (string record in records) {
                        var best = stats.Where(m => m.Parent.Record.Equals(record)).OrderByDescending(m => bitmapScores[m.Stat]);
                        if (best.Any()) {
                            recordBitmap[record] = best.First().TextValue;
                        }
                    }

                }
            }

            return recordBitmap;
        }
    }
}
