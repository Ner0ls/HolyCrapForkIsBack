using BepInEx.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using R2API;
using UnityEngine.AddressableAssets;

namespace HolyCrapForkIsBack.Items
{
    public class Chopsticks : ItemBase<Chopsticks>
    {
        public static ItemDef chopsticksItemDef;

        public override string name => prefix + "CHOPSTICKS";
        public override ItemTag[] itemTags => new ItemTag[2] { ItemTag.Damage, ItemTag.LowHealth };

        public override bool canRemove => false;
        public override bool hidden => false;
        
        public float critChanceBonus = 20f;
        public float critDamageBonus = 0.3f;
        public BuffDef recentBreak { get; private set; }
        public override bool dlcRequired => false;

        #region LanguageTokens
        public override string nameToken => prefix + "CHOPSTICKS_NAME";
        public override string pickupToken => prefix + "CHOPSTICKS_PICKUP";
        public override string descToken => prefix + "CHOPSTICKS_DESC";
        public override string loreToken => prefix + "CHOPSTICKS_LORE";
        #endregion

        #region DefaultLanguage
        public override string nameDefault => "Sharpened Chopsticks";
        public override string pickupDefault => "Boosts critical chance and critical damage, breaks at low health.";
        public override string descDefault => $"Gain +{critChanceBonus}% critical chance <style=cStack>(+{critChanceBonus}% per stack)</style> and {critDamageBonus * 100}% critical damage <style=cStack>(+{critDamageBonus * 100}% per stack)</style>.";
        public override string loreDefault => "\"So... How would we use this again?\" I asked my partner.\n" +
                                                "\n" +
                                                "\"Look, I know you think it’s just a pair of chopsticks, but if you turn them this way...\"\n He held the chopsticks between his fingers." +
                                                "\n" +
                                                "\"Why would you throw that? It’s not even sharp...\" I replied.\n" +
                                                "\n" +
                                                "\"Not yet.\" He insisted, while throwing the chopsticks on his backpack\n" +
                                                "\n" +
                                                "\"Why do I bother to debate...\" I said, as I continued foraging the place.\n" +
                                                "\n" +
                                                "\"It also looks cool!\" He yelled from the distance.";
        #endregion

        public override void Init(ConfigFile config)
        {
            chopsticksItemDef = InitializeItemDef();
            displayRules = new ItemDisplayRuleDict(null);

            chopsticksItemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier2Def.asset").WaitForCompletion();
            chopsticksItemDef.pickupIconSprite = Assets.mainAssetBundle.LoadAsset<Sprite>("Assets/Import/Items/icons/sharp_chopsticks.png");
            chopsticksItemDef.pickupModelPrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("Assets/Import/Items/models/chopsticks/Chopsticks.prefab");
            HopooShaderToMaterial.Standard.Apply(chopsticksItemDef.pickupModelPrefab.GetComponentInChildren<Renderer>().sharedMaterial);

            SetupLanguageTokens();
            SetupHooks();
            
            ItemAPI.Add(new CustomItem(chopsticksItemDef, displayRules));
        }

        protected override void SetupHooks()
        {
            On.RoR2.HealthComponent.UpdateLastHitTime += HealthComponent_UpdateLastHitTime;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void HealthComponent_UpdateLastHitTime(On.RoR2.HealthComponent.orig_UpdateLastHitTime orig, HealthComponent self, float damageValue, Vector3 damagePosition, bool damageIsSilent, GameObject attacker)
        {
            orig.Invoke(self, damageValue, damagePosition, damageIsSilent, attacker);
            CharacterBody characterBody = self.body;

            if (characterBody.inventory && self.alive && self.isHealthLow)
            {
                var grabCount = characterBody.inventory.GetItemCount(chopsticksItemDef.itemIndex);

                if (grabCount > 0)
                {
                    CharacterMasterNotificationQueue.PushItemTransformNotification(characterBody.master, ItemBase<BrokenChopsticks>.instance.itemDef.itemIndex, ItemBase<BrokenChopsticks>.instance.itemDef.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
                    characterBody.inventory.RemoveItem(chopsticksItemDef, grabCount);
                    characterBody.inventory.GiveItem(ItemBase<BrokenChopsticks>.instance.itemDef, grabCount);
                }
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody characterBody, RecalculateStatsAPI.StatHookEventArgs args)
        {
            //We need an inventory to do check for our item
            if (characterBody.inventory)
            {
                //store the amount of our item we have
                var grabCount = characterBody.inventory.GetItemCount(chopsticksItemDef.itemIndex);
                if (grabCount > 0)
                {
                    args.critAdd += (critChanceBonus * grabCount);
                    args.critDamageMultAdd += (critDamageBonus * grabCount);
                }
            }
        }
    }
}
