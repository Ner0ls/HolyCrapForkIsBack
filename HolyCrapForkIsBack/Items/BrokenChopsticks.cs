using BepInEx.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using R2API;
using UnityEngine.AddressableAssets;

namespace HolyCrapForkIsBack.Items
{
    public class BrokenChopsticks : ItemBase<BrokenChopsticks>
    {
        public static ItemDef brokenChopsticksItemDef;
        public override bool forceEnable => true;

        public override string name => prefix + "BROKEN_CHOPSTICKS";
        public override ItemTag[] itemTags => new ItemTag[] { };

        public override bool canRemove => false;
        public override bool hidden => false;
        public override bool dlcRequired => false;


        #region LanguageTokens
        public override string nameToken => prefix + "BROKEN_CHOPSTICKS_NAME";
        public override string pickupToken => prefix + "BROKEN_CHOPSTICKS_PICKUP";
        public override string descToken => prefix + "BROKEN_CHOPSTICKS_DESC";
        public override string loreToken => prefix + "BROKEN_CHOPSTICKS_LORE";
        #endregion

        #region DefaultLanguage
        public override string nameDefault => "Broken Chopsticks";
        public override string pickupDefault => "I told you it was a bad idea...";
        public override string descDefault => $"I told you it was a bad idea...";
        public override string loreDefault => "";
        #endregion

        public override void Init(ConfigFile config)
        {
            brokenChopsticksItemDef = InitializeItemDef();
            displayRules = new ItemDisplayRuleDict(null);

            //brokenChopsticksItemDef._itemTierDef = null;
            brokenChopsticksItemDef.deprecatedTier = ItemTier.NoTier;
            brokenChopsticksItemDef.pickupIconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            brokenChopsticksItemDef.pickupModelPrefab = Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");
            
            SetupLanguageTokens();
            
            ItemAPI.Add(new CustomItem(brokenChopsticksItemDef, displayRules));
        }
    }
}
