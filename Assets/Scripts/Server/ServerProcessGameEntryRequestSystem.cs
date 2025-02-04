using Unity.Entities;
using Unity.Collections;
using Unity.NetCode;
using TMG.NFE_Tutorial;
using Common;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;

namespace Server
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct ServerProcessGameEntryRequestSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MobaPrefabs>();
            var builder = new EntityQueryBuilder(Allocator.Temp).WithAll<MobaTeamRequest, ReceiveRpcCommandRequest>();
            state.RequireForUpdate(state.GetEntityQuery(builder));
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var championPrefab = SystemAPI.GetSingleton<MobaPrefabs>().Champion;

            foreach (var (teamRequest, requestSource, requestEntity) in 
                SystemAPI.Query<MobaTeamRequest, ReceiveRpcCommandRequest>().WithEntityAccess())
            {
                ecb.DestroyEntity(requestEntity);
                ecb.AddComponent<NetworkStreamInGame>(requestSource.SourceConnection);

                var requestedTeamType = teamRequest.Value;

                if (requestedTeamType == TeamType.AutoAssine)
                {
                    requestedTeamType = TeamType.Blue;
                }

                var clientId = SystemAPI.GetComponent<NetworkId>(requestSource.SourceConnection).Value;

                Debug.Log($"Server is assigning Client ID: {clientId} to the {requestedTeamType.ToString()} team.");
                
                var spawnPosition = new float3(0, 1, 0);

                switch (requestedTeamType)
                {
                    case TeamType.Blue:
                        spawnPosition = new float3(-50f, 1, -50f);
                        break;
                    case TeamType.Red:
                        spawnPosition = new float3(50f, 1, 50f);
                        break;
                    default: continue;
                }

                var newChamp = ecb.Instantiate(championPrefab);
                ecb.SetName(newChamp, "Champion");
                
                var newTransform = LocalTransform.FromPosition(spawnPosition);
                ecb.SetComponent(newChamp, newTransform);
                ecb.SetComponent(newChamp, new GhostOwner
                {
                    NetworkId = clientId
                });
                ecb.SetComponent(newChamp, new MobaTeam
                {
                    Value = requestedTeamType
                });
                ecb.AppendToBuffer(requestSource.SourceConnection, new LinkedEntityGroup
                {
                    Value = newChamp
                });
            }

            ecb.Playback(state.EntityManager);
        }
    }

}