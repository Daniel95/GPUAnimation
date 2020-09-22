using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public class EntitySpawnerTestAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private int numUnitsPerSpawn;
    [SerializeField] private float2 maxSpawnPos;

    private void Awake()
    {
        EntitySpawnerTest spawnSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<EntitySpawnerTest>();

        spawnSystem.unitPrefab = unitPrefab;
        spawnSystem.numUnitsPerSpawn = numUnitsPerSpawn;
        spawnSystem.maxSpawnPos = maxSpawnPos;
    }
}

public class EntitySpawnerTest : SystemBase
{
    public GameObject unitPrefab;
    public int numUnitsPerSpawn;
    public float2 maxSpawnPos;

    //private List<Entity> unitsInGame;
    private int colMask;

    protected override void OnUpdate() 
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            //EntityMovementData newEntityMovementData = new EntityMovementData
            for (int i = 0; i < numUnitsPerSpawn; i++)
            {
                Entities.ForEach((in SpawnSystemPrefabComponent spawnSystemPrefabComponent) =>
                {
                    var newUnit = EntityManager.Instantiate(spawnSystemPrefabComponent.prefabEntity);
                    //EntityManager.SetComponentData(newUnit, newEntityMovementData);
                    //unitsInGame.Add(newUnit);
                    float3 newPosition;
                    do
                    {
                        newPosition = new float3(Random.Range(0f, maxSpawnPos.x), 0, Random.Range(0, maxSpawnPos.y));
                        EntityManager.SetComponentData(newUnit, new Translation { Value = newPosition });
                    } while (Physics.OverlapSphere(newPosition, 0.25f, colMask).Length > 0);
                })
                .WithStructuralChanges()
                .WithoutBurst()
                .Run();
            }
        }

        //if (Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    foreach (Entity entity in unitsInGame)
        //    {
        //        EntityManager.DestroyEntity(entity);
        //    }
        //    unitsInGame.Clear();
        //}
    }
}