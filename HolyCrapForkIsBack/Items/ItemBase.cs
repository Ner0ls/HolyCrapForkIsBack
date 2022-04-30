using BepInEx.Configuration;
using System;
using RoR2;
using R2API;
using UnityEngine;
using System.Reflection;
using System.Linq;
using RoR2.ExpansionManagement;
using RoR2.Items;
using System.Collections.Generic;

namespace HolyCrapForkIsBack.Items
{
    public abstract class ItemBase<T> : ItemBase where T : ItemBase<T>
    {
        public static T instance { get; private set; }
        public ItemBase()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class ItemBase
    {
        public static string prefix = "HCFB_ITEM_";
        public abstract bool disabled { get; }
        public abstract string name { get; }
        public abstract ItemTag[] itemTags { get; }
        public abstract bool canRemove { get; }
        public abstract bool hidden { get; }
        #region LanguageTokens
        public abstract string nameToken { get; }
        public abstract string pickupToken { get; }
        public abstract string descToken { get; }
        public abstract string loreToken { get; }
        #endregion

        #region DefaultLanguage
        public abstract string nameDefault { get; }
        public abstract string pickupDefault { get; }
        public abstract string descDefault { get; }
        public abstract string loreDefault { get; }
        #endregion

        public ItemDisplayRuleDict displayRules;

        //public virtual bool dlcRequired { get; } = false;

        //public ConfigEntry<bool> enabled;
        //public ConfigEntry<bool> aiBlacklist;

        public ItemDef itemDef;
        public static List<ItemBase> items = new List<ItemBase>();

        public abstract void Init();

        //protected virtual void SetupConfig(ConfigFile config) { }

        protected virtual void SetupLanguageTokens()
        {
            // We'll just setup default for now
            LanguageAPI.Add(nameToken, nameDefault);
            LanguageAPI.Add(pickupToken, pickupDefault);
            LanguageAPI.Add(descToken, descDefault);
            LanguageAPI.Add(loreToken, loreDefault);
        }

        protected virtual ItemDef InitializeItemDef()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = name;
            itemDef.nameToken = nameToken;
            itemDef.pickupToken = pickupToken;
            itemDef.descriptionToken = descToken;
            itemDef.loreToken = loreToken;
            itemDef.tags = itemTags;
            itemDef.canRemove = canRemove;
            itemDef.hidden = hidden;

            return itemDef;
        }

        protected virtual void SetupHooks() { }
    }
}