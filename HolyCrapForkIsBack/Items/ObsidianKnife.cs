using BepInEx.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using R2API;
using UnityEngine.AddressableAssets;

namespace HolyCrapForkIsBack.Items
{
    public class ObsidianKnife : ItemBase<ObsidianKnife>
    {
        public static ItemDef gKnifeItemDef;
        public override bool disabled => false;

        public override string name => prefix + "OBSIDIAN_KNIFE";
        public override ItemTag[] itemTags => new ItemTag[1] { ItemTag.Damage };

        public override bool canRemove => false;
        public override bool hidden => false;

        protected float critDamageBonus = 0.5f;
        protected float hemoDotChance = 30;

        #region LanguageTokens
        public override string nameToken => prefix + "OBSIDIAN_KNIFE_NAME";
        public override string pickupToken => prefix + "OBSIDIAN_KNIFE_PICKUP";
        public override string descToken => prefix + "OBSIDIAN_KNIFE_DESC";
        public override string loreToken => prefix + "OBSIDIAN_KNIFE_LORE";
        #endregion

        #region DefaultLanguage
        public override string nameDefault => "Obsidian Knife";
        public override string pickupDefault => "Increases critical damage, critical hits have a chance to apply hemorrhage.";
        public override string descDefault => $"Gain {critDamageBonus * 100}% Critical Damage <style=cStack>(+{critDamageBonus * 100}% per stack)</style>, critical hits have a {hemoDotChance}% chance (+{hemoDotChance}% per stack) to apply Hemorrhage.";
        public override string loreDefault => "TBD";
        #endregion

        public override void Init()
        {
            gKnifeItemDef = InitializeItemDef();
            displayRules = new ItemDisplayRuleDict(null);

            gKnifeItemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier3Def.asset").WaitForCompletion();
            //gKnifeItemDef.pickupIconSprite = Assets.mainAssetBundle.LoadAsset<Sprite>("Assets/Import/Items/icons/knife.png");
            //gKnifeItemDef.pickupModelPrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("Assets/Import/Items/models/knife/Knife.prefab");

            SetupLanguageTokens();
            SetupHooks();
            
            ItemAPI.Add(new CustomItem(gKnifeItemDef, displayRules));
        }

        protected override void SetupHooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            if (!damageInfo.attacker)
            {
                orig.Invoke(self, damageInfo, victim);
                return;
            }

            var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();

            if (attackerBody && attackerBody.inventory && damageInfo.crit)
            {
                var grabCount = attackerBody.inventory.GetItemCount(gKnifeItemDef);

                if (grabCount > 0)
                {
                    var dotChance = hemoDotChance * grabCount;

                    if (Util.CheckRoll(dotChance, attackerBody.master))
                    {
                        InflictDotInfo inflictDotInfo = new InflictDotInfo
                        {
                            attackerObject = damageInfo.attacker,
                            victimObject = victim,
                            totalDamage = damageInfo.damage,
                            damageMultiplier = 1f,
                            dotIndex = DotController.DotIndex.SuperBleed
                        };

                        DotController.InflictDot(ref inflictDotInfo);
                    }
                }
            }

            orig.Invoke(self, damageInfo, victim);
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody characterBody, RecalculateStatsAPI.StatHookEventArgs args)
        {
            //We need an inventory to do check for our item
            if (characterBody.inventory)
            {
                //store the amount of our item we have
                var grabCount = characterBody.inventory.GetItemCount(gKnifeItemDef.itemIndex);
                if (grabCount > 0)
                {
                    args.critDamageMultAdd += (critDamageBonus * grabCount);
                }
            }
        }
    }
}
