using Unity.Entities;
using Unity.Mathematics;

namespace TMG.ECSFlowField
{
	public class InitializeFlowFieldGridSystem : SystemBase
	{
		private EntityCommandBufferSystem ecbSystem;
		private static EntityArchetype cellArchetype;
		
		protected override void OnCreate()
		{
			ecbSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
			cellArchetype = EntityManager.CreateArchetype(typeof(CellData));
		}

		protected override void OnUpdate()
		{
			EntityArchetype _entityArchetype = cellArchetype;
			EntityCommandBuffer commandBuffer = ecbSystem.CreateCommandBuffer();

			//TODO: find a way to access CostFieldHelper outside of this function so we can use burst.
			Entities.WithoutBurst().ForEach((Entity entity, in NewFlowFieldData newFlowFieldData, in FlowFieldData flowFieldData) =>
			{
				commandBuffer.RemoveComponent<NewFlowFieldData>(entity);

				DynamicBuffer<EntityBufferElement> buffer = newFlowFieldData.isExistingFlowField
					? GetBuffer<EntityBufferElement>(entity)
					: commandBuffer.AddBuffer<EntityBufferElement>(entity);
				DynamicBuffer<Entity> entityBuffer = buffer.Reinterpret<Entity>();

				float cellRadius = flowFieldData.cellRadius;
				float cellDiameter = cellRadius * 2;

				int2 gridSize = flowFieldData.gridSize;

				for (int x = 0; x < gridSize.x; x++)
				{
					for (int y = 0; y < gridSize.y; y++)
					{
						float3 cellWorldPos = new float3(cellDiameter * x + cellRadius, 0, cellDiameter * y + cellRadius);
						byte cellCost = CostFieldHelper.instance.EvaluateCost(cellWorldPos, cellRadius);
						CellData newCellData = new CellData
						{
							worldPos = cellWorldPos,
							gridIndex = new int2(x, y),
							cost = cellCost,
							bestCost = ushort.MaxValue,
							bestDirection = int2.zero
						};

						Entity curCell;
						if (newFlowFieldData.isExistingFlowField)
						{
							int flatIndex = FlowFieldHelper.ToFlatIndex(new int2(x, y), gridSize.y);
							curCell = entityBuffer[flatIndex];
						}
						else
						{
							curCell = commandBuffer.CreateEntity(_entityArchetype);
							entityBuffer.Add(curCell);
						}
						commandBuffer.SetComponent(curCell, newCellData);
					}
				}

				int2 destinationIndex = FlowFieldHelper.GetCellIndexFromWorldPos(flowFieldData.clickedPos, gridSize, cellDiameter);
				DestinationCellData newDestinationCellData = new DestinationCellData{ destinationIndex = destinationIndex};
				if (!newFlowFieldData.isExistingFlowField)
				{
					commandBuffer.AddComponent<DestinationCellData>(entity);
				}
				commandBuffer.SetComponent(entity, newDestinationCellData);
				commandBuffer.AddComponent<CalculateFlowFieldTag>(entity);
			}).Run();
		}

        //public static int ToFlatIndex(int2 index2D, int height)
        //{
        //    return height * index2D.x + index2D.y;
        //}

        //public static int2 GetCellIndexFromWorldPos(float3 worldPos, int2 gridSize, float cellDiameter)
        //{
        //    float percentX = worldPos.x / (gridSize.x * cellDiameter);
        //    float percentY = worldPos.z / (gridSize.y * cellDiameter);

        //    percentX = math.clamp(percentX, 0f, 1f);
        //    percentY = math.clamp(percentY, 0f, 1f);

        //    int2 cellIndex = new int2
        //    {
        //        x = math.clamp((int)math.floor((gridSize.x) * percentX), 0, gridSize.x - 1),
        //        y = math.clamp((int)math.floor((gridSize.y) * percentY), 0, gridSize.y - 1)
        //    };

        //    return cellIndex;
        //}
    }
}