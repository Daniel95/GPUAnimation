using Unity.Entities;

[GenerateAuthoringComponent]
public struct PhysicsConstraintsComponent : IComponentData
{
    public bool LockX;
    public bool LockY;
    public bool LockZ;
}
