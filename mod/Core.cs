﻿using HarmonyLib;
using System.Collections.Generic;
using System.Text;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using Newtonsoft.Json;
using ArchipelagoULTRAKILL.Structures;
using ArchipelagoULTRAKILL.Components;
using BepInEx.Logging;
using System.Reflection;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;

namespace ArchipelagoULTRAKILL
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency("com.eternalUnion.pluginConfigurator")]
    public class Core : BaseUnityPlugin
    {
        public const string PluginGUID = "trpg.archipelagoultrakill";
        public const string PluginName = "Archipelago";
        public const string PluginVersion = "2.0.1";

        public static string workingPath;
        public static string workingDir;

        public static new ManualLogSource Logger { get; } = BepInEx.Logging.Logger.CreateLogSource("Archipelago");
        public static plog.Logger PLogger { get; } = new plog.Logger("Archipelago");

        public static GameObject obj;
        public static UIManager uim;

        public static bool IsInIntro => GameStateManager.Instance.IsStateActive("intro");
        public static bool IsPitFalling => GameStateManager.Instance.IsStateActive("pit-falling");
        public static bool IsPaused => GameStateManager.Instance.IsStateActive("pause") || GameStateManager.Instance.IsStateActive("pit-falling");
        public static bool IsInLevel => !GameStateManager.Instance.IsStateActive("main-menu") && !GameStateManager.Instance.IsStateActive("intro");
        public static bool IsPlaying => IsInLevel && !IsPaused && PlayerHelper.Instance != null;
        public static bool CanGetWeapon => IsPlaying && GunSetter.Instance != null;

        public static Data data = new Data();
        public static bool firstTimeLoad = false;

        public static readonly List<LevelInfo> levelInfos = new List<LevelInfo>()
        {
            new LevelInfo("0-1", 1, 0, true, MusicType.Normal, SkullsType.None),
            new LevelInfo("0-2", 2, 0, true, MusicType.Normal, SkullsType.Normal, new List<string>() { "2_b" }),
            new LevelInfo("0-3", 3, 0, true, MusicType.Normal, SkullsType.None),
            new LevelInfo("0-4", 4, 0, true, MusicType.Normal, SkullsType.None),
            new LevelInfo("0-5", 5, 0, false, MusicType.Special2, SkullsType.None),
            new LevelInfo("1-1", 6, 1, true, MusicType.Special, SkullsType.Normal, new List<string>() { "6_b", "6_r" }),
            new LevelInfo("1-2", 7, 1, true, MusicType.Special, SkullsType.Normal, new List<string>() { "7_r", "7_b" }),
            new LevelInfo("1-3", 8, 1, true, MusicType.Normal, SkullsType.Normal, new List<string>() { "8_r", "8_b" }),
            new LevelInfo("1-4", 9, 1, false, MusicType.Special, SkullsType.Special),
            new LevelInfo("2-1", 10, 2, true, MusicType.Special, SkullsType.None),
            new LevelInfo("2-2", 11, 2, true, MusicType.Normal, SkullsType.None),
            new LevelInfo("2-3", 12, 2, true, MusicType.Normal, SkullsType.Normal, new List<string>() { "12_r", "12_b" }),
            new LevelInfo("2-4", 13, 2, false, MusicType.Special, SkullsType.Normal, new List<string>() { "13_r", "13_b" }),
            new LevelInfo("3-1", 14, 3, true, MusicType.Special, SkullsType.None),
            new LevelInfo("3-2", 15, 3, false, MusicType.Special, SkullsType.None),
            new LevelInfo("4-1", 16, 4, true, MusicType.Normal, SkullsType.None),
            new LevelInfo("4-2", 17, 4, true, MusicType.Normal, SkullsType.Normal, new List<string>() { "17_r", "17_b" }),
            new LevelInfo("4-3", 18, 4, true, MusicType.Special, SkullsType.Normal, new List<string>() { "18_b" }),
            new LevelInfo("4-4", 19, 4, false, MusicType.Special, SkullsType.Normal, new List<string>() { "19_b" }),
            new LevelInfo("5-1", 20, 5, true, MusicType.Normal, SkullsType.Special),
            new LevelInfo("5-2", 21, 5, true, MusicType.Skip, SkullsType.Normal, new List<string>() { "21_r", "21_b" }),
            new LevelInfo("5-3", 22, 5, true, MusicType.Special, SkullsType.Normal, new List<string>() { "22_r", "22_b" }),
            new LevelInfo("5-4", 23, 5, false, MusicType.Skip, SkullsType.None),
            new LevelInfo("6-1", 24, 6, true, MusicType.Special, SkullsType.Normal, new List<string>() { "24_r" }),
            new LevelInfo("6-2", 25, 6, false, MusicType.Special, SkullsType.None),
            new LevelInfo("7-1", 26, 7, true, MusicType.Special, SkullsType.Normal, new List<string>() { "26_b", "26_r" }),
            new LevelInfo("7-2", 27, 7, true, MusicType.Special, SkullsType.Normal, new List<string>() { "27_r" }),
            new LevelInfo("7-3", 28, 7, true, MusicType.Special, SkullsType.None),
            new LevelInfo("7-4", 29, 7, false, MusicType.Skip, SkullsType.None),
            new LevelInfo("P-1", 666, 666, false, MusicType.Special, SkullsType.None),
            new LevelInfo("P-2", 667, 667, false, MusicType.Special, SkullsType.None)
        };

        public static List<string> AllLevels
        {
            get
            {
                List<string> list = new List<string>();
                foreach (LevelInfo info in levelInfos)
                {
                    if (info.Name == "0-1") continue;
                    list.Add(info.Name);
                }
                return list;
            }
        }

        public static bool CurrentLevelHasInfo
        {
            get
            {
                if (SceneHelper.CurrentScene == "Level 0-S") return true;
                else return SceneHelper.CurrentScene.Contains("Level ") && !SceneHelper.IsSceneRankless;
            }
        }

        public static LevelInfo CurrentLevelInfo
        {
            get
            {
                if (SceneHelper.CurrentScene == "Level 0-S") return new LevelInfo("0-S", 0, 0, false, MusicType.Skip, SkullsType.Normal);
                else if (CurrentLevelHasInfo) return GetLevelInfo(SceneHelper.CurrentLevelNumber);
                else return null;
            }
        }

        public static LevelInfo GetLevelInfo(int id)
        {
            foreach (LevelInfo info in levelInfos)
            {
                if (info.Id == id) return info;
            }
            Logger.LogWarning($"No level info for ID {id}.");
            return null;
        }

        public static LevelInfo GetLevelInfo(string name)
        {
            foreach (LevelInfo info in levelInfos)
            {
                if (info.Name == name) return info;
            }
            Logger.LogWarning($"No level info for name {name}.");
            return null;
        }

        public static int GetLevelIdFromName(string name)
        {
            LevelInfo info = GetLevelInfo(name);
            if (info == null)
            {
                Logger.LogWarning($"No level info for name {name}.");
                return 0;
            }
            return info.Id;
        }

        public static string GetLevelNameFromId(int id)
        {
            LevelInfo info = GetLevelInfo(id);
            if (info == null)
            {
                Logger.LogWarning($"No level info for ID {id}.");
                return null;
            }
            return info.Name;
        }

        public static readonly Dictionary<string, int> shopPrices = new Dictionary<string, int>()
        {
            ["rev2"] = 7500,
            ["rev1"] = 12500,
            ["sho1"] = 12500,
            ["nai1"] = 25000,
            ["rai1"] = 100000,
            ["rai2"] = 100000,
            ["rock1"] = 75000
        };

        public void Awake()
        {
            Harmony harmony = new Harmony("archipelago");
            harmony.PatchAll();

            workingPath = Assembly.GetExecutingAssembly().Location;
            workingDir = Path.GetDirectoryName(workingPath);
            //logger.LogInfo($"Working Path: {workingPath}, Working Dir: {workingDir}");

            ConfigManager.Initialize();

            obj = gameObject;
            obj.transform.localPosition = new Vector3(960, 540, 0);

            uim = obj.AddComponent<UIManager>();

            SceneManager.sceneLoaded += OnSceneLoaded;

            GameConsole.Console.Instance.RegisterCommand(new Commands.Connect());
            GameConsole.Console.Instance.RegisterCommand(new Commands.Disconnect());
            GameConsole.Console.Instance.RegisterCommand(new Commands.Say());

            StartCoroutine(VersionChecker.CheckVersion());
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (SceneHelper.CurrentScene == "Intro" || SceneHelper.CurrentScene == "Bootstrap" || SceneHelper.CurrentScene == null) return;
            uim.StopCoroutine("DisplayMessage");

            UIManager.displayingMessage = false;
            UIManager.levels.Clear();
            UIManager.secrets.Clear();
            UIManager.skullIcons.Clear();
            uim.deathLinkMessage = null;

            LevelManager.skulls.Clear();

            ConfigManager.connectionInfo.text = "";
            if (ConfigManager.uiColorRandomizer.value == ColorOptions.EveryLoad) ColorRandomizer.RandomizeUIColors();
            if (ConfigManager.gunColorRandomizer.value == ColorOptions.EveryLoad) ColorRandomizer.RandomizeGunColors();

            if (SceneHelper.CurrentScene == "Main Menu")
            {
                UIManager.FindMenuObjects();

                if (DataExists() && Multiworld.Authenticated) UIManager.menuIcon.GetComponent<Image>().color = Colors.Green;
                else if (DataExists() && !Multiworld.Authenticated) UIManager.menuIcon.GetComponent<Image>().color = Colors.Red;

                if (UIManager.log == null) UIManager.CreateLogObject();

                if (DataExists() && !firstTimeLoad)
                {
                    LoadData();
                    ConfigManager.LoadConnectionInfo();
                    ConfigManager.LoadStats();
                    firstTimeLoad = true;
                }
                else if (!DataExists()) ConfigManager.ResetStatsDefaults();

                if (DataExists() && data.randomizeSkulls) UIManager.CreateSkullIcons();

                if (data.completedLevels.Count >= data.goalRequirement)
                {
                    if (!data.unlockedLevels.Contains(data.goal)) data.unlockedLevels.Add(data.goal);
                }
            }
            else if (IsInLevel && DataExists())
            {
                UIManager.CreateMessageUI();
                if (data.musicRandomizer && CurrentLevelHasInfo && CurrentLevelInfo.Music > MusicType.Skip && CurrentLevelInfo.Music < MusicType.Special2) AudioManager.ChangeMusic();
            }
            else if (SceneHelper.CurrentScene == "Endless" && Multiworld.HintMode) UIManager.CreateMessageUI();
            if (!IsInIntro) OptionsManager.Instance.optionsMenu.gameObject.AddComponent<OptionsMenuState>();

            if (DataExists() && UIManager.log != null) UIManager.AdjustLogBounds();
            if (DataExists() && SceneHelper.CurrentScene == "Level 1-2" && GameProgressSaver.GetGeneralProgress().nai0 == 0) LevelManager.DeactivateNailgun();
        }

        public static bool DataExists()
        {
            string filePath = Path.Combine(GameProgressSaver.BaseSavePath, string.Format("Slot{0}", GameProgressSaver.currentSlot + 1)) + "\\archipelago.json";
            return File.Exists(filePath);
        }

        public static void SaveData()
        {
            string filePath = Path.Combine(GameProgressSaver.BaseSavePath, string.Format("Slot{0}", GameProgressSaver.currentSlot + 1)) + "\\archipelago.json";
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
            File.WriteAllBytes(filePath, bytes);
        }

        public static void LoadData()
        {
            string filePath = Path.Combine(GameProgressSaver.BaseSavePath, string.Format("Slot{0}", GameProgressSaver.currentSlot + 1)) + "\\archipelago.json";
            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    data = JsonConvert.DeserializeObject<Data>(reader.ReadToEnd());
                }
            }
            else
            {
                Logger.LogError("Archipelago data for slot " + (GameProgressSaver.currentSlot + 1) + " does not exist.");
            }
        }

        public static void DeleteData(int slot)
        {
            string filePath = Path.Combine(GameProgressSaver.BaseSavePath, string.Format("Slot{0}", slot + 1)) + "\\archipelago.json";
            if (File.Exists(filePath)) File.Delete(filePath);
        }

        public static bool IsFire2Unlocked(string weapon)
        {
            if (!data.randomizeFire2) return true;
            return data.unlockedFire2.Contains(weapon);
        }

        public static bool CanBreakGlass()
        {
            GameProgressMoneyAndGear save = GameProgressSaver.GetGeneralProgress();

            // Piercer
            if (save.rev0 > 0 && IsFire2Unlocked("rev0")) return true;
            // Sharpshooter
            if (save.rev1 > 0 && IsFire2Unlocked("rev1")) return true;
            // Marksman
            if (save.rev2 > 0 && IsFire2Unlocked("rev2")) return true;

            // Core Eject
            if (save.sho0 > 0 && IsFire2Unlocked("sho0")) return true;
            // Pump Charge
            if (save.sho1 > 0 && IsFire2Unlocked("sho1")) return true;

            // Electric
            if (save.rai0 > 0) return true;
            // Malicious
            if (save.rai2 > 0) return true;

            // Freezeframe
            if (save.rock0 > 0) return true;
            // S.R.S. Cannon
            if (save.rock1 > 0) return true;

            // Knuckleblaster
            if (save.arm1 > 0) return true;

            return false;
        }

        public static void SpawnSoap()
        {
            LocationManager.soapWaiting = false;
            GameObject obj = Instantiate(AssetHelper.LoadPrefab("Assets/Prefabs/Items/Soap.prefab"), NewMovement.Instance.transform);
            obj.transform.parent = null;

            if (FistControl.Instance.currentPunch != null || !(!data.hasArm && FistControl.Instance.currentPunch.type == FistType.Standard))
            {
                if (!FistControl.Instance.currentPunch.holding)
                {
                    FistControl.Instance.currentPunch.ForceHold(obj.GetComponent<ItemIdentifier>());
                }
            }
        }

        public static List<string> SearchAssetKeys(string contains)
        {
            List<string> keys = new List<string>();
            foreach (var locator in Addressables.ResourceLocators)
            {
                if (locator is ResourceLocationMap)
                {
                    foreach (string key in locator.Keys)
                    {
                        if (key.Contains(contains)) keys.Add(key);
                    }
                }
            }
            return keys;
        }

        public static List<T> FindAllComponentsInCurrentScene<T>() where T : Behaviour
        {
            List<T> list = new List<T>();
            foreach (T component in Resources.FindObjectsOfTypeAll<T>())
            {
                if (component.gameObject.scene.name == SceneManager.GetActiveScene().name)
                {
                    list.Add(component);
                }
            }
            return list;
        }
    }
}
