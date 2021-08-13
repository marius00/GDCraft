using DataAccess;
using IAGrim.Database.Interfaces;
using IAGrim.Services.Dto;
using IAGrim.Utilities;
using StatTranslator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IAGrim.Database.Model;

namespace IAGrim.Database {
    public class RecipeItem {
        public virtual long Id { get; set; }

        public virtual string BaseRecord { get; set; }
        public virtual int PrefixRarity => 0;

        private DatabaseItem Internal { get; set; }
        public virtual bool IsHardcore { get; set; }
        public virtual bool IsExpansion1 { get; set; }
        
        public virtual bool IsKnown { get { return Internal != null; } }

        public virtual ISet<DBSTatRow> Tags {
            get;
            set;
        }


        public virtual List<SkillModifierStat> ModifiedSkills { get; } = new List<SkillModifierStat>();

        public virtual IList<TranslatedStat> HeaderStats => GlobalSettings.StatManager.ProcessStats(new HashSet<IItemStat>(Tags), TranslatedStatType.HEADER);

        public virtual IList<TranslatedStat> BodyStats => GlobalSettings.StatManager.ProcessStats(new HashSet<IItemStat>(Tags), TranslatedStatType.BODY);

        public virtual string Bitmap => DatabaseItem.GetBitmap(Tags);

        public virtual string Rarity => DatabaseItem.GetRarity(Tags);

        public virtual float MinimumLevel {
            get; set;
        }

        public virtual string Slot => DatabaseItem.GetSlot(Tags);

        public virtual string Stash => null;

        public virtual bool IsRecipe => true;

        public virtual bool HasRecipe
        {
            get { return true; }
            set { }
        }

        public virtual List<string> Buddies { get; set; } = new List<string>();

        public virtual uint Count {
            get { return 1; }
            set { }
        }

        public virtual string Name => Internal?.Name;

    }
}
