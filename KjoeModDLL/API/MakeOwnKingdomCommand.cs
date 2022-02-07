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
    internal class RevoltCommands : MonoBehaviour
    {
        public string getRevoltCmd = KjoeMod.databaseEndpoint.Value + "/api/slaves/revolt";
        public void startCheckRevoltCmd()
        {
                print("Starting checking revolt" + Time.time);
                WWW request = new WWW(getRevoltCmd);
                StartCoroutine(checkRevoltCmd(request));
        }

        IEnumerator checkRevoltCmd(WWW request)
        {
            while (!request.isDone)
            {
                yield return new WaitForSeconds(20.0f);
            }

            DiscordSlavesList slaves = JsonConvert.DeserializeObject<DiscordSlavesList>(request.text);

            foreach (DiscordSlave slave in slaves.data)
            {
                try
                {
                    Actor actor = MapBox.instance.getActorByID(slave.firstName);

                    if (actor.kingdom.count_cities <= 1)
                    {
                        WorldTip.showNow(actor.name + " could not revolt, kingdom must have spare cities!", false, "top", 10);

                        startCheckRevoltCmd();
                        yield break;
                    }


                    WorldTip.showNow(actor.name + " Revolted!", false, "top", 10);
                    Reflection.CallMethod(actor.city, "makeOwnKingdom");
                }
                catch (Exception e)
                {

                }
            }


            startCheckRevoltCmd();
            yield return true;
        }

    }
}
