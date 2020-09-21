using Unity.Entities;

[GenerateAuthoringComponent]
public struct SpawnSystemPrefabComponent : IComponentData
{
    public Entity prefabEntity;
}
