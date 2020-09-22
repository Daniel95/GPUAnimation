using Unity.Entities;

namespace TMG.ECSFlowField
{
    [GenerateAuthoringComponent]
    public struct EntityMovementData : IComponentData
    {
        public float moveSpeed;
        public float destinationMoveSpeed;
        public bool destinationReached;
    }
}