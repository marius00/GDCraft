using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IAGrim.Database.Interfaces;
using IAGrim.Parsers.Arz;
using IAGrim.Services.Crafting;
using IAGrim.UI.Misc;
using IAGrim.Utilities;
using log4net;

namespace IAGrim.Services {
    class JsonBindingService {
        private readonly RecipeService _recipeService;
        private readonly CostCalculationService _costCalculationService;
        private readonly string _previousMod = string.Empty;
        private readonly JSWrapper _jsBind;
        private readonly StashManager _stashManager;
        private readonly CefBrowserHandler _browser;

        private string _previousRecipe;
        private string _previousCallback;
        



        public JsonBindingService(StashManager stashManager, JSWrapper jsBind, CefBrowserHandler browser, RecipeService recipeService, CostCalculationService costCalculationService) {
            _stashManager = stashManager;
            this._jsBind = jsBind;
            _browser = browser;
            _recipeService = recipeService;
            _costCalculationService = costCalculationService;

            // 

            // Return the ingredients for a given recipe
            jsBind.OnRequestRecipeIngredients += (sender, args) => {
                var recipeArgument = args as RequestRecipeArgument;
                var ingredients = _recipeService.GetRecipeIngredients(recipeArgument?.RecipeRecord);
                _costCalculationService.Populate(ingredients);
                _costCalculationService.SetMod(_previousMod);

                _previousCallback = recipeArgument?.Callback;
                _previousRecipe = recipeArgument?.RecipeRecord;
                _browser.JsCallback(recipeArgument?.Callback, jsBind.Serialize(ingredients));
            };

            jsBind.OnSetTransferFile += (sender, args) => {
                var arg = args as SetTransferFile;
                _stashManager.SetTransferFile(arg?.Filename);

            };


            // Update the recipe when the stash has changed
            stashManager.StashUpdated += StashManagerOnStashUpdated;


            // Return the list of recipes
            jsBind.OnRequestRecipeList += (sender, args) => {
                var recipeArgument = args as RequestRecipeArgument;
                var recipes = _recipeService.GetRecipeList();
                _browser.JsCallback(recipeArgument?.Callback, jsBind.Serialize(recipes));
            };
        }


        private void StashManagerOnStashUpdated(object o, EventArgs eventArgs) {
            if (!string.IsNullOrEmpty(_previousRecipe)) {
                var ingredients = _recipeService.GetRecipeIngredients(_previousRecipe);
                _costCalculationService.Populate(ingredients);
                _costCalculationService.SetMod(_previousMod);
                _browser.JsCallback(_previousCallback, _jsBind.Serialize(ingredients));
            }
        }
    }
}
