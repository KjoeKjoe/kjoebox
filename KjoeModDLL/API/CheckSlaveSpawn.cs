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
using BepInEx;
using BepInEx.Configuration;


namespace SlaveMod
{
    public class CheckSlaveSpawn : MonoBehaviour
    {
        public bool checking = false;
        public Image mapImage;
        public string renderMapUrl = KjoeMod.databaseEndpoint.Value + "/api/map/render";
        public string getSlaveUrl = KjoeMod.databaseEndpoint.Value + "/api/slaves/get";
        public string getSlavesUrl = KjoeMod.databaseEndpoint.Value + "/api/slaves/all";
        public string postSlaveDied = KjoeMod.databaseEndpoint.Value + "/api/slaves/died";
        public string postSlaveStats = KjoeMod.databaseEndpoint.Value + "/api/slaves/stats/update";

        //public string renderMapUrl = "https://worldboxweb.pixelists.nl/api/map/render";
        //public string getSlaveUrl = "https://worldboxweb.pixelists.nl/api/slaves/get";
        //public string getSlavesUrl = "https://worldboxweb.pixelists.nl/api/slaves/all";
        //public string postSlaveDied = "https://worldboxweb.pixelists.nl/api/slaves/died";
        //public string postSlaveStats = "https://worldboxweb.pixelists.nl/api/slaves/stats/update";
        public void StartGenerating()
        {
            if (KjoeMod.isSpawningSlaves.Value) { 
                print("Starting the Generation " + Time.time);
                WWW request = new WWW(getSlaveUrl);
                StartCoroutine(GenerateDiscordCivUnit(request, 10.0f));
            }
        }
        public void StartChecking()
        {
            if (KjoeMod.isCheckingUpdates.Value && !this.checking)
            { 
                print("Starting checking stats" + Time.time);
                WWW request = new WWW(getSlavesUrl);
                StartCoroutine(checkDiscordCivUnitStats(request, 40.0f));
            }
        }
        public void StartRenderingMap()
        {
            MapLayer unitMapLayer = Reflection.GetField(MapBox.instance.GetType(), MapBox.instance, "unitLayer") as MapLayer;
            Texture2D unitTexture = Reflection.GetField(unitMapLayer.GetType(), unitMapLayer, "texture") as Texture2D;
            
            //Image mapImage = GetComponent<Image>();
            Texture2D map = PreviewHelper.convertMapToTexture();

            //Sprite mapSprite = Sprite.Create(pixels, new Rect(0.0f, 0.0f, (float)pixels.width, (float)pixels.height), new Vector2(0.0f, 0.0f));            
            byte[] mapPng = map.EncodeToPNG();
            byte[] unitPng = unitTexture.EncodeToPNG();

            WWWForm form1 = new WWWForm();
            form1.AddBinaryData("image", mapPng);
            form1.AddField("type", "map");

            WWWForm form2 = new WWWForm();
            form2.AddBinaryData("image", mapPng);
            form2.AddField("type", "map");

            if (KjoeMod.isRenderingMap.Value)
            {
                print("Starting rendering Map" + Time.time);
                WWW request1 = new WWW(renderMapUrl, form1);
                WWW request2 = new WWW(renderMapUrl, form2);
            }
        }
        IEnumerator GenerateDiscordCivUnit(WWW request, float waitTime)
        {
            while (!request.isDone)
            {
                yield return new WaitForSeconds(waitTime);
            }
             
            if (request.text == "no spawns")
            {
                StartChecking();
                KjoeMod.isSpawningSlaves.Value = false;
                yield return true;
            }

            if (request.text != "no spawns")
            {
                Debug.Log(request.text);
                DiscordSlave slave = JsonConvert.DeserializeObject<DiscordSlave>(request.text);

                Debug.Log("spawning unit");
                Debug.Log(slave);
                var spawn = new SpawnSlaveOnWorld();
                spawn.generateSlaveOnWorld(slave, "spawnSlave");

                Debug.Log(slave.firstName);

                StartGenerating();
                yield return true;
            }
        }

        IEnumerator checkDiscordCivUnitStats(WWW request, float waitTime)
        {
            this.checking = true;
            while (!request.isDone)
            {
                yield return new WaitForSeconds(waitTime);
            }


            if (request.text == "{\"data\": []}")
                yield return new WaitForSeconds(waitTime);

            DiscordSlavesList slaves = JsonConvert.DeserializeObject<DiscordSlavesList>(request.text);
            bool deaths = false;
            List<DiscordSlave> deadSlaves = new List<DiscordSlave>();

            foreach (DiscordSlave slave in slaves.data)
            {
                try
                {
                    Actor actorById = MapBox.instance.getActorByID(slave.firstName);

                    if (actorById.kingdom != null)
                    {

                       if (actorById.kingdom.count_units > 200 && actorById.kingdom.name != slave.firstName)
                        {
                            Reflection.CallMethod(actorById.city, "makeOwnKingdom");
                        }

                        if (actorById.kingdom.name != slave.firstName)
                        {
                            actorById.kingdom.name = slave.nickname;

                            actorById.kingdom.king = actorById;
                        }
                        else
                        {

                        }
                    }

                    ActorStatus data = Reflection.GetField(actorById.GetType(), actorById, "data") as ActorStatus;

                    WWWForm form = new WWWForm();
                    form.AddField("firstName", slave.firstName);
                    form.AddField("age", data.age);
                    form.AddField("children", data.children);
                    form.AddField("kills", data.kills);
                    form.AddField("diplomacy", data.diplomacy);
                    form.AddField("warfare", data.warfare);
                    form.AddField("stewardship", data.stewardship);
                    form.AddField("intelligence", data.intelligence);
                    form.AddField("health", data.health);
                    
                    WWW diedRequest = new WWW(postSlaveStats, form);

                    //if (slave.follow)
                    //{
                    //    WorldLog.locationFollow(actorById);
                    //}

                }
                catch (Exception ex)
                {
                    deaths = true;
                    deadSlaves.Add(slave);
                }

            }

            if (deaths)
            {
                foreach (DiscordSlave deadSlave in deadSlaves)
                {
                    WWWForm form = new WWWForm();
                    form.AddField("firstName", deadSlave.firstName);

                    yield return new WaitForSeconds(2.0f);
                    WWW diedRequest = new WWW(postSlaveDied, form);
                    StartCoroutine(slaveRespawn(diedRequest, 10.0f));

                }
                KjoeMod.isSpawningSlaves.Value = true;
                StartGenerating();
            }

            print("Starting checking completed " + Time.time);
            this.checking = false;
            StartChecking();
            yield return true;
        }


        IEnumerator slaveRespawn(WWW request, float waitTime)
        {
            while (!request.isDone)
            {
                yield return new WaitForSeconds(waitTime);
            }
        }

        public static object GetPropValue(Actor src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }
    }
}
