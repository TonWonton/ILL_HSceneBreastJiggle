#nullable enable
using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using H;
using Character;
using MagicaCloth;

using LogLevel = BepInEx.Logging.LogLevel;


namespace ILL_HSceneBreastJiggle
{
	[BepInProcess(PROCESS_NAME)]
	[BepInPlugin(GUID, PLUGIN_NAME, VERSION)]
	public partial class HSceneBreastJiggle : BasePlugin
	{
		/*PLUGIN INFO*/
		public const string PLUGIN_NAME = "AC_HSceneBreastJiggle";
		public const string PROCESS_NAME = "Aicomi";
		public const string GUID = "AC_HSceneBreastJiggle";
		public const string VERSION = "1.0.1";



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
						foreach (HActor femaleHActor in _hscene._hActorReceivers)
						{
							if (femaleHActor != null)
							{
								//Get info
								HumanBody body = femaleHActor.Human.body;
								HumanDataBody fileBody = body._fileBody;
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
									fileBody.bustSoftness = Mathf.Clamp01(originalBaseSoftness + softness.Value);
									fileBody.bustSoftness2 = Mathf.Clamp01(originalTipSoftness + softness.Value);
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
					else
					{
						//If disabled or default softnessSettingType
						foreach (HActor femaleHActor in _hscene._hActorReceivers)
						{
							if (femaleHActor != null)
							{
								//Else just update bust shake to revert to original values
								femaleHActor.Human.body.UpdateBustShake();
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
			[HarmonyPatch(typeof(ProcBase), nameof(ProcBase.Initialize))]
			public static void ProcBasePostInitialize(ProcBase __instance)
			{
				if (_hscene == null)
				{
					_hscene = __instance._hScene;
					UpdateBreastSoftness();
				}
			}

			[HarmonyPrefix]
			[HarmonyPatch(typeof(HResult), nameof(HResult.EvaluationResult))]
			[HarmonyPatch(typeof(HScene), nameof(HScene.RestoreActors))]
			public static void HScenePreEnd()
			{
				_hscene = null;
			}
		}
	}
}