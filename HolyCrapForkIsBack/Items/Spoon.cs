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
        public override string name => prefix + "SPOON";
        public override ItemTag[] itemTags => new ItemTag[2] { ItemTag.Damage, ItemTag.OnKillEffect };

        public override bool canRemove => false;
        public override bool hidden => false;
        public override bool dlcRequired => false;

        public float constantDamageBonus = 2f;
        public float damageBonusPerKill = 0.02f;
        public float damageBonusCap = 1f;

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
        public override string descDefault => $"Gain +{constantDamageBonus} base damage <style=cStack>(does not scale in any way)</style> and +{damageBonusPerKill} base damage <style=cStack>(scales 25% with player level)</style> each time you <style=cIsDamage>kill an enemy</style> with a max cap of {damageBonusCap} <style=cStack>(+{damageBonusCap} per stack)</style>.";

        public override string loreDefault => "\"Well, if you said that could be useful, why can't I try with this?\" I exclaimed.\n" +
            "\n" +
            "...\n" +
            "\n" +
            "\"After all... What if we need to dig a hole as a shelter? Or pop some monster eyes out?\" I insisted.\n" +
            "\n" +
            "\"Dig a hole with a spoon...?\" He said in disbelief.\n" +
            "\n" +
            "\"Yeah.\"\n" +
            "\n" +
            "...\n" +
            "\n" +
            "\"We are gonna die, aren't we?\" I said, breaking his silence.\n" +
            "\n" +
            "\"Probably...\"";
        #endregion

        public override void Init(ConfigFile config)
        {
            spoonItemDef = InitializeItemDef();
            displayRules = new ItemDisplayRuleDict(null);

            spoonItemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier1Def.asset").WaitForCompletion();
            spoonItemDef.pickupIconSprite = Assets.mainAssetBundle.LoadAsset<Sprite>("Assets/Import/Items/icons/spoon.png");
            spoonItemDef.pickupModelPrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("Assets/Import/Items/models/spoon/Spoon.prefab");
            HopooShaderToMaterial.Standard.Apply(spoonItemDef.pickupModelPrefab.GetComponentInChildren<Renderer>().sharedMaterial);
            HopooShaderToMaterial.Standard.Gloss(spoonItemDef.pickupModelPrefab.GetComponentInChildren<Renderer>().sharedMaterial, 0.1f, 10f, Color.white);

            CreateBuff();
            SetupLanguageTokens();
            SetupHooks();
            
            ItemAPI.Add(new CustomItem(spoonItemDef, displayRules));
        }

        protected override void SetupHooks()
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
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
                var grabCount = characterBody.inventory.GetItemCount(spoonItemDef.itemIndex);
                if (grabCount > 0)
                {
                    var stackGrabCount = characterBody.inventory.GetItemCount(SpoonStack.spoonStackItemDef.itemIndex);
                    float currentMaxStacks = (damageBonusCap / damageBonusPerKill) * grabCount;

                    if (stackGrabCount < currentMaxStacks)
                    {
                        characterBody.inventory.GiveItem(SpoonStack.spoonStackItemDef);
                        characterBody.AddBuff(stackBuff);
                    }
                }
            }

            orig.Invoke(self, report);
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody characterBody)
        {
            if (characterBody.inventory)
            {
                var grabCount = characterBody.inventory.GetItemCount(spoonItemDef.itemIndex);

                if (grabCount > 0)
                {
                    var stacksGrabCount = characterBody.inventory.GetItemCount(SpoonStack.spoonStackItemDef.itemIndex);

                    if (stacksGrabCount > 0)
                    {
                        for (var x = 0; x < stacksGrabCount; x++)
                        {
                            characterBody.AddBuff(stackBuff);
                        }
                    }
                }
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody characterBody, RecalculateStatsAPI.StatHookEventArgs args)
        {
            //We need an inventory to do check for our item
            if (characterBody.inventory && characterBody.isPlayerControlled)
            {
                //store the amount of our item we have
                var grabCount = characterBody.inventory.GetItemCount(spoonItemDef.itemIndex);

                if (grabCount > 0)
                {
                    args.baseDamageAdd += constantDamageBonus;
                }
                else
                {
                    var stackGrabCount = characterBody.inventory.GetItemCount(SpoonStack.spoonStackItemDef);
                    if (characterBody.GetBuffCount(stackBuff) > 0)
                    {
                        for (var x = 0; x < characterBody.GetBuffCount(stackBuff); x++)
                        {
                            characterBody.RemoveBuff(stackBuff);
                        }

                        characterBody.inventory.RemoveItem(SpoonStack.spoonStackItemDef, stackGrabCount);
                    }
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
            stackBuff.iconSprite = Assets.mainAssetBundle.LoadAsset<Sprite>("Assets/Import/Buffs/spoonStack.png");

            ContentAddition.AddBuffDef(stackBuff);
        }
    }
}
