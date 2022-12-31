﻿using BepInEx.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using R2API;
using UnityEngine.AddressableAssets;

namespace HolyCrapForkIsBack.Items
{
    public class Knife : ItemBase<Knife>
    {
        public static ItemDef knifeItemDef;

        public override string name => prefix + "KNIFE";
        public override ItemTag[] itemTags => new ItemTag[1] { ItemTag.Damage };

        public override bool canRemove => false;
        public override bool hidden => false;
        public override bool dlcRequired => false;

        protected float critChanceBonus = 5f;
        protected float critDamageBonus = 0.05f;

        #region LanguageTokens
        public override string nameToken => prefix + "KNIFE_NAME";
        public override string pickupToken => prefix + "KNIFE_PICKUP";
        public override string descToken => prefix + "KNIFE_DESC";
        public override string loreToken => prefix + "KNIFE_LORE";
        #endregion

        #region DefaultLanguage
        public override string nameDefault => "Knife";
        public override string pickupDefault => "Slightly boosts Critical Damage and Critical Chance.";
        public override string descDefault => $"Gain {critDamageBonus * 100}% <style=cIsDamage>Critical Damage</style> <style=cStack>(+{critDamageBonus * 100}% per stack)</style> and {critChanceBonus}% <style=cIsDamage>Critical Chance</style> <style=cStack>(+{critChanceBonus}% per stack)</style>.";
        public override string loreDefault => "I was being attacked, dissarmed, we were both at our physical limits. I looked for the closest thing to me, a kitchen knife was laying on the floor...\n" +
                                                "\n" +
                                                "\"Hmph, who would've imagined that I would use cutlery in a situation like this.\"\n I thought as I slowly crawled to grab it." +
                                                "\n" +
                                                "The attacker was relentless, he wanted to see my face while he stealed my last breath, in a flash, I stabbed him, right through his heart.\n" +
                                                "\n" +
                                                "He collapsed to the ground as he looked me dead in the eyes. I knew the face, but not the being...\n" +
                                                "\n" +
                                                "\"That's what you get, you sneaky bastard, you took one precious life already, so I took yours.\" I exclaimed, exhausted.\n" +
                                                "\n" +
                                                "He was done for. A small little creature crawled from the dead body and escaped, really fast.\n" +
                                                "\n" +
                                                "\"How long it will take...\" I thought, as I sitted on the ground.\n" +
                                                "\n" +
                                                "\"I knew we wouldn't...\" I passed out.";
        #endregion

        public override void Init(ConfigFile config)
        {
            knifeItemDef = InitializeItemDef();
            displayRules = new ItemDisplayRuleDict(null);

            knifeItemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier1Def.asset").WaitForCompletion();
            knifeItemDef.pickupIconSprite = Assets.mainAssetBundle.LoadAsset<Sprite>("Assets/Import/Items/icons/knife.png");
            knifeItemDef.pickupModelPrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("Assets/Import/Items/models/knife/Knife.prefab");
            HopooShaderToMaterial.Standard.Apply(knifeItemDef.pickupModelPrefab.GetComponentInChildren<Renderer>().sharedMaterial);
            HopooShaderToMaterial.Standard.Gloss(knifeItemDef.pickupModelPrefab.GetComponentInChildren<Renderer>().sharedMaterial, 0.05f, 10f, Color.white);

            SetupLanguageTokens();
            SetupHooks();
            
            ItemAPI.Add(new CustomItem(knifeItemDef, displayRules));
        }

        protected override void SetupHooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody characterBody, RecalculateStatsAPI.StatHookEventArgs args)
        {
            //We need an inventory to do check for our item
            if (characterBody.inventory)
            {
                //store the amount of our item we have
                var grabCount = characterBody.inventory.GetItemCount(knifeItemDef.itemIndex);
                if (grabCount > 0)
                {
                    args.critAdd += (critChanceBonus * grabCount);
                    args.critDamageMultAdd += (critDamageBonus * grabCount);
                }
            }
        }
    }
}
