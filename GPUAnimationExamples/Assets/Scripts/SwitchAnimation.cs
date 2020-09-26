using Unity.Entities;
using UnityEngine;

public class SwitchAnimation : SystemBase
{
    protected override void OnUpdate()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, 100000));

            Entities.ForEach((ref SimpleAnim simpleAnim) => 
            {
                simpleAnim.ClipIndex = random.NextInt(0, 2);
            }).Run();
        }
    }
}
