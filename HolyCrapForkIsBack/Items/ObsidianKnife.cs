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
        public override string name => prefix + "OBSIDIAN_KNIFE";
        public override ItemTag[] itemTags => new ItemTag[1] { ItemTag.Damage };

        public override bool canRemove => false;
        public override bool hidden => false;
        public override bool dlcRequired => false;

        protected float critDamageBonus = 0.5f;
        protected float hemoDotChance = 10;

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
        public override string loreDefault => "\"Here it is, your excellence, the sharpest tool of this world.\"\n" +
                                              "\n" +
                                              "\"A knife, forged in the hardest material, capable of cutting flesh like air.\"\n" +
                                              "\n" +
                                              "\"I present this to you, as my gift, as a way to honor you.\"\n" +
                                              "\n" +
                                              "“Perfect. Bring the offerings, we shall celebrate this gift in her name.”\n" +
                                              "\n" +
                                              "\"As you wish my lord.\"";
        #endregion

        public override void Init(ConfigFile config)
        {
            gKnifeItemDef = InitializeItemDef();
            displayRules = new ItemDisplayRuleDict(null);

            gKnifeItemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier3Def.asset").WaitForCompletion();
            gKnifeItemDef.pickupIconSprite = Assets.mainAssetBundle.LoadAsset<Sprite>("Assets/Import/Items/icons/obs_knife.png");
            gKnifeItemDef.pickupModelPrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("Assets/Import/Items/models/obs_knife/ObsKnife.prefab");
            HopooShaderToMaterial.Standard.Apply(gKnifeItemDef.pickupModelPrefab.GetComponentInChildren<Renderer>().sharedMaterial);
            HopooShaderToMaterial.Standard.Emission(gKnifeItemDef.pickupModelPrefab.GetComponentInChildren<Renderer>().sharedMaterial, 0.001f, Color.white);
            HopooShaderToMaterial.Standard.Gloss(gKnifeItemDef.pickupModelPrefab.GetComponentInChildren<Renderer>().sharedMaterial, 0.2f, 5f, Color.white);

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

            if (attackerBody != null && attackerBody.inventory && damageInfo.crit)
            {
                var grabCount = attackerBody.inventory.GetItemCount(gKnifeItemDef.itemIndex);

                if (grabCount > 0)
                {
                    var dotChance = hemoDotChance * grabCount;

                    if (Util.CheckRoll(dotChance, attackerBody.master))
                    {
                        damageInfo.damageType = DamageType.SuperBleedOnCrit;
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
