﻿using IAGrim.UI.Controller;
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

        public JSWrapper() {
            _settings = new JsonSerializerSettings {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Culture = System.Globalization.CultureInfo.InvariantCulture,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            var lang = GlobalSettings.Language;
            translation = new HtmlTranslation {
                iatag_html_any = lang.GetTag("iatag_html_any"),
                iatag_html_badstate_subtitle = lang.GetTag("iatag_html_badstate_subtitle"),
                iatag_html_badstate_title = lang.GetTag("iatag_html_badstate_title"),
                iatag_html_bonustopets = lang.GetTag("iatag_html_bonustopets"),
                iatag_html_copytoclipboard = lang.GetTag("iatag_html_copytoclipboard"),
                iatag_html_crafting_lacking = lang.GetTag("iatag_html_crafting_lacking"),
                iatag_html_items_affix2 = lang.GetTag("iatag_html_items_affix2"),
                iatag_html_items_affix3 = lang.GetTag("iatag_html_items_affix3"),
                iatag_html_items_buddy_alsohasthisitem1 = lang.GetTag("iatag_html_items_buddy_alsohasthisitem1"),
                iatag_html_items_buddy_alsohasthisitem2 = lang.GetTag("iatag_html_items_buddy_alsohasthisitem2"),
                iatag_html_items_buddy_alsohasthisitem3 = lang.GetTag("iatag_html_items_buddy_alsohasthisitem3"),
                iatag_html_items_buddy_alsohasthisitem4 = lang.GetTag("iatag_html_items_buddy_alsohasthisitem4"),
                iatag_html_items_grantsskill = lang.GetTag("iatag_html_items_grantsskill"),
                iatag_html_items_level = lang.GetTag("iatag_html_items_level"),
                iatag_html_items_no_items = lang.GetTag("iatag_html_items_no_items"),
                iatag_html_items_unknown = lang.GetTag("iatag_html_items_unknown"),
                iatag_html_items_youcancraftthisitem = lang.GetTag("iatag_html_items_youcancraftthisitem"),
                iatag_html_levlerequirement = lang.GetTag("iatag_html_levlerequirement"),
                iatag_html_tab_header_components = lang.GetTag("iatag_html_tab_header_components"),
                iatag_html_tab_header_crafting = lang.GetTag("iatag_html_tab_header_crafting"),
                iatag_html_tab_header_discord = lang.GetTag("iatag_html_tab_header_discord"),
                iatag_html_tab_header_items = lang.GetTag("iatag_html_tab_header_items"),
                iatag_html_transfer = lang.GetTag("iatag_html_transfer"),
                iatag_html_transferall = lang.GetTag("iatag_html_transferall"),
                iatag_html_badstate_close = lang.GetTag("iatag_html_badstate_close"),
                iatag_html_choose_a_relic = lang.GetTag("iatag_html_choose_a_relic"),
                iatag_html_choose_a_component = lang.GetTag("iatag_html_choose_a_component"),
                iatag_html_choose_a_recipe = lang.GetTag("iatag_html_choose_a_recipe")
            };
        }

        public string Serialize(object o) {
            return JsonConvert.SerializeObject(o, _settings);
        }

        public HtmlTranslation translation { get; private set; }


        public void OpenURL(string url) {
            System.Diagnostics.Process.Start(url);
        }

    }
}