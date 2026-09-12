using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raincord100k
{
    internal class DebugScavengerMapper : World.WorldProcess
    {
        public static DebugScavengerMapper Instance;

        private LinkedList<RoomNode> nodesToCheckConnectivityFrom = [];
        private HashSet<RoomNode> checkedNodes = [];
        public HashSet<string> accessibleRooms = [];

        private CreatureTemplate scavengerTemplate;
        private Step step = Step.MapScavEntrances;

        public DebugScavengerMapper(World world) : base(world)
        {
            Instance = this;
            scavengerTemplate = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Scavenger);
        }

        public override void Update()
        {
            base.Update();
            switch (step)
            {
                case Step.MapScavEntrances:
                    foreach (var room in world.abstractRooms)
                    {
                        if (room.offScreenDen) continue;

                        for (int i = 0; i < room.nodes.Length; i++)
                        {
                            if (room.nodes[i].type == AbstractRoomNode.Type.RegionTransportation)
                            {
                                nodesToCheckConnectivityFrom.AddLast(new RoomNode(room.name, room.index, i));
                                accessibleRooms.Add(room.name);
                            }
                        }

                    }
                    step = Step.MapRoomConnectivity;
                    break;
                case Step.MapRoomConnectivity:
                    if (nodesToCheckConnectivityFrom.Count > 0)
                    {
                        var currNode = nodesToCheckConnectivityFrom.First.Value;
                        nodesToCheckConnectivityFrom.RemoveFirst();
                        if (!checkedNodes.Add(currNode)) break;

                        var room = world.GetAbstractRoom(currNode.RoomIndex);
                        for (int i = 0; i < room.nodes.Length; i++)
                        {
                            if (room.nodes[i].type == AbstractRoomNode.Type.Exit && room.ConnectivityCost(currNode.Node, i, scavengerTemplate) >= 0f)
                            {
                                int otherSide = room.connections[i];
                                if (otherSide < 0) continue;

                                var otherRoom = world.GetAbstractRoom(otherSide);
                                int otherConn = otherRoom.ExitIndex(currNode.RoomIndex);
                                var otherNode = new RoomNode(otherRoom.name, otherRoom.index, otherConn);
                                if (!checkedNodes.Contains(otherNode))
                                {
                                    nodesToCheckConnectivityFrom.AddLast(otherNode);
                                    accessibleRooms.Add(otherRoom.name);
                                }
                            }
                        }
                    }
                    else
                    {
                        step = Step.Print;
                    }
                    break;
                case Step.Print:
                    StringBuilder sb = new StringBuilder("ROOMS ACCESSIBLE TO SCAVENGERS:\n");
                    var sorted = accessibleRooms.OrderBy(x => x, StringComparer.OrdinalIgnoreCase);
                    foreach ( var room in sorted)
                    {
                        sb.AppendLine(room);
                    }
                    Plugin.Logger.LogMessage(sb.ToString());
                    step = Step.Done;
                    break;
            }
        }

        private record struct RoomNode(string Room, int RoomIndex, int Node);
        private enum Step
        {
            MapScavEntrances,
            MapRoomConnectivity,
            Print,
            Done
        }
    }
}
