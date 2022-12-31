using BepInEx;
using BepInEx.Configuration;
using HolyCrapForkIsBack.Items;
using R2API;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using VoidItemAPI;
using UnityEngine;
using RoR2.ExpansionManagement;

namespace HolyCrapForkIsBack
{
    //This is an example plugin that can be put in BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
    //It's a small plugin that adds a relatively simple item to the game, and gives you that item whenever you press F2.

    //This attribute specifies that we have a dependency on R2API, as we're using it to add our item to the game.
    //You don't need this if you're not using R2API in your plugin, it's just to tell BepInEx to initialize R2API before this plugin so it's safe to use R2API.
    [BepInDependency(R2API.R2API.PluginGUID)]
    
    [BepInDependency(VoidItemAPI.VoidItemAPI.MODGUID)]

    //This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    //We will be using 2 modules from R2API: ItemAPI to add our item and LanguageAPI to add our language tokens.
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI), nameof(RecalculateStatsAPI))]

    //This is the main declaration of our plugin class. BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    //BaseUnityPlugin itself inherits from MonoBehaviour, so you can use this as a reference for what you can declare and use in your plugin class: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class Main : BaseUnityPlugin
    {
        //The Plugin GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config).
        //If we see this PluginGUID as it is on thunderstore, we will deprecate this mod. Change the PluginAuthor and the PluginName !
        public const string PluginGUID = "com." + PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Ner0ls";
        public const string PluginName = "HolyCrapForkIsBack";
        public const string PluginVersion = "1.0.3";
        public static PluginInfo PInfo { get; private set; }

        public List<ItemBase> Items = new List<ItemBase>();

        //The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            PInfo = Info;
            //Init our logging class so that we can properly log for debugging
            Log.Init(Logger);

            Assets.Init();
            Log.LogInfo("Trying to initialize items.");

            var itemsClasses = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ItemBase)));

            foreach (var item in itemsClasses)
            {
                ItemBase hcfbItem = (ItemBase)Activator.CreateInstance(item);

                if (hcfbItem.forceEnable || ValidateItem(hcfbItem, Items))
                {
                    Log.LogInfo("Initializing item: " + hcfbItem.name);
                    hcfbItem.Init(Config);

                    //From bubbet's itembase
                    if (hcfbItem.dlcRequired)
                    {
                        hcfbItem.itemDef.requiredExpansion = ExpansionCatalog.expansionDefs.FirstOrDefault(x => x.nameToken == "DLC1_NAME");
                    }
                }

            }

            //But now we have defined an item, but it doesn't do anything yet. So we'll need to define that ourselves.

            // This line of log will appear in the bepinex console when the Awake method is done.
            Log.LogInfo(nameof(Awake) + " done.");
        }

        public bool ValidateItem(ItemBase item, List<ItemBase> itemList)
        {
            var enabled = Config.Bind<bool>("Item: " + item.nameDefault, "Enable Item?", true, "Should this item appear in runs?").Value;
            var aiBlacklist = Config.Bind<bool>("Item: " + item.nameDefault, "Blacklist Item from AI Use?", false, "Should the AI not be able to obtain this item?").Value;
            if (enabled)
            {
                itemList.Add(item);
                if (aiBlacklist)
                {
                    item.AIBlacklisted = true;
                }
            }
            return enabled;
        }
    }
}
