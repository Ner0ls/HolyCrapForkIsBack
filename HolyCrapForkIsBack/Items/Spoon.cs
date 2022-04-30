using BepInEx.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using R2API;
using UnityEngine.AddressableAssets;

namespace HolyCrapForkIsBack.Items
{
    public class Spoon : ItemBase<Spoon>
    {
        public static ItemDef spoonItemDef;
        public override bool disabled => false;

        public override string name => prefix + "SPOON";
        public override ItemTag[] itemTags => new ItemTag[2] { ItemTag.Damage, ItemTag.OnKillEffect };

        public override bool canRemove => false;
        public override bool hidden => false;

        public float constantDamageBonus = 2f;
        public float damageBonusStacks = 0f;
        public float damageBonusPerKill = 0.01f;
        public float damageBonusCap = 1f;
        public float levelScalingF = 0.20f;
        public BuffDef stackBuff { get; set; }

        #region LanguageTokens
        public override string nameToken => prefix + "SPOON_NAME";
        public override string pickupToken => prefix + "SPOON_PICKUP";
        public override string descToken => prefix + "SPOON_DESC";
        public override string loreToken => prefix + "SPOON_LORE";
        #endregion

        #region DefaultLanguage
        public override string nameDefault => "Spoon";
        public override string pickupDefault => "Do more damage, stack more damage each time you kill an enemy.";
        public override string descDefault => $"Gain +{constantDamageBonus} <style=cStack>(does not scale)</style> and +{damageBonusPerKill} <style=cStack>(scales 30% with player level)</style> base damage each time you <style=cIsDamage>kill an enemy</style> with a max cap of {damageBonusCap} <style=cStack>(+{damageBonusCap} per stack)</style>.";

        public override string loreDefault => "\"Well, if you said that could be useful, why can't I try with this?\" I exclaimed.\n" +
            "\n" +
            "...\n" +
            "\n" +
            "\"After all... What if we need to dig a hole as a shelter? Or pop some monster eyes out?\" I insisted.\n" +
            "\n" +
            "\"Dig a hole with a spoon...?\" This time he looked at me with a questioning look.\n" +
            "\n" +
            "\"Yeah.\"\n" +
            "\n" +
            "...\n" +
            "\n" +
            "\"We are gonna die, aren't we?\" I said, breaking his silence.\n" +
            "\n" +
            "\"Probably...\"";
        #endregion

        public override void Init()
        {
            spoonItemDef = InitializeItemDef();
            displayRules = new ItemDisplayRuleDict(null);

            spoonItemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier1Def.asset").WaitForCompletion();
            spoonItemDef.pickupIconSprite = Assets.mainAssetBundle.LoadAsset<Sprite>("Assets/Import/Items/icons/spoon.png");
            spoonItemDef.pickupModelPrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("Assets/Import/Items/models/spoon/Spoon.prefab");

            CreateBuff();
            SetupLanguageTokens();
            SetupHooks();
            
            ItemAPI.Add(new CustomItem(spoonItemDef, displayRules));
        }

        protected override void SetupHooks()
        {
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody playerBody)
        {
            if (playerBody.inventory && playerBody.isPlayerControlled && damageBonusStacks > 0)
            {
                for (var x = 0; x < damageBonusStacks; x++)
                {
                    playerBody.AddBuff(stackBuff);
                }
            }
        }

        private void CreateBuff()
        {
            stackBuff = ScriptableObject.CreateInstance<BuffDef>();
            stackBuff.name = prefix + "SPOON_STACK_BUFF";
            stackBuff.buffColor = Color.white;
            stackBuff.canStack = true;
            stackBuff.isDebuff = false;
            stackBuff.eliteDef = null;
            stackBuff.iconSprite = Addressables.LoadAsset<Sprite>("RoR2/Base/GainArmor/bdElephantArmorBoost.asset").WaitForCompletion();

            ContentAddition.AddBuffDef(stackBuff);
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport report)
        {
            //If a character was killed by the world, we shouldn't do anything.
            if (!report.attacker || !report.attackerBody)
            {
                return;
            }

            // Else, let's pump up those stacks
            CharacterBody playerBody = report.attackerBody;

            if (playerBody.inventory && playerBody.isPlayerControlled)
            {
                var grabCount = playerBody.inventory.GetItemCount(spoonItemDef.itemIndex);

                if (grabCount > 0)
                {
                    float currentMaxStacks = (damageBonusCap / damageBonusPerKill) * grabCount;

                    if (damageBonusStacks < currentMaxStacks)
                    {
                        playerBody.AddBuff(stackBuff);
                        damageBonusStacks++;
                    }
                }
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody playerBody, RecalculateStatsAPI.StatHookEventArgs args)
        {
            //We need an inventory to do check for our item
            if (playerBody.inventory && playerBody.isPlayerControlled)
            {
                //store the amount of our item we have
                var grabCount = playerBody.inventory.GetItemCount(spoonItemDef.itemIndex);

                if (grabCount > 0)
                {
                    args.baseDamageAdd += constantDamageBonus + (damageBonusPerKill * damageBonusStacks * (1f + (levelScalingF * playerBody.level)));
                }
                else
                {
                    if (playerBody.GetBuffCount(stackBuff) > 0)
                    {
                        for (var x = 0; x < playerBody.GetBuffCount(stackBuff); x++)
                        {
                            playerBody.RemoveBuff(stackBuff);
                        }
                    }
                    damageBonusStacks = 0;
                }
            }
        }
    }
}
