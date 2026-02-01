#nullable enable
using System;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using SV.H;
using Character;
using MagicaCloth;

using LogLevel = BepInEx.Logging.LogLevel;


namespace ILL_HSceneBreastJiggle
{
	[BepInProcess(PROCESS_NAME)]
	[BepInPlugin(GUID, PLUGIN_NAME, VERSION)]
	[BepInIncompatibility("SVS_HSceneAddOns")]
	public partial class HSceneBreastJiggle : BasePlugin
	{
		/*PLUGIN INFO*/
		public const string PLUGIN_NAME = "SVS_HSceneBreastJiggle";
		public const string PROCESS_NAME = "SamabakeScramble";
		public const string GUID = "SVS_HSceneBreastJiggle";
		public const string VERSION = "1.0.0";



		/*VARIABLES*/
		private static HScene? _hscene = null!;



		/*METHODS*/
		private static partial void UpdateBreastSoftness()
		{
			if (_hscene != null)
			{
				if (enable.Value)
				{
					switch (softnessSettingType.Value)
					{
						case ValueType.Default:
						{
							foreach (HActor hActor in _hscene.Actors)
							{
								if (hActor != null)
								{
									if (hActor.IsMan == false || (hActor.IsMan == false && hActor.IsManOrFutanari))
									{
										//Revert to original values
										hActor.Human.body.UpdateBustShake();
									}
								}
							}
							return;
						}
						case ValueType.Fixed:
						{
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
										fileBody.bustSoftness = Mathf.Clamp01(baseSoftness.Value);
										fileBody.bustSoftness2 = Mathf.Clamp01(tipSoftness.Value);
										fileBody.bustWeight = Mathf.Clamp01(softness.Value);
										body.UpdateBustShake();

										//Revert values in order to prevent permanent changes
										fileBody.bustSoftness = originalBaseSoftness;
										fileBody.bustSoftness2 = originalTipSoftness;
										fileBody.bustWeight = originalWeight;
									}
								}
							}
							return;
						}
						case ValueType.Offset:
						{
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

										//Apply new values and update bust shake
										fileBody.bustSoftness = Mathf.Clamp01(originalBaseSoftness + baseSoftness.Value);
										fileBody.bustSoftness2 = Mathf.Clamp01(originalTipSoftness + tipSoftness.Value);
										fileBody.bustWeight = Mathf.Clamp01(originalWeight + softness.Value);
										body.UpdateBustShake();

										//Revert values in order to prevent permanent changes
										fileBody.bustSoftness = originalBaseSoftness;
										fileBody.bustSoftness2 = originalTipSoftness;
										fileBody.bustWeight = originalWeight;
									}
								}
							}
							return;
						}
					}
				}
				else
				{
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
		}



		/*HOOKS*/
		public static partial class Hooks
		{
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