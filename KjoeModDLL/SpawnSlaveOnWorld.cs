using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
namespace SlaveMod
{
    public class SpawnSlaveOnWorld
    {
        public bool generateSlaveOnWorld(DiscordSlave slave, string pPower = "")
        {
            // We need some default cloud to use for create out own
            GodPower godPower = AssetManager.powers.get("_spawnActor");



            ActorData aData = new ActorData();
            //Debug.Log(spawnSlave);

            string name = slave.firstName;

            if (slave.nickname.Length > 0)
            {
                name = slave.nickname;
            }

            ActorStatus Status = new ActorStatus
            {
                firstName = name,
                age = 3,
                actorID = slave.firstName,
                statsID = "unit_" + slave.race,
                hunger = 100,
                favorite = true,
                warfare = slave.base_warfare,
                stewardship = slave.base_stewardship,
                intelligence = slave.base_intelligence,
                diplomacy = slave.base_diplomacy
            };


            ActorTrait actorTrait = new ActorTrait();
            actorTrait.baseStats.health = slave.base_health;
            actorTrait.baseStats.attackSpeed = slave.base_attack_speed;
            actorTrait.baseStats.accuracy = slave.base_accuracy;
            actorTrait.baseStats.speed = slave.base_speed;
            actorTrait.baseStats.crit = slave.base_crit;
            actorTrait.baseStats.armor = slave.base_armor;
            actorTrait.inherit = 0f;
            actorTrait.id = "customTrait_" + slave.firstName;
            AssetManager.traits.add(actorTrait);

            Status.addTrait("customTrait_" + slave.firstName);

            DiscordSlaveTraitList traits = JsonConvert.DeserializeObject<DiscordSlaveTraitList>(slave.traits);

            if (traits.trait_data.Any())
            {
                //Debug.Log(traits.trait_data[0]);
                
                foreach (string trait in traits.trait_data)
                {
                    Status.addTrait(trait);
                }

            }

            slave.base_health = slave.base_health;
            aData.status = Status;
            

            List<Building> buildings = MapBox.instance.buildings.getSimpleList();
            List<Actor> units = MapBox.instance.units.getSimpleList();

            int height = MapBox.height;
            int width = MapBox.width;

            var randHeight = UnityEngine.Random.Range(0, height);
            var randWidth = UnityEngine.Random.Range(0, width);

            WorldTile worldTile = new WorldTile(randWidth, randHeight, 1, MapBox.instance);
               
            if (MapBox.instance.kingdoms.list.Count > 0)
            {
                foreach (Kingdom pKingdom in MapBox.instance.kingdoms.list)
                {
                    if (pKingdom.name == slave.nickname)
                    {
                        worldTile = pKingdom.buildings.GetRandom().currentTile.tile_down;
                    } else if (pKingdom.raceID == slave.race && MapBox.instance.kingdoms.list.Count > 5)
                    {
                        worldTile = pKingdom.buildings.GetRandom().currentTile.tile_down;
                    }
                }
            }
            else if (buildings.Any())
            {
                Building building = buildings.GetRandom<Building>();
                worldTile = building.currentTile.tile_down;
                //
                //if (building.kingdom.count_units > 200)
                //{
                //    slave.race = building.kingdom.raceID;
                //}

            } else if (units.Any())
            {
                worldTile = units.GetRandom().currentTile;
            }

            // Check tile for null just in case
            if (worldTile == null)
            {
                // Exit from method if null
                return false;
            }

            WorldTip.showNow("Spawning in " + slave.nickname, false, "top", 1);
            Actor pActor = MapBox.instance.createNewUnit("unit_" + slave.race, worldTile, null, 0, aData);
            pActor.restoreHealth(slave.base_health);
            //Debug.Log(slave.helmet + slave.boots + slave.bodyarmor);

            Dictionary<string, EquipmentType> equipments = new Dictionary<string, EquipmentType>();
            equipments.Add("helmet", EquipmentType.Helmet );
            equipments.Add("ring", EquipmentType.Ring );
            equipments.Add("boots", EquipmentType.Boots );
            equipments.Add("armor", EquipmentType.Armor );
            equipments.Add("amulet", EquipmentType.Amulet );
            equipments.Add("weapon", EquipmentType.Weapon );

            foreach (var equip in equipments)
            {
                AddItemToSlave(equip.Key, equip.Value, pActor, slave);
            }

            setActorStatsDirty(pActor);

            return true;
        }

        public void AddItemToSlave(string item, EquipmentType itemType, Actor pActor, DiscordSlave slave)
        {
           // Debug.Log(item + " " + itemType + pActor.name + slave.firstName + "-----------------");

            string material = slave.bodyarmor;
            if (item != "armor")
            {
                material = slave.GetType().GetProperty(item).GetValue(slave, null) as string;
            }
            //Debug.Log("MATERIAL = " + material+"---------------------");


            if (material == null || material.Length < 1)
            {
                return;
            }


            Dictionary<string, ItemQuality> qualities = new Dictionary<string, ItemQuality>();

            qualities.Add("Junk", ItemQuality.Junk);
            qualities.Add("Normal", ItemQuality.Normal);
            qualities.Add("Rare", ItemQuality.Rare);
            qualities.Add("Epic", ItemQuality.Epic);
            qualities.Add("Legendary", ItemQuality.Legendary);

            List<string> keyList = new List<string>(qualities.Keys);
            string randomKey = keyList.GetRandom();

            //Debug.Log(randomKey + "---------------------");
            ItemQuality pickedQuality = qualities.ElementAt(UnityEngine.Random.Range(0, qualities.Count)).Value;

            ActorEquipmentSlot slot = pActor.equipment.helmet;

            if (itemType == EquipmentType.Weapon)
            {
                return;
            }
            if (itemType == EquipmentType.Ring)
            {
                slot = pActor.equipment.ring;
            }
            if (itemType == EquipmentType.Amulet)
            {
                slot = pActor.equipment.amulet;
            }
            if (itemType == EquipmentType.Boots)
            {
                slot = pActor.equipment.boots;
            }
            if (itemType == EquipmentType.Armor)
            {
                slot = pActor.equipment.armor;
            }

            ItemAsset equipment = AssetManager.items.get(item);
            equipment.quality = pickedQuality;


            //Debug.Log(slot + "---------------------");
            ItemGenerator.generateItem(equipment, material, slot, 1, "discord shop", "god", 1);


        }
        public static void setActorStatsDirty(Actor target)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                    | BindingFlags.Static;
            FieldInfo field = typeof(Actor).GetField("statsDirty", bindFlags);
            field.SetValue(target, true);
        }
    }
}
