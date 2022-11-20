using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using MEC;
using Synapse;
using Synapse.Api;
using Synapse.Api.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLEnforcedRNG.Modules
{
    public class SchematicsModule : BaseModule
    {
        //-------------------------------------------------------------------------------
        //Overrides
        public override string ModuleName { get { return "Schematics"; } }
        public override void Activate()
        {
        }
        public override void SetUpRound()
        {
            //ClearSchematics();
            schematics.Clear();
            SpawnArmoryInDoggoRoom();
        }

        //-------------------------------------------------------------------------------
        //Main
        public static List<Synapse.Api.CustomObjects.SynapseObject> schematics = new();

        public static void ClearSchematics()
        {
            foreach(var schem in schematics)
            {
                schem.Destroy();
            }
            schematics.Clear();
        }
        public static Synapse.Api.CustomObjects.SynapseObject CreateSchematicInRoom(Room room, int schematicID, Vector3 subjectivePos, Vector3 rotation)
        {
            Vector3 finalPos = new();
            var mainPos = room.Position;
            DebugTranslator.Console("angle: " + room.Rotation.eulerAngles.y.ToString());
            if (room.Rotation.eulerAngles.y == 0)   { finalPos.x = subjectivePos.z; finalPos.y = subjectivePos.y; finalPos.z = subjectivePos.x; }
            if (room.Rotation.eulerAngles.y == 90)  { finalPos.x = subjectivePos.x; finalPos.y = subjectivePos.y; finalPos.z = subjectivePos.z; }
            if (room.Rotation.eulerAngles.y == 180) { finalPos.x = -subjectivePos.z; finalPos.y = subjectivePos.y; finalPos.z = -subjectivePos.x; }
            if (room.Rotation.eulerAngles.y == 270) { finalPos.x = -subjectivePos.x; finalPos.y = subjectivePos.y; finalPos.z = -subjectivePos.z; }
            //DebugTranslator.Console("w" + room.Rotation.w + " x" + room.Rotation.x + " y" + room.Rotation.y + " z" + room.Rotation.z);

            var schematic = Server.Get.Schematic.SpawnSchematic(schematicID, mainPos+finalPos, room.Rotation*Quaternion.Euler(rotation));
            schematics.Add(schematic);

            return schematic;
        }
        //-------------------------------------------------------------------------------
        //Rooms
        public static void SpawnArmoryInDoggoRoom()
        {
            var room = Map.Get.Rooms.First(room => room.RoomType == RoomName.Hcz939);
            var schematic = CreateSchematicInRoom(room, 4, new Vector3(2f, -6f, 0f), new Vector3(0f, 90f, 0f));
            foreach(var door in schematic.DoorChildrens)
            {
                door.Door.DoorPermissions.RequiredPermissions = KeycardPermissions.ArmoryLevelThree;
            }
            foreach(var item in schematic.ItemChildrens)
            {
                if (item.Item.ItemType == ItemType.Ammo9x19) item.Item.Durabillity = 30;
                if (item.Item.ItemType == ItemType.Ammo556x45) item.Item.Durabillity = 60;
                if (item.Item.ItemType == ItemType.Ammo762x39) item.Item.Durabillity = 40;
                if (item.Item.ItemType == ItemType.ParticleDisruptor) { item.Item.Durabillity = 5; }
            }
        }



        //-------------------------------------------------------------------------------
        //Events


        //-------------------------------------------------------------------------------
        //Coords
        //2 -6 0 0 90 0
    }
}
