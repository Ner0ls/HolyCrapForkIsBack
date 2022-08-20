using BepInEx.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using R2API;
using UnityEngine.AddressableAssets;

namespace HolyCrapForkIsBack.Items
{
    public class SpoonStack : ItemBase<SpoonStack>
    {
        public static ItemDef spoonStackItemDef;
        public override bool disabled => false;

        public override string name => prefix + "SPOON_STACK";
        public override ItemTag[] itemTags => new ItemTag[2] { ItemTag.Damage, ItemTag.CannotCopy };

        public override bool canRemove => false;
        public override bool hidden => true;

        public float constantDamageBonus = 2f;
        public float damageBonusPerKill = 0.01f;
        public float damageBonusCap = 1f;
        public float levelScalingF = 0.20f;

        #region LanguageTokens
        public override string nameToken => prefix + "SPOON_STACK_NAME";
        public override string pickupToken => prefix + "SPOON_STACK_PICKUP";
        public override string descToken => prefix + "SPOON_STACK_DESC";
        public override string loreToken => prefix + "SPOON_STACK_LORE";
        #endregion

        #region DefaultLanguage
        public override string nameDefault => "Spoon Stack";
        public override string pickupDefault => "What are you doing with this?.";
        public override string descDefault => "";

        public override string loreDefault => "";
        #endregion

        public override void Init()
        {
            spoonStackItemDef = InitializeItemDef();
            displayRules = new ItemDisplayRuleDict(null);

            spoonStackItemDef.deprecatedTier = ItemTier.NoTier;

            SetupLanguageTokens();
            SetupHooks();
            
            ItemAPI.Add(new CustomItem(spoonStackItemDef, displayRules));
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
                var grabCount = characterBody.inventory.GetItemCount(spoonStackItemDef.itemIndex);

                if (grabCount > 0)
                {
                    args.baseDamageAdd += damageBonusPerKill * grabCount * (1f + (levelScalingF * characterBody.level));
                }
            }
        }
    }
}
