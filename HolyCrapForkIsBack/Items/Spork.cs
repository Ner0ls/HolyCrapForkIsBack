using BepInEx.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using R2API;
using UnityEngine.AddressableAssets;
using VoidItemAPI;

namespace HolyCrapForkIsBack.Items
{
    public class Spork : ItemBase<Spork>
    {
        public static ItemDef sporkItemDef;
        public override bool disabled => false;

        public override string name => prefix + "SPORK";
        public override ItemTag[] itemTags => new ItemTag[3] { ItemTag.Damage, ItemTag.OnKillEffect,  ItemTag.OnStageBeginEffect};
        public override bool canRemove => false;
        public override bool hidden => false;

        public float cooldownBonusStacks = 0f;
        public float cooldownBonusPerKill = 0.0005f;
        public float maxStacks = 100;
        public float cooldownBonusCap;
        public BuffDef stackBuff { get; private set; }
        #region LanguageTokens
        public override string nameToken => prefix + "SPORK_NAME";
        public override string pickupToken => prefix + "SPORK_PICKUP";
        public override string descToken => prefix + "SPORK_DESC";
        public override string loreToken => prefix + "SPORK_LORE";
        #endregion

        #region DefaultLanguage
        public override string nameDefault => "Spork";
        public override string pickupDefault => "Gain cooldown reduction per kill, resets every stage or if you die. <style=cIsVoid>Corrupts all Spoons</style>";
        public override string descDefault => $"Gain +{cooldownBonusPerKill * 100}% <style=cStack>(+{cooldownBonusPerKill * 100}% per stack)</style> cooldown reduction each time you <style=cIsDamage>kill an enemy</style> with a max cap of {cooldownBonusCap * 100}% <style=cStack>(+{cooldownBonusCap * 100}% per stack)</style>, buff's max stacks is {maxStacks}, resets every stage or if you die. <style=cIsVoid>Corrupts all Spoons</style>";

        public override string loreDefault => "\"Oh wow this one looks freaky\" He said.\n" +
            "\n" +
            "\"What is that? Don’t touch it!\" I shouted from the distance.\n" +
            "\n" +
            "\"Too late...\" He told me as I saw him holding an strange object with a purple shine.\n" +
            "\n" +
            "\"Hm, it did nothing.\" He added\n" +
            "\n" +
            "\"You sure? What if it is radioactive or some stuff?\"\n" +
            "\n" +
            "\"You're overthinking it.\" He said, as he packed the mysterious object in his bag.\n" +
            "\n" +
            "A sudden chill went right down my spine as we continued our journey...";
        #endregion

        public override void Init()
        {
            cooldownBonusCap = maxStacks * cooldownBonusPerKill;
            sporkItemDef = InitializeItemDef();
            displayRules = new ItemDisplayRuleDict(null);

            //sporkItemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/VoidTier1Def.asset").WaitForCompletion();
            sporkItemDef.deprecatedTier = ItemTier.VoidTier1;
            sporkItemDef.pickupIconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            sporkItemDef.pickupModelPrefab = Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

            CreateBuff();
            SetupLanguageTokens();
            SetupHooks();

            ItemAPI.Add(new CustomItem(sporkItemDef, displayRules));

            VoidTransformation.CreateTransformation(sporkItemDef, ItemBase<Spoon>.instance.itemDef);
        }

        public void CreateBuff()
        {
            stackBuff = ScriptableObject.CreateInstance<BuffDef>();
            stackBuff.buffColor = Color.magenta;
            stackBuff.canStack = true;
            stackBuff.isDebuff = false;
            stackBuff.isHidden = false;
            stackBuff.name = prefix + "SPORK_STACK_BUFF";
            stackBuff.iconSprite = Addressables.LoadAsset<Sprite>("RoR2/Base/Common/bdVoidFogStrong.asset").WaitForCompletion();

            ContentAddition.AddBuffDef(stackBuff);
        }

        protected override void SetupHooks()
        {
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            Stage.onStageStartGlobal += Stage_onStageStartGlobal;
            //CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
        }

        private void Stage_onStageStartGlobal(Stage obj)
        {
            cooldownBonusStacks = 0;
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

            if (playerBody.inventory)
            {
                var grabCount = playerBody.inventory.GetItemCount(sporkItemDef.itemIndex);

                if (grabCount > 0)
                {
                    if (cooldownBonusStacks < maxStacks)
                    {
                        playerBody.AddBuff(stackBuff);
                        cooldownBonusStacks++;
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
                var grabCount = playerBody.inventory.GetItemCount(sporkItemDef.itemIndex);

                if (grabCount >= 0)
                {
                    args.cooldownMultAdd -= cooldownBonusPerKill * cooldownBonusStacks * grabCount;
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
                    cooldownBonusStacks = 0;
                }
            }
        }
    }
}
