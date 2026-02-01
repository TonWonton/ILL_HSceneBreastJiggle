#nullable enable
using System;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Character;
using MagicaCloth;

using LogLevel = BepInEx.Logging.LogLevel;


namespace ILL_HSceneBreastJiggle
{
	public enum ValueType
	{
		Default = 1,
		Fixed = 2,
		Offset = 3
	}


	public partial class HSceneBreastJiggle : BasePlugin
	{
		#region PLUGIN_INFO

		/*PLUGIN INFO*/
		public const string COPYRIGHT = "";
		public const string COMPANY = "https://github.com/TonWonton/ILL_HSceneBreastJiggle";



		#endregion



		/*VARIABLES*/
		//Instance
		public static HSceneBreastJiggle Instance { get; private set; } = null!;
		private static ManualLogSource _log = null!;
		private static MagicaPhysicsManager? _magicaPhysicsManager = null!;



		/*CONFIG*/
		public const string CATEGORY_GENERAL = "General";
		public static ConfigEntry<bool> enable = null!;

		public const string MAGICA_CLOTH = "Magica physics";
		public static ConfigEntry<UpdateTimeManager.UpdateMode> updateMode = null!;
		public static ConfigEntry<UpdateTimeManager.UpdateCount> updateCount = null!;

		public const string SOFTNESS = "Softness";
		public static ConfigEntry<ValueType> softnessSettingType = null!;
		public static ConfigEntry<float> baseSoftness = null!;
		public static ConfigEntry<float> tipSoftness = null!;
		public static ConfigEntry<float> softness = null!;

		public static ConfigDescription SoftnessConfig(int order) => new ConfigDescription(string.Empty, new AcceptableValueRange<float>(-1f, 1f), new ConfigurationManagerAttributes() { Order = order });



		/*METHODS*/
		private static void UpdateAllSettings()
		{
			UpdateMagicaPhysicsManager();
			UpdateBreastSoftness();
		}

		private static void UpdateMagicaPhysicsManager()
		{
			if (_magicaPhysicsManager != null)
			{
				if (enable.Value)
				{
					_magicaPhysicsManager.UpdateMode = updateMode.Value;
					_magicaPhysicsManager.UpdatePerSeccond = updateCount.Value;
					_magicaPhysicsManager.UpdateTime.SetUpdateMode(updateMode.Value);
					_magicaPhysicsManager.UpdateTime.SetUpdatePerSecond(updateCount.Value);
				}
				else
				{
					_magicaPhysicsManager.UpdateMode = UpdateTimeManager.UpdateMode.UnscaledTime;
					_magicaPhysicsManager.UpdatePerSeccond = UpdateTimeManager.UpdateCount._90_Default;
					_magicaPhysicsManager.UpdateTime.SetUpdateMode(UpdateTimeManager.UpdateMode.UnscaledTime);
					_magicaPhysicsManager.UpdateTime.SetUpdatePerSecond(UpdateTimeManager.UpdateCount._90_Default);
				}
			}
		}

		private static partial void UpdateBreastSoftness();



		/*EVENT HANDLING*/
		private static void OnEnableChanged(object? sender, EventArgs args)
		{
			UpdateAllSettings();
		}

		private static void OnMagicaPhysicsManagerSettingsChanged(object? sender, EventArgs args)
		{
			UpdateMagicaPhysicsManager();
		}

		private static void OnBreastSoftnessSettingsChanged(object? sender, EventArgs args)
		{
			UpdateBreastSoftness();
		}


		/*PLUGIN LOAD*/
		public override void Load()
		{
			//Instance
			Instance = this;
			_log = Log;

			//Config
			enable = Config.Bind(CATEGORY_GENERAL, "Enable", false, new ConfigDescription(string.Empty, null, new ConfigurationManagerAttributes() { Order = 0 }));
			updateMode = Config.Bind(MAGICA_CLOTH, "Update Mode", UpdateTimeManager.UpdateMode.UnscaledTime, new ConfigDescription("https://magicasoft.jp/en/magica-cloth-physics-manager-2/\n\n" +
																																   "[Unscaled Time]\n" +
																																   "Updates are performed regardless of the Unity frame rate. Useful when the frame rate fluctuates during the game. However, the physics engine may be executed multiple times in one frame, and this will cause a wave in performance.\n\n" +
																																   "[Once Per Frame]\n" +
																																   "Updates are made only once per Unity frame. In this case, the physics engine advances by the time step of [Update Per Second] with one update. When using this mode, [Update Per Second] needs to be adjusted to the frame rate of the game.\n\n" +
																																   "[Delay Unscaled Time]\n" +
																																   "Delayed execution. Update is the same as Unscaled Time, but also runs the Cloth simulation while rendering. This greatly improves performance. But note that the result is one frame late.", null, new ConfigurationManagerAttributes() { Order = 0 }));
			updateCount = Config.Bind(MAGICA_CLOTH, "Update Count", UpdateTimeManager.UpdateCount._90_Default, new ConfigDescription("Number of physics engine updates per second.", null, new ConfigurationManagerAttributes() { Order = -1 }));
			softnessSettingType = Config.Bind(SOFTNESS, "Softness setting type", ValueType.Offset, new ConfigDescription("[Default] Use softness values from the character card.\n" +
																														 "[Fixed] Use fixed softness values.\n" +
																														 "[Offset] Use values from the character card + the softness settings.\n", null, new ConfigurationManagerAttributes() { Order = 0 }));
			baseSoftness = Config.Bind(SOFTNESS, "Base softness", 0.2f, SoftnessConfig(-1));
			tipSoftness = Config.Bind(SOFTNESS, "Tip softness", 0.2f, SoftnessConfig(-2));
			softness = Config.Bind(SOFTNESS, "Weight", 0.2f, SoftnessConfig(-3));

			//Register events
			enable.SettingChanged += OnEnableChanged;
			updateMode.SettingChanged += OnMagicaPhysicsManagerSettingsChanged;
			updateCount.SettingChanged += OnMagicaPhysicsManagerSettingsChanged;
			softnessSettingType.SettingChanged += OnBreastSoftnessSettingsChanged;
			baseSoftness.SettingChanged += OnBreastSoftnessSettingsChanged;
			tipSoftness.SettingChanged += OnBreastSoftnessSettingsChanged;
			softness.SettingChanged += OnBreastSoftnessSettingsChanged;

			//Create hooks
			Harmony.CreateAndPatchAll(typeof(Hooks), GUID);
			Logging.Info("Loaded");
		}



		/*HOOKS*/
		public static partial class Hooks
		{
			[HarmonyPostfix]
			[HarmonyPatch(typeof(MagicaPhysicsManager), nameof(MagicaPhysicsManager.Awake))]
			public static void MagicaPhysicsManagerPostAwake(MagicaPhysicsManager __instance)
			{
				_magicaPhysicsManager = __instance;
				UpdateMagicaPhysicsManager();
			}
		}



		//Logging
		public static class Logging
		{
			public static void Log(LogLevel level, string message)
			{
				_log.Log(level, message);
			}

			public static void Fatal(string message)
			{
				_log.LogFatal(message);
			}

			public static void Error(string message)
			{
				_log.LogError(message);
			}

			public static void Warning(string message)
			{
				_log.LogWarning(message);
			}

			public static void Message(string message)
			{
				_log.LogMessage(message);
			}

			public static void Info(string message)
			{
				_log.LogInfo(message);
			}

			public static void Debug(string message)
			{
				_log.LogDebug(message);
			}
		}
	}
}