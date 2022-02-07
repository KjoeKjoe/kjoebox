using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using HarmonyLib;
using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine.Networking;
using BepInEx;
using BepInEx.Configuration;

namespace SlaveMod
{
    internal class EventCommands : MonoBehaviour
    {
        public string getEventCmd = KjoeMod.databaseEndpoint.Value  + "/api/slaves/event/cmd";
        public void startCheckEventCmd()
        {
            print("Starting checking Events " + Time.time);
            WWW request = new WWW(getEventCmd);
            StartCoroutine(checkEventCmd(request));
        }

        IEnumerator checkEventCmd(WWW request)
        {
            print("check request " + Time.time);
            while (!request.isDone)
            {
                yield return new WaitForSeconds(20.0f);
            }
            print("Work on response " + Time.time);

            if (request.text == "no event")
            {
                startCheckEventCmd();
                yield break;
            }

            WorldEvent data = JsonConvert.DeserializeObject<WorldEvent>(request.text);

            int height = MapBox.height;
            int width = MapBox.width;

            for (int i = 0; i < data.times ; i++)
            {
                var randHeight = UnityEngine.Random.Range(0, height);
                var randWidth = UnityEngine.Random.Range(0, width);
                WorldTile worldTile = new WorldTile(randWidth, randHeight, 1, MapBox.instance);

                DisasterAsset disaster = new DisasterAsset();
                disaster.type = DisasterType.Nature;
                disaster.min_world_population = 0;
                DisasterLibrary lib = new DisasterLibrary();

                WorldTip.showNow("Viewers spawned " + data.eventName + " x" + data.times, false, "top", 10);
                if (data.eventName == "Meteor")
                {
                    lib.spawnMeteorite(disaster);
                }
                if (data.eventName == "Earthquake")
                {
                    MapBox.instance.earthquakeManager.startQuake(worldTile);
                }
                if (data.eventName == "Tornado")
                {
                    lib.spawnTornado(disaster);
                }
                if (data.eventName == "Rain")
                {
                    lib.spawnRainCloud(disaster);
                }
                if (data.eventName == "Mad Thoughts")
                {
                    lib.spawnMadThought(disaster);
                }
                if (data.eventName == "Evil Mage")
                {
                    disaster.type = DisasterType.Other;
                    lib.spawnEvilMage(disaster);
                }
                if (data.eventName == "Greg")
                {
                    disaster.type = DisasterType.Other;
                    City city = MapBox.instance.citiesList?.GetRandom();
                    Building building = city.buildings.GetRandom();
                    if (!(building == null))
                    {
                        WorldTile currentTile = building.currentTile;
                        Actor actor = MapBox.instance.createNewUnit("greg", currentTile, "");
                        WorldLog.logDisaster(disaster, currentTile, "greg", city, actor);
                        int num = Toolbox.randomInt(5, 25);
                        for (int rand = 0; rand < num; rand++)
                        {
                            MapBox.instance.createNewUnit("greg", currentTile.region.tiles.GetRandom(), "");
                        }
                    }
                }
                if (data.eventName == "Hellspawn")
                {
                    disaster.type = DisasterType.Other;
                    lib.spawnHellSpawn(disaster);
                }
                if (data.eventName == "Necromancer")
                {
                    disaster.type = DisasterType.Other;
                    lib.spawnNecromancer(disaster);
                }
            }

            startCheckEventCmd();
            yield return true;
        }

    }
}
