using IAGrim.UI.Controller;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IAGrim.Utilities;

namespace IAGrim.UI.Misc {

    public class JSWrapper {
        private JsonSerializerSettings _settings;

        public event EventHandler OnRequestRecipeList;
        public event EventHandler OnRequestRecipeIngredients;
        public event EventHandler OnSetTransferFile;

        public void requestRecipeIngredients(string recipeRecord, string callback) {
            OnRequestRecipeIngredients?.Invoke(this, new RequestRecipeArgument {
                RecipeRecord = recipeRecord,
                Callback = callback
            });
        }

        public void requestRecipeList(string callback) {
            OnRequestRecipeList?.Invoke(this, new RequestRecipeArgument {
                Callback = callback
            });
        }

        public void setTransferFile(string file) {
            OnSetTransferFile?.Invoke(this, new SetTransferFile {
                Filename = file
            });
        }

        public string TransferFiles => Serialize(GlobalPaths.TransferFiles);


        public JSWrapper() {
            _settings = new JsonSerializerSettings {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Culture = System.Globalization.CultureInfo.InvariantCulture,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
        }

        public string Serialize(object o) {
            return JsonConvert.SerializeObject(o, _settings);
        }

        public void OpenURL(string url) {
            System.Diagnostics.Process.Start(url);
        }

    }
}