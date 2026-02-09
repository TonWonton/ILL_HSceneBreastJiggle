#nullable enable
using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using SV.H;
using Character;
using MagicaCloth;

using Logging = ILL_HSceneBreastJiggle.BreastJigglePlugin.Logging;


namespace ILL_HSceneBreastJiggle
{
	[BepInProcess(PROCESS_NAME)]
	[BepInPlugin(GUID, PLUGIN_NAME, VERSION)]
	[BepInIncompatibility("SVS_HSceneAddOns")]
	public partial class BreastJigglePlugin : BasePlugin
	{
		/*PLUGIN INFO*/
		public const string PLUGIN_NAME = "SVS_HSceneBreastJiggle";
		public const string PROCESS_NAME = "SamabakeScramble";
		public const string GUID = "SVS_HSceneBreastJiggle";
		public const string VERSION = "1.0.2";



		/*VARIABLES*/
		private static HScene? _hscene = null!;
		private static bool _isUpdatingBreastSoftness = false;



		/*METHODS*/
		private static partial void UpdateBreastSoftness()
		{
			if (_hscene != null)
			{
				//Set flag to prevent infinite loop
				_isUpdatingBreastSoftness = true;

				//Perform update
				try
				{
					ValueType valueType = softnessSettingType.Value;
					if (enable.Value && valueType != ValueType.Default)
					{
						//If enabled and not default softnessSettingType
						foreach (HActor hActor in _hscene.Actors)
						{
							if (hActor != null)
							{
								if (hActor.IsMan == false || (hActor.IsMan == false && hActor.IsManOrFutanari))
								{
									//Get info
									HumanBody body = hActor.Human.body;
									HumanDataBody fileBody = body.fileBody;
									float originalBaseSoftness = fileBody.bustSoftness;
									float originalTipSoftness = fileBody.bustSoftness2;
									float originalWeight = fileBody.bustWeight;

									//float breastSizeMultiplier = (femaleHActor.Human.body._fileBody.

									//Apply new values and update bust shake
									if (valueType == ValueType.Fixed)
									{
										fileBody.bustSoftness = Mathf.Clamp01(baseSoftness.Value);
										fileBody.bustSoftness2 = Mathf.Clamp01(tipSoftness.Value);
										fileBody.bustWeight = Mathf.Clamp01(softness.Value);
									}
									else //Offset
									{
										fileBody.bustSoftness = Mathf.Clamp01(originalBaseSoftness + baseSoftness.Value);
										fileBody.bustSoftness2 = Mathf.Clamp01(originalTipSoftness + tipSoftness.Value);
										fileBody.bustWeight = Mathf.Clamp01(originalWeight + softness.Value);
									}
									body.UpdateBustShake();

									//Revert values in order to prevent permanent changes
									fileBody.bustSoftness = originalBaseSoftness;
									fileBody.bustSoftness2 = originalTipSoftness;
									fileBody.bustWeight = originalWeight;
								}
							}
						}
					}
					else
					{
						//If disabled or default softnessSettingType
						foreach (HActor hActor in _hscene.Actors)
						{
							if (hActor != null)
							{
								if (hActor.IsMan == false || (hActor.IsMan == false && hActor.IsManOrFutanari))
								{
									//Else just update bust shake to revert to original values
									hActor.Human.body.UpdateBustShake();
								}
							}
						}
					}
				}

				//Reset flag
				finally { _isUpdatingBreastSoftness = false; }
			}
		}



		/*HOOKS*/
		public static partial class Hooks
		{
			[HarmonyPostfix]
			[HarmonyPatch(typeof(HumanMagicaSpringSetting), nameof(HumanMagicaSpringSetting.ChangeBlendWeight))]
			public static void HumanMagicaSpringSettingPostChangeBlendWeight()
			{
				//Check flag to prevent infinite loop
				if (_isUpdatingBreastSoftness == false)
				{
					UpdateBreastSoftness();
				}
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(HScene), nameof(HScene.Start))]
			public static void HScenePostStart(HScene __instance)
			{
				_hscene = __instance;
				UpdateBreastSoftness();
			}

			[HarmonyPrefix]
			[HarmonyPatch(typeof(HScene), nameof(HScene.Dispose))]
			public static void HScenePreDispose()
			{
				_hscene = null;
			}
		}
	}
}