using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using UnityEngine;

namespace SlaveMod
{
    internal class FollowSlave
    {
        public static void followActor(Actor target, float speed = 5f)
        {
            Vector3 posV = target.currentPosition;
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, posV, speed * Time.deltaTime);
        }
    }
}
