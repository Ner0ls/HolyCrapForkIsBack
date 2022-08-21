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
        public override string name => prefix + "MOLTEN_FORK";
        public override ItemTag[] itemTags => new ItemTag[1] { ItemTag.Damage };

        public override bool canRemove => false;
        public override bool hidden => false;
        public override bool dlcRequired => false;

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
        public override string loreDefault => "\"I present to you an ancient relic of the gods!\" I saw my partner as he held a shiny little thing on his hand\n" +
                                              "\n" +
                                              "\"That just looks like a regular fork with a rare color\". I said as I tried to manipulate the object.\n" +
                                              "\n" +
                                              "\"It feels really warm though, maybe we could use it as a heat source.\" I added, the instrument slipped from my hand, landing on the floor and creating a bright spark of fire." +
                                              "\n" +
                                              "\"THAT IS COOL AS HECK! We should keep it!\" He exclaimed full of excitement.\n" +
                                              "\n" +
                                              "\"How did that even work? Is it reactive to friction?\" I wondered." +
                                              "\n" +
                                              "\"I don’t know man, but if we don’t take this incredible device someone else will do it.\" He added, as he tried to recover the instrument from the floor.\n" +
                                              "\n" +
                                              "\"Alright, just make sure you don’t end up on fire.\" I told him, as I chuckled a little\n" +
                                              "\n" +
                                              "\"Deal.\" He replied.";
        #endregion

        public override void Init(ConfigFile config)
        {
            gForkItemDef = InitializeItemDef();
            displayRules = new ItemDisplayRuleDict(null);

            gForkItemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier3Def.asset").WaitForCompletion();
            gForkItemDef.pickupIconSprite = Assets.mainAssetBundle.LoadAsset<Sprite>("Assets/Import/Items/icons/molt_fork.png");
            gForkItemDef.pickupModelPrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("Assets/Import/Items/models/molt_fork/MoltFork.prefab");

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
