using BepInEx.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using R2API;
using UnityEngine.AddressableAssets;

namespace HolyCrapForkIsBack.Items
{
    public class Fork : ItemBase<Fork>
    {
        public static ItemDef forkItemDef;
        public override bool disabled => false;
        public override string name => prefix + "FORK";
        public override ItemTag[] itemTags => new ItemTag[1] { ItemTag.Damage };

        public override bool canRemove => false;
        public override bool hidden => false;
        
        public float damageBonus = 3f;
        public float damageBonusPerStack = 3f;

        #region LanguageTokens
        public override string nameToken => prefix + "FORK_NAME";
        public override string pickupToken => prefix + "FORK_PICKUP";
        public override string descToken => prefix + "FORK_DESC";
        public override string loreToken => prefix + "FORK_LORE";
        #endregion

        #region DefaultLanguage
        public override string nameDefault => "Fork";
        public override string pickupDefault => "Do more damage.";
        public override string descDefault => $"Gain +{damageBonus} Base Damage <style=cStack>(+{damageBonusPerStack} per stack)</style>.";
        public override string loreDefault => "Totally original idea";
        #endregion

        public override void Init()
        {
            forkItemDef = InitializeItemDef();
            displayRules = new ItemDisplayRuleDict(null);

            forkItemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier1Def.asset").WaitForCompletion();
            forkItemDef.pickupIconSprite = Assets.mainAssetBundle.LoadAsset<Sprite>("Assets/Import/Items/icons/fork.png");
            forkItemDef.pickupModelPrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("Assets/Import/Items/models/fork/Fork.prefab");
            HopooShaderToMaterial.Standard.Apply(forkItemDef.pickupModelPrefab.GetComponentInChildren<Renderer>().sharedMaterial);
            HopooShaderToMaterial.Standard.Gloss(forkItemDef.pickupModelPrefab.GetComponentInChildren<Renderer>().sharedMaterial, 0.1f, 10f, Color.white);

            SetupLanguageTokens();
            SetupHooks();
            
            ItemAPI.Add(new CustomItem(forkItemDef, displayRules));
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
                var grabCount = characterBody.inventory.GetItemCount(forkItemDef.itemIndex);
                if (grabCount > 0)
                {
                    args.baseDamageAdd += (damageBonus + (damageBonusPerStack * grabCount));
                }
            }
        }
    }
}
