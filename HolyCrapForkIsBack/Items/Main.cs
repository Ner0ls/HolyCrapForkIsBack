using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using RoR2.ExpansionManagement;

namespace HolyCrapForkIsBack.Items
{
    class Main
    {
        public static void InitializeItems()
        {
            var itemsClasses = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ItemBase)));
            foreach (var item in itemsClasses)
            {
                ItemBase hcfbItem = (ItemBase)Activator.CreateInstance(item);
                //if (!hcfbItem.enabled.Value) { continue; }
                //PlugInChips.instance.Logger.LogMessage("Initializing Items...");

                //item.Init(HolyCrapForkIsBack.Main.instance.Config);
                if (hcfbItem.disabled) continue;
                Log.LogInfo("Initializing item: " + hcfbItem.name);
                hcfbItem.Init();

                //From bubbet's itembase
                //if (chipsItem.dlcRequired)
                //{
                //    chipsItem.itemDef.requiredExpansion = ExpansionCatalog.expansionDefs.FirstOrDefault(x => x.nameToken == "DLC1_NAME");
                //}
            }
        }
    }
}
