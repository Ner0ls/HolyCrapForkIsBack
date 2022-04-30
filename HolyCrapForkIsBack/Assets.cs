using System.Reflection;
using R2API;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using System.IO;
using System.Collections.Generic;
using RoR2.UI;

namespace HolyCrapForkIsBack
{
    using UnityEngine;
    using System.IO;

    //Static class for ease of access
    public static class Assets
    {
        internal static AssetBundle mainAssetBundle;

        // Name of Asset bundle
        private const string assetbundleName = "hcfbasset";
        private static string[] assetNames = new string[0];

        public static void Init()
        {
            if (mainAssetBundle == null)
            {
                using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("HolyCrapForkIsBack." + assetbundleName))
                {
                    mainAssetBundle = AssetBundle.LoadFromStream(assetStream);
                }
            }

            assetNames = mainAssetBundle.GetAllAssetNames();
            Log.LogInfo(assetNames.ToString());
        }
    }
}
