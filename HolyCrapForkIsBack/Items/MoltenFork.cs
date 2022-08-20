using BepInEx.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using R2API;
using UnityEngine.AddressableAssets;

namespace HolyCrapForkIsBack.Items
{
    public class MoltenFork : ItemBase<MoltenFork>
    {
        public static ItemDef gForkItemDef;
        public override bool disabled => false;

        public override string name => prefix + "MOLTEN_FORK";
        public override ItemTag[] itemTags => new ItemTag[1] { ItemTag.Damage };

        public override bool canRemove => false;
        public override bool hidden => false;

        protected float damageBonus = 2f;
        protected float igniteChance = 5;

        #region LanguageTokens
        public override string nameToken => prefix + "MOLTEN_FORK_NAME";
        public override string pickupToken => prefix + "MOLTEN_FORK_PICKUP";
        public override string descToken => prefix + "MOLTEN_FORK_DESC";
        public override string loreToken => prefix + "MOLTEN_FORK_LORE";
        #endregion

        #region DefaultLanguage
        public override string nameDefault => "Molten Fork";
        public override string pickupDefault => "Boosts damage against ignited enemies, chance to ignite on hit.";
        public override string descDefault => $"Multiply your damage by {damageBonus} <style=cStack>(+1 to multiplier per stack)</style> against ignited enemies, {igniteChance}% of igniting enemies on hit.";
        public override string loreDefault => "TBD";
        #endregion

        public override void Init()
        {
            gForkItemDef = InitializeItemDef();
            displayRules = new ItemDisplayRuleDict(null);

            gForkItemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier3Def.asset").WaitForCompletion();
            //gKnifeItemDef.pickupIconSprite = Assets.mainAssetBundle.LoadAsset<Sprite>("Assets/Import/Items/icons/knife.png");
            //gKnifeItemDef.pickupModelPrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("Assets/Import/Items/models/knife/Knife.prefab");

            SetupLanguageTokens();
            SetupHooks();

            ItemAPI.Add(new CustomItem(gForkItemDef, displayRules));
        }

        protected override void SetupHooks()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            if (!damageInfo.attacker)
            {
                orig.Invoke(self, damageInfo, victim);
                return;
            }

            var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();

            if (attackerBody && attackerBody.inventory)
            {
                var grabCount = attackerBody.inventory.GetItemCount(gForkItemDef);

                if (grabCount > 0)
                {
                    if (Util.CheckRoll(igniteChance, attackerBody.master))
                    {
                        InflictDotInfo inflictDotInfo = new InflictDotInfo
                        {
                            attackerObject = damageInfo.attacker,
                            victimObject = victim,
                            totalDamage = damageInfo.damage,
                            damageMultiplier = 1f,
                            dotIndex = DotController.DotIndex.Burn
                        };

                        StrengthenBurnUtils.CheckDotForUpgrade(attackerBody.inventory, ref inflictDotInfo);
                        DotController.InflictDot(ref inflictDotInfo);
                    }
                }
            }

            orig.Invoke(self, damageInfo, victim);
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (!damageInfo.attacker)
            {
                orig.Invoke(self, damageInfo);

                return;
            }
            var attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
            
            if (attackerBody != null && attackerBody.inventory)
            {
                var grabCount = attackerBody.inventory.GetItemCount(gForkItemDef);

                if (grabCount > 0)
                {
                    var attackedBody = self.body;
                    var onFireBuffDef = Addressables.LoadAssetAsync<BuffDef>("RoR2/Base/Common/bdOnFire.asset").WaitForCompletion();
                    var onSFireBuffDef = Addressables.LoadAssetAsync<BuffDef>("RoR2/DLC1/StrengthenBurn/bdStrongerBurn.asset").WaitForCompletion();

                    if (attackedBody.HasBuff(onFireBuffDef) || attackedBody.HasBuff(onSFireBuffDef))
                    {
                        damageInfo.damage *= damageBonus * grabCount;
                    }
                }
            }

            orig.Invoke(self, damageInfo);
        }
    }
}
