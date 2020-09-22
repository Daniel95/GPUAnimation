using Unity.Entities;
using Unity.Physics;

public class PhysicsConstraintsSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((
            ref PhysicsMass physicsMass,
            in Entity entity,
            in PhysicsConstraintsComponent physicsConstraint
            ) =>
        {
            physicsMass.InverseInertia[0] = physicsConstraint.LockX ? 0 : physicsMass.InverseInertia[0];
            physicsMass.InverseInertia[1] = physicsConstraint.LockY ? 0 : physicsMass.InverseInertia[1];
            physicsMass.InverseInertia[2] = physicsConstraint.LockZ ? 0 : physicsMass.InverseInertia[2];

            EntityManager.RemoveComponent<PhysicsConstraintsComponent>(entity);
        })
        .WithStructuralChanges()
        .Run();
    }
}
