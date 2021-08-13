using System;
using System.Windows.Forms;

namespace IAGrim.UI {
    class UIHelper {
        public static Action<Form, Panel> AddAndShow = (Form f, Panel p) => {
            f.TopLevel = false;
            p.Controls.Add(f);
            f.Show();
        };
    }
}
