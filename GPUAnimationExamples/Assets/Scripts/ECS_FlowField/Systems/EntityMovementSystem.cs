using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace TMG.ECSFlowField
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class EntityMovementSystem : SystemBase
    {
        public static EntityMovementSystem instance;
        
        private EntityQuery flowFieldQuery;
        private Entity flowFieldEntity;
        private FlowFieldData flowFieldData;
        private DestinationCellData destinationCellData;
        private DynamicBuffer<EntityBufferElement> entityBuffer;
        private DynamicBuffer<Entity> gridEntities;
        private static NativeArray<CellData> cellDataContainer;
        
        protected override void OnCreate()
        {
            instance = this;
        }

        public void SetMovementValues()
        {
            flowFieldQuery = GetEntityQuery(typeof(FlowFieldData));
            flowFieldEntity = flowFieldQuery.GetSingletonEntity();
            flowFieldData = EntityManager.GetComponentData<FlowFieldData>(flowFieldEntity);
            destinationCellData = EntityManager.GetComponentData<DestinationCellData>(flowFieldEntity);
            entityBuffer = EntityManager.GetBuffer<EntityBufferElement>(flowFieldEntity);
            gridEntities = entityBuffer.Reinterpret<Entity>();
            
            if (cellDataContainer.IsCreated)
            {
                cellDataContainer.Dispose();
            }
            cellDataContainer = new NativeArray<CellData>(gridEntities.Length, Allocator.Persistent);
            
            for (int i = 0; i < entityBuffer.Length; i++)
            {
                cellDataContainer[i] = GetComponent<CellData>(entityBuffer[i]);
            }
            
            Entities.ForEach((ref EntityMovementData entityMovementData) =>
            {
                entityMovementData.destinationReached = false;
            }).Run();
        }

        protected override void OnUpdate()
        {
            if (flowFieldEntity.Equals(Entity.Null)) { return; }

            float deltaTime = Time.DeltaTime;
            FlowFieldData _flowFieldData = flowFieldData;
            int2 destinationCell = destinationCellData.destinationIndex;

            NativeArray<CellData> _cellDataContainer = cellDataContainer;

            Entities.ForEach((ref PhysicsVelocity physVelocity, ref EntityMovementData entityMovementData, 
                ref Translation translation) =>
            {
                int2 curCellIndex = FlowFieldHelper.GetCellIndexFromWorldPos(translation.Value, _flowFieldData.gridSize,
                    _flowFieldData.cellRadius * 2);

                if (curCellIndex.Equals(destinationCell))
                {
                    entityMovementData.destinationReached = true;
                }

                int flatCurCellIndex = FlowFieldHelper.ToFlatIndex(curCellIndex, _flowFieldData.gridSize.y);
                float2 moveDirection = _cellDataContainer[flatCurCellIndex].bestDirection;
                float finalMoveSpeed = (entityMovementData.destinationReached ? entityMovementData.destinationMoveSpeed : entityMovementData.moveSpeed) * deltaTime;

                float2 movement = moveDirection * finalMoveSpeed;

                //translation.Value += new float3(movement.x, 0, movement.y);

                physVelocity.Linear.xz = moveDirection * finalMoveSpeed;
                translation.Value.y = 0f;

            }).Run();


            // JobHandle jobHandle = new JobHandle();

            // jobHandle = Entities.ForEach((ref PhysicsVelocity physVelocity, ref EntityMovementData entityMovementData, 
            //     ref Translation translation) =>
            // {
            //     int2 curCellIndex = FlowFieldHelper.GetCellIndexFromWorldPos(translation.Value, flowFieldData.gridSize,
            //         flowFieldData.cellRadius * 2);

            //     if (curCellIndex.Equals(destinationCell))
            //     {
            //         entityMovementData.destinationReached = true;
            //     }

            //     int flatCurCellIndex = FlowFieldHelper.ToFlatIndex(curCellIndex, flowFieldData.gridSize.y);
            //     float2 moveDirection = _cellDataContainer[flatCurCellIndex].bestDirection;
            //     float finalMoveSpeed = (entityMovementData.destinationReached ? entityMovementData.destinationMoveSpeed : entityMovementData.moveSpeed) * deltaTime;

            //     physVelocity.Linear.xz = moveDirection * finalMoveSpeed;
            //     translation.Value.y = 0f;

            // }).ScheduleParallel(jobHandle);

            // //TODO: Do we need to complete??
            //jobHandle.Complete();
        }

        protected override void OnDestroy()
        {
            if (cellDataContainer.IsCreated)
            {
                cellDataContainer.Dispose();
            }
        }
    }
}