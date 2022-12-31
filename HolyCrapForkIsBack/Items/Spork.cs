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
        public override string name => prefix + "SPORK";
        public override ItemTag[] itemTags => new ItemTag[4] { ItemTag.Utility, ItemTag.OnKillEffect, ItemTag.OnStageBeginEffect, ItemTag.CannotCopy };
        public override bool canRemove => false;
        public override bool hidden => false;
        public override bool dlcRequired => true;

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
        public override string descDefault => $"Gain +{cooldownBonusPerKill * 100}% <style=cIsUtility>Cooldown Reduction</style> <style=cStack>(+{cooldownBonusPerKill * 100}% per stack)</style> each time you <style=cIsDamage>kill an enemy</style> with a max cap of {cooldownBonusCap * 100}% <style=cStack>(+{cooldownBonusCap * 100}% per stack)</style>, caps at {maxStacks} stacks, resets every <style=cEvent>Stage</style> or if you <style=cDeath>Die</style>. <style=cIsVoid>Corrupts all Spoons</style>";

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

        public override void Init(ConfigFile config)
        {
            cooldownBonusCap = maxStacks * cooldownBonusPerKill;
            sporkItemDef = InitializeItemDef();
            displayRules = new ItemDisplayRuleDict(null);

            //sporkItemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/VoidTier1Def.asset").WaitForCompletion();
            sporkItemDef.deprecatedTier = ItemTier.VoidTier1;
            sporkItemDef.pickupIconSprite = Assets.mainAssetBundle.LoadAsset<Sprite>("Assets/Import/Items/icons/spork.png");
            sporkItemDef.pickupModelPrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("Assets/Import/Items/models/spork/Spork.prefab");
            HopooShaderToMaterial.Standard.Apply(sporkItemDef.pickupModelPrefab.GetComponentInChildren<Renderer>().sharedMaterial);
            HopooShaderToMaterial.Standard.Gloss(sporkItemDef.pickupModelPrefab.GetComponentInChildren<Renderer>().sharedMaterial, 0.2f, 5f, Color.white);

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
            stackBuff.iconSprite = Assets.mainAssetBundle.LoadAsset<Sprite>("Assets/Import/Buffs/sporkStack.png");

            ContentAddition.AddBuffDef(stackBuff);
        }

        protected override void SetupHooks()
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport report)
        {
            //If a character was killed by the world, we shouldn't do anything.
            if (!report.attacker || !report.attackerBody)
            {
                return;
            }

            // Else, let's try to pump up those stacks
            CharacterBody characterBody = report.attackerBody;

            if (characterBody.inventory)
            {
                var grabCount = characterBody.inventory.GetItemCount(sporkItemDef.itemIndex);

                if (grabCount > 0)
                {
                    var stackCount = characterBody.GetBuffCount(stackBuff);

                    if (stackCount < maxStacks)
                    {
                        characterBody.AddBuff(stackBuff);
                    }
                }
            }

            orig.Invoke(self, report);
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody characterBody, RecalculateStatsAPI.StatHookEventArgs args)
        {
            //We need an inventory to do check for our item
            if (characterBody.inventory && characterBody.isPlayerControlled)
            {
                //store the amount of our item we have
                var grabCount = characterBody.inventory.GetItemCount(sporkItemDef.itemIndex);
                var cooldownBonusStacks = characterBody.GetBuffCount(stackBuff);

                if (grabCount > 0)
                {
                    args.cooldownMultAdd -= cooldownBonusPerKill * cooldownBonusStacks * grabCount;
                }
                else
                {
                    var stackCount = characterBody.GetBuffCount(stackBuff);

                    if (stackCount > 0)
                    {
                        for (var x = 0; x < stackCount; x++)
                        {
                            characterBody.RemoveBuff(stackBuff);
                        }
                    }
                }
            }
        }
    }
}
