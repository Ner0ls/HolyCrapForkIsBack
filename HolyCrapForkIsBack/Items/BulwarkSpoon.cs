using BepInEx.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using R2API;
using UnityEngine.AddressableAssets;

namespace HolyCrapForkIsBack.Items
{
    public class BulwarkSpoon : ItemBase<BulwarkSpoon>
    {
        public static ItemDef gSpoonItemDef;

        public override string name => prefix + "BULWARK_SPOON";
        public override ItemTag[] itemTags => new ItemTag[2] { ItemTag.OnKillEffect, ItemTag.Utility };

        public override bool canRemove => false;
        public override bool hidden => false;
        public override bool dlcRequired => false;

        protected float barrierOnKill = 30f;
        protected float damageBonus = 0.20f;
        protected float armorBonus = 30f;
        protected float cdrBonus = 0.20f;
        protected float cdrPerStack = 0.10f;

        #region LanguageTokens
        public override string nameToken => prefix + "BULWARK_SPOON_NAME";
        public override string pickupToken => prefix + "BULWARK_SPOON_PICKUP";
        public override string descToken => prefix + "BULWARK_SPOON_DESC";
        public override string loreToken => prefix + "BULWARK_SPOON_LORE";
        #endregion

        #region DefaultLanguage
        public override string nameDefault => "Bulwark Spoon";
        public override string pickupDefault => "Gain barrier on kill.\nIncreases your stats in relation of your current barrier value.";
        public override string descDefault => $"Gain {barrierOnKill} barrier on kill.\n" +
            $"Increases your damage by {damageBonus * 100}% <style=cStack>(+{damageBonus * 100}% per stack)</style>, your armor by {armorBonus} <style=cStack>(+{armorBonus} per stack)</style> " +
            $"and reduces your cooldowns by {cdrBonus * 100}% <style=cStack>(+{cdrPerStack * 100}% per stack)</style> in relation of the current percent of barrier you have over your combined health.";
        public override string loreDefault => "For all my followers.\n" +
                                              "\n" +
                                              "This is a piece of myself.\n" +
                                              "\n" +
                                              "This is the way to show your devotion.\n" +
                                              "\n" +
                                              "This is our bond, our connection.\n" +
                                              "\n" +
                                              "This is our emblem, our identity.\n" +
                                              "\n" +
                                              "This is my will.\n" +
                                              "\n" +
                                              "This… is… the reminder.\n";
        #endregion

        public override void Init(ConfigFile config)
        {
            gSpoonItemDef = InitializeItemDef();
            displayRules = new ItemDisplayRuleDict(null);

            gSpoonItemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier3Def.asset").WaitForCompletion();
            gSpoonItemDef.pickupIconSprite = Assets.mainAssetBundle.LoadAsset<Sprite>("Assets/Import/Items/icons/bul_spoon.png");
            gSpoonItemDef.pickupModelPrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("Assets/Import/Items/models/bul_spoon/BulSpoon.prefab");
            HopooShaderToMaterial.Standard.Apply(gSpoonItemDef.pickupModelPrefab.GetComponentInChildren<Renderer>().sharedMaterial);
            HopooShaderToMaterial.Standard.Gloss(gSpoonItemDef.pickupModelPrefab.GetComponentInChildren<Renderer>().sharedMaterial, 0.1f, 1f, Color.white);

            SetupLanguageTokens();
            SetupHooks();
            
            ItemAPI.Add(new CustomItem(gSpoonItemDef, displayRules));
        }

        protected override void SetupHooks()
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;

        }

        private void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            //If a character was killed by the world, we shouldn't do anything.
            if (!damageReport.attacker || !damageReport.attackerBody)
            {
                return;
            }

            CharacterBody characterBody = damageReport.attackerBody;

            if (characterBody.inventory)
            {
                var grabCount = characterBody.inventory.GetItemCount(gSpoonItemDef.itemIndex);

                if (grabCount > 0)
                {
                    characterBody.healthComponent.AddBarrier(barrierOnKill);
                }
            }

            orig.Invoke(self, damageReport);
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody characterBody, RecalculateStatsAPI.StatHookEventArgs args)
        {
            //We need an inventory to do check for our item
            if (characterBody.inventory)
            {
                //store the amount of our item we have
                var grabCount = characterBody.inventory.GetItemCount(gSpoonItemDef.itemIndex);

                if (grabCount > 0)
                {
                    var maxBarrier = characterBody.maxBarrier; // 100% Barrier
                    var currentBarrierAmount = characterBody.healthComponent.barrier;
                    var currentBarrierPercent = currentBarrierAmount / maxBarrier;
                    var maxCDRBonus = cdrBonus + (cdrPerStack * grabCount);

                    args.damageMultAdd += damageBonus * currentBarrierPercent * grabCount;
                    args.armorAdd += armorBonus * currentBarrierPercent * grabCount;
                    args.cooldownMultAdd -= maxCDRBonus * currentBarrierPercent;
                }
            }
        }
    }
}
