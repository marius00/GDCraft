﻿using IAGrim.Database.Dto;
using IAGrim.Database.Interfaces;
using log4net;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using DataAccess;
using IAGrim.Database.DAO.Table;
using IAGrim.Database.Model;
using IAGrim.Parsers.GameDataParsing.Model;
using IAGrim.Services.Dto;
using log4net.Repository.Hierarchy;

namespace IAGrim.Database {
    /// <summary>
    /// Database class for handling internal Grim Dawn items
    /// These are not user owned items
    /// </summary>
    public class DatabaseItemDaoImpl : BaseDao<DatabaseItem>, IDatabaseItemDao {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DatabaseItemDaoImpl));

        public DatabaseItemDaoImpl(ISessionCreator sessionCreator) : base(sessionCreator) {
        }


        /// <summary>
        /// Get the existing tag dictionary
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetTagDictionary() {
            Dictionary<string, string> result = new Dictionary<string, string>();
            using (var session = SessionCreator.OpenStatelessSession()) {
                using (ITransaction transaction = session.BeginTransaction()) {
                    foreach (ItemTag entry in session.CreateCriteria<ItemTag>().List()) {
                        result[entry.Tag] = entry.Name;
                    }
                }
            }

            return result;
        }



        public void Save(DatabaseItem item) {

            using (var session = SessionCreator.OpenSession()) {
                using (ITransaction transaction = session.BeginTransaction()) {

                    session.CreateQuery("DELETE FROM DatabaseItemStat as S WHERE S.Parent.Id = :id")
                        .SetParameter("id", item.Id)
                        .ExecuteUpdate();
                        
                    session.CreateQuery("DELETE FROM DatabaseItem WHERE Record = :record")
                        .SetParameter("record", item.Record)
                        .ExecuteUpdate();

                    transaction.Commit();
                }
                using (ITransaction transaction = session.BeginTransaction()) {

                    foreach (DatabaseItemStat stat in item.Stats) {
                        stat.Parent = item;
                        //session.Insert(stat);
                    }
                    session.Save(item);

                    transaction.Commit();
                }
            }

        }

        private void ExecuteTransactionSql(SQLiteConnection dbConnection, string[] commands) {
            using (var transaction = dbConnection.BeginTransaction()) {
                using (SQLiteCommand command = new SQLiteCommand(dbConnection)) {
                    foreach (var sql in commands) {
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
            
        }

        public void Save(ICollection<DatabaseItem> items, ProgressTracker progressTracker) {
            Save(items, progressTracker, true);
        }
        private void Save(ICollection<DatabaseItem> items, ProgressTracker progressTracker, bool reset) {
            long numStats = 0;

            progressTracker.MaxValue = items.Count;
            using (var session = SessionCreator.OpenSession()) {
                if (reset) {
                    using (ITransaction transaction = session.BeginTransaction()) {
                        session.CreateQuery("DELETE FROM DatabaseItemStat").ExecuteUpdate();
                        session.CreateQuery("DELETE FROM DatabaseItem").ExecuteUpdate();
                        session.CreateSQLQuery("UPDATE SQLITE_SEQUENCE SET seq = 0 WHERE name = 'DatabaseItemStat_v2'").ExecuteUpdate();
                        transaction.Commit();
                    }
                }
                
                using (ITransaction transaction = session.BeginTransaction()) {
                    foreach (DatabaseItem item in items) {
                        session.Save(item);
                        progressTracker.Increment();
                    }
                    transaction.Commit();
                }
            }


            var createCommands = new string[] {
                        "create index idx_databaseitemstatv2_parent on DatabaseItemStat_v2 (id_databaseitem)",
                        "create index idx_databaseitemstatv2_stat on DatabaseItemStat_v2 (Stat)",
                        "create index idx_databaseitemstatv2_tv on DatabaseItemStat_v2 (TextValue)"
                    };
            var dropCommands = new string[] {
                        "drop index if exists idx_databaseitemstatv2_parent",
                        "drop index if exists idx_databaseitemstatv2_stat",
                        "drop index if exists idx_databaseitemstatv2_tv"
                    };


            using (SQLiteConnection dbConnection = new SQLiteConnection(SessionFactoryLoader.SessionFactory.ConnectionString)) {
                dbConnection.Open();
                var sq = new System.Diagnostics.Stopwatch();
                sq.Start();

                ExecuteTransactionSql(dbConnection, dropCommands);

                using (var transaction = dbConnection.BeginTransaction()) {
                    using (SQLiteCommand command = new SQLiteCommand(dbConnection)) {
                        string sql = "insert into databaseitemstat_v2 (id_databaseitem, stat, textvalue, val1) values (@id, @stat, @tv, @val)";
                        command.CommandText = sql;

                        foreach (DatabaseItem item in items) {
                            foreach (DatabaseItemStat stat in item.Stats) {
                                command.Parameters.Add(new SQLiteParameter("@id", item.Id));
                                command.Parameters.Add(new SQLiteParameter("@stat", stat.Stat));
                                command.Parameters.Add(new SQLiteParameter("@tv", stat.TextValue));
                                command.Parameters.Add(new SQLiteParameter("@val", stat.Value));
                                command.ExecuteNonQuery();
                                numStats++;
                            }
                        }
                    }

                    transaction.Commit();
                }
                ExecuteTransactionSql(dbConnection, createCommands);

                sq.Stop();
                Logger.Info("Records stored");
                Console.WriteLine($"Storing the records took {sq.ElapsedMilliseconds} milliseconds");
            }

            Logger.InfoFormat("Stored {0} items and {1} stats to internal db.", items.Count, numStats);
        }



        public DatabaseItem FindByRecord(string record) {
            using (ISession session = SessionCreator.OpenSession()) {
                using (ITransaction transaction = session.BeginTransaction()) {
                    var item = session.CreateCriteria<DatabaseItem>().Add(Restrictions.Eq("Record", record)).UniqueResult();
                    if (item != null)
                        return (DatabaseItem)item;
                }
            }

            return null;
        }


        internal class InteralRowStat {
            public string Record { get; set; }
            public string Stat { get; set; }
            public double Value { get; set; }
            public string TextValue { get; set; }

            public string Name { get; set; }
        }


        public List<DatabaseItemDto> GetByClass(string itemClass) {

            using (ISession session = SessionCreator.OpenSession()) {
                using (ITransaction transaction = session.BeginTransaction()) {
                    var subquery = $"SELECT {DatabaseItemStatTable.Item} FROM {DatabaseItemStatTable.Table} " +
                                   $" WHERE {DatabaseItemStatTable.Stat} = 'Class' AND {DatabaseItemStatTable.TextValue} = :itemClass";
                    var sql = string.Join(" ",
                        $" SELECT {DatabaseItemTable.Record} as Record, ",
                        $" {DatabaseItemTable.Name} as Name, ",
                        $" {DatabaseItemStatTable.Stat} as Stat, {DatabaseItemStatTable.Value} as Value, {DatabaseItemStatTable.TextValue} AS TextValue",
                        $" FROM {DatabaseItemTable.Table} i, {DatabaseItemStatTable.Table} s ",
                        $" WHERE i.{DatabaseItemTable.Id} = s.{DatabaseItemStatTable.Item} ",
                        $" AND i.{DatabaseItemTable.Id} IN ({subquery})"
                    );

                    IQuery query = session.CreateSQLQuery(sql)
                        .SetParameter("itemClass", itemClass)
                        .SetResultTransformer(Transformers.AliasToBean<InteralRowStat>());

                    return ToDto(query.List<InteralRowStat>());
                }
            }
        }

        public List<DatabaseItemDto> GetCraftableItems() {
            using (ISession session = SessionCreator.OpenSession()) {
                using (ITransaction transaction = session.BeginTransaction()) {
                    var subquery1 = string.Join(" ",
                        $"SELECT {DatabaseItemStatTable.TextValue} FROM {DatabaseItemStatTable.Table} ",
                        $"WHERE {DatabaseItemStatTable.Stat} = 'artifactName'"
                    );

                    var subquery2 = string.Join(" ",
                        $"SELECT {DatabaseItemStatTable.Item} FROM {DatabaseItemStatTable.Table} ",
                        $" WHERE {DatabaseItemStatTable.Stat} = 'Class' AND {DatabaseItemStatTable.TextValue} = 'ItemRelic'"
                    );

                    var sql = string.Join(" ",
                        $" SELECT {DatabaseItemTable.Record} as Record,",
                        $"{DatabaseItemTable.Name} as Name, {DatabaseItemStatTable.Stat} as Stat, ",
                        $"{DatabaseItemStatTable.Value} as Value, {DatabaseItemStatTable.TextValue} as TextValue",
                        $" FROM {DatabaseItemTable.Table} i, {DatabaseItemStatTable.Table} s ",
                        $" WHERE i.{DatabaseItemTable.Id} = s.{DatabaseItemStatTable.Item} ",
                        $" AND ({DatabaseItemTable.Record} IN ({subquery1}) OR i.{DatabaseItemTable.Id} IN ({subquery2}))"
                    );

                    Logger.Debug(sql);

                    IQuery query = session.CreateSQLQuery(sql)
                        .SetResultTransformer(Transformers.AliasToBean<InteralRowStat>());

                    return ToDto(query.List<InteralRowStat>());
                }
            }
        }

        private static List<DatabaseItemDto> ToDto(IList<InteralRowStat> rows) {
            List<DatabaseItemDto> items = new List<DatabaseItemDto>();

            DatabaseItemDto item = null;
            foreach (InteralRowStat row in rows) {
                // New item
                if (item == null || !item.Record.Equals(row.Record)) {
                    item = new DatabaseItemDto {
                        Record = row.Record,
                        Name = row.Name
                    };
                    items.Add(item);
                }

                item.Stats.Add(new RecipeDbStatRow {
                    Stat = row.Stat,
                    Value = row.Value,
                    TextValue = row.TextValue,
                    Parent = item
                });
            }

            return items;
        }


        public DatabaseItemDto FindDtoByRecord(string record) {
            using (ISession session = SessionCreator.OpenSession()) {
                using (ITransaction transaction = session.BeginTransaction()) {
                    var sql = string.Join(" ",
                        $" SELECT {DatabaseItemTable.Record} as Record, {DatabaseItemTable.Name} as Name, ",
                        $"{DatabaseItemStatTable.Stat} as Stat, {DatabaseItemStatTable.Value} as Value, ",
                        $"{DatabaseItemStatTable.TextValue} as TextValue",
                        $" FROM {DatabaseItemTable.Table} i, {DatabaseItemStatTable.Table} s ",
                        $" WHERE i.{DatabaseItemTable.Id} = s.{DatabaseItemStatTable.Item} ",
                        $" AND {DatabaseItemTable.Record} = :record "
                    );

                    IQuery query = session.CreateSQLQuery(sql)
                        .SetParameter("record", record)
                        .SetResultTransformer(Transformers.AliasToBean<InteralRowStat>());

                    foreach (DatabaseItemDto elem in ToDto(query.List<InteralRowStat>())) {
                        return elem;
                    }
                }
            }

            return null;
        }



        public IList<string> ListAllRecords() {
            using (ISession session = SessionCreator.OpenSession()) {
                using (ITransaction transaction = session.BeginTransaction()) {
                    return session.CreateCriteria<DatabaseItem>()
                        .SetProjection(Projections.Property("Record"))
                        .AddOrder(Order.Asc("Record"))
                        .List<string>();
                }
            }
        }


        public Int64 GetRowCount() {
            using (ISession session = SessionCreator.OpenSession()) {
                using (ITransaction transaction = session.BeginTransaction()) {
                    return session.CreateCriteria<DatabaseItem>()
                        .SetProjection(Projections.RowCountInt64())
                        .UniqueResult<Int64>();
                }
            }
        }
        

    }
}
