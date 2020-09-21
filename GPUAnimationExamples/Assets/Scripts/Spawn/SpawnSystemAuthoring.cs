using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class SpawnSystemAuthoring : MonoBehaviour
{
    public int spawnAmount;
    public float interleave;

    private void Awake()
    {
        SpawnSystem spawnSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<SpawnSystem>();

        spawnSystem.spawnAmount = spawnAmount;
        spawnSystem.interleave = interleave;
    }
}

public class SpawnSystem : SystemBase
{
    public int spawnAmount;
    public float interleave;

    protected override void OnStartRunning()
    {
        // calculate sqrt so we can spawn N * N = Amount
        float sqrt = math.sqrt(spawnAmount);

        // calculate spawn xz start positions
        // based on spawnAmount * interleave
        float offset = -sqrt / 2 * interleave;

        int spawned = 0;
        for (int spawnX = 0; spawnX < sqrt; ++spawnX)
        {
            for (int spawnZ = 0; spawnZ < sqrt; ++spawnZ)
            {
                if (spawned < spawnAmount)
                {
                    Entities.ForEach((SpawnSystemPrefabComponent spawnSystemPrefabComponent) =>
                    {
                        Entity spawnedEntity = EntityManager.Instantiate(spawnSystemPrefabComponent.prefabEntity);

                        float x = offset + spawnX * interleave;
                        float z = offset + spawnZ * interleave;
                        float3 position = new float3(x, 0, z);
                        EntityManager.SetComponentData(spawnedEntity, new Translation { Value = position });

                        //SetComponent(spawnedEntity, new Translation { Value = position });

                        ++spawned;
                    })
                    .WithStructuralChanges()                     
                    .Run();
                }
            }
        }
    }

    protected override void OnUpdate() { }
}
