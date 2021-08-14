using IAGrim.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAGrim.Utilities.HelperClasses {


    public class GDTransferFile : ComboBoxItemToggle {
        public string Filename { get; set; }
        public bool IsHardcore { get; set; }
        public string Mod { get; set; }
        public virtual bool Enabled { get; set; }

        public bool IsCloud { get; set; }

        public virtual DateTime LastAccess { get; set; }

        
        public override string ToString() {
            string text = Mod;
            if (this.IsHardcore)
                return $"{text} (HC)";
            else
                return text;
        }
    }

}
