using System;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

namespace SlaveMod
{
    public class SpawnSlave
    { 
        public bool action_spawnSlave()
        {
            try
            {
                var Api = new GameObject("check").AddComponent<CheckSlaveSpawn>();
                Api.StartGenerating();
            } catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return true;
        }

    }
}