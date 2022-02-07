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
    internal class KingdomCommands : MonoBehaviour
    {
        public string getWarCmd = KjoeMod.databaseEndpoint.Value + "/api/slaves/kingdom/war/cmd";
        public void startCheckPeaceWarCmd()
        {
                print("Starting checking kingdom peace and war" + Time.time);
                WWW request = new WWW(getWarCmd);
                StartCoroutine(checkMakeWarCmd(request));
        }

        IEnumerator checkMakeWarCmd(WWW request)
        {
            while (!request.isDone)
            {
                yield return new WaitForSeconds(20.0f);
            }

            if (request.text == "no cmd")
            {
                startCheckPeaceWarCmd();
                yield break;
            }

            StartWarCmd data = JsonConvert.DeserializeObject<StartWarCmd>(request.text);

            ActorBase player1 = MapBox.instance.getActorByID(data.player1);
            ActorBase player2 = MapBox.instance.getActorByID(data.player2);

            //Debug.Log("player " + data.player1);
            //Debug.Log("player " + data.player2);
            //Debug.Log("type " + data.type);

            ActorStatus data1 = Reflection.GetField(player1.GetType(), player1, "data") as ActorStatus;
            ActorStatus data2 = Reflection.GetField(player2.GetType(), player2, "data") as ActorStatus;

            if (player2 == null)
            {
                startCheckPeaceWarCmd();
                yield break;
            }

            if (data.type == "war")
            {
                WorldTip.showNow(data1.firstName + " declared war to " + data2.firstName, false, "top", 3);
                Reflection.CallMethod(MapBox.instance.kingdoms.diplomacyManager, "startWar", new object[] { player1.kingdom, player2.kingdom, true });
            }

            if (data.type == "peace")
            {
                WorldTip.showNow(data1.firstName + " made peace with " + data2.firstName, false, "top", 3);
                Reflection.CallMethod(MapBox.instance.kingdoms.diplomacyManager, "startPeace", new object[] { player1.kingdom, player2.kingdom, true });
            }

            startCheckPeaceWarCmd();
            yield return true;
        }

    }
}
