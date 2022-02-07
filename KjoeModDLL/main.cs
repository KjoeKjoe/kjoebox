using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

namespace SlaveMod
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class KjoeMod : BaseUnityPlugin
    {
        public const string pluginGuid = "kjoekjoe.worldbox.ownmod";
        public const string pluginName = "KjoeMod";
        public const string pluginVersion = "0.0.1";
        public string getSlavesUrl = "https://worldboxweb.pixelists.nl/api/slaves/all";
        public List<string> actorIds;

        public void Awake()
        {
            SettingSetup();
            isSpawningSlaves.Value = false;
            isCheckingUpdates.Value = false;
            isRenderingMap.Value = false;
        }
        public void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 120, 0, 120, 30));
            if (GUILayout.Button("Kjoe's Mod"))
            {
                showHideMainWindowConfig.Value = !showHideMainWindowConfig.Value;
                if (showHideMainWindowConfig.Value == false)
                {
                    CloseAllWindows();
                }
            }
            GUILayout.EndArea();
            if (showHideMainWindowConfig.Value == true)
            {
                updateWindows();
            }
        }
        public void CloseAllWindows()
        {
        }

        public void SettingSetup()
        {
            showHideMainWindowConfig = Config.AddSetting("Menus", "Title", false, "main menu");
            isSpawningSlaves = Config.AddSetting("SpawningSettings", "Spawning", false, "spawning slaves");
            isCheckingUpdates = Config.AddSetting("CheckingSpawnUpdates", "Checking", false, "checking slaves");
            isRenderingMap = Config.AddSetting("RenderingMap", "rendering", false, "rendering map");
            activeSlaves = Config.AddSetting("ActiveSlaves", "Active Slaves", "", "slaves Active");
            databaseEndpoint = Config.AddSetting("DatabaseEndpoint", "Database Endpoint", "", "database Endpoint");
        }

        public void updateWindows()
        {
            Color originalcol = GUI.backgroundColor;
            if (showHideMainWindowConfig.Value)
            {
                GUI.contentColor = UnityEngine.Color.white;
                mainWindowRect = GUILayout.Window(1001, mainWindowRect, new GUI.WindowFunction(mainWindow), "Main", new GUILayoutOption[] { GUILayout.MaxWidth(300f), GUILayout.MinWidth(200f) });
            }

            SetWindowInUse(-1);
        }

        public void mainWindow(int windowID)
        {
            SetWindowInUse(windowID);
            WorldTile activeTile = MapBox.instance.getMouseTilePos();

            if (GUILayout.Button("Spawn Slaves"))
            {
                isSpawningSlaves.Value = !isSpawningSlaves.Value;
                isCheckingUpdates.Value = !isCheckingUpdates.Value;

                var spawn = new SpawnSlave();
                spawn.action_spawnSlave();

            }
            if (GUILayout.Button("Save map image"))
            {
                var msg = new AchievementPopup();

                isRenderingMap.Value = !isRenderingMap.Value;

                var render = new CheckSlaveSpawn();
                render.StartRenderingMap();
                
            }
            if (GUILayout.Button("Init Camera"))
            {
                Debug.Log("starting Camera");
                initCamera();                
            }
            if (GUILayout.Button("Init WorldEvents"))
            {
                Debug.Log("starting events");
                var events = new GameObject("checkWorldEvent").AddComponent<EventCommands>();
                events.startCheckEventCmd();                
            }
            if (GUILayout.Button("Init War/Peace"))
            {

                var WarPeace = new GameObject("checkWarPeace").AddComponent<KingdomCommands>();
                WarPeace.startCheckPeaceWarCmd();
            }
            if (GUILayout.Button("Init Revolts"))
            {

                var revolt = new GameObject("checkRevoltCmd").AddComponent<RevoltCommands>();
                revolt.startCheckRevoltCmd();
            }
            GUILayout.Label("Database Endpoint");
            string endpoint = GUILayout.TextField("https://worldboxweb.pixelists.nl");
            databaseEndpoint.Value = endpoint;

            GUI.DragWindow();
        }

        public static void SetWindowInUse(int windowID)
        {
            Event current = Event.current;
            bool inUse = current.type == EventType.MouseDown || current.type == EventType.MouseUp || current.type == EventType.MouseDrag || current.type == EventType.MouseMove;
            if (inUse)
            {
                windowInUse = windowID;
            }
        }

        public void initCamera()
        {
            MoveCamera.focusUnit = null;
            Reflection.CallMethod(MapBox.instance, "centerCamera");
            if (this.actorIds != null )
            {
                this.actorIds.Clear();
            }

            WWW request = new WWW(getSlavesUrl);
            StartCoroutine(getUnits(request, 40.0f));
        }

        IEnumerator getUnits(WWW request, float waitTime)
        {
            while (!request.isDone)
                yield return new WaitForSeconds(waitTime);


            if (request.text == "{\"data\": []}")
                yield return new WaitForSeconds(waitTime);

            DiscordSlavesList slaves = JsonConvert.DeserializeObject<DiscordSlavesList>(request.text);

            foreach (DiscordSlave slave in slaves.data)
            {
                try
                {
                    Actor actorById = MapBox.instance.getActorByID(slave.firstName);
                    WorldLog.locationFollow(actorById);
                    Debug.Log("watching: " + slave.nickname);

                } catch (Exception e)
                {
                    continue;
                }

                yield return new WaitForSeconds(20.0f);

            };

            Debug.Log("centering camera");
            //Camera.main.transform.position.x = (float)(MapBox.width / 2);
            // Camera.main.transform.position.y = (float)(MapBox.height / 2
            MoveCamera.focusUnit = null;
            Reflection.CallMethod(MapBox.instance, "centerCamera");

            yield return new WaitForSeconds(30.0f);

            initCamera();
            yield return true;
        }


        public static int windowInUse = 0;
        public static Rect mainWindowRect = new Rect(0f, 1f, 1f, 1f);

        public static ConfigEntry<string> databaseEndpoint
        {
            get; set;
        }
        public static ConfigEntry<bool> showHideMainWindowConfig
        {
            get; set;
        }
        public static ConfigEntry<bool> isSpawningSlaves
        {
            get; set;
        }
        public static ConfigEntry<bool> isCheckingUpdates
        {
            get; set;
        }
        public static ConfigEntry<bool> isRenderingMap
        {
            get; set;
        }
        public static ConfigEntry<string> activeSlaves
        {
            get; set;
        }
    }
}
