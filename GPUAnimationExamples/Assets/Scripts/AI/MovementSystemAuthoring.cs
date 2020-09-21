using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class MovementSystemAuthoring : MonoBehaviour
{
    private void Awake()
    {
    }
}

public class MonsterMovementSystem : SystemBase
{
    protected override void OnStartRunning()
    {
        // set up the start positions once
        Entities.ForEach((ref MovementComponent movement,
                            in Translation translation) =>
        {
            movement.startPosition = translation.Value;
        })
        .Run();
    }

    protected override void OnUpdate()
    {
        // new random for each update
        // (time+1 because seed must be non-zero to avoid exceptions)
        uint seed = 1 + (uint)Time.ElapsedTime;
        Unity.Mathematics.Random random = new Unity.Mathematics.Random(seed);

        // foreach
        float deltaTime = Time.DeltaTime;
        Entities.ForEach((ref Translation translation,
                            ref MovementComponent movement) =>
        {
            // are we moving?
            if (movement.isMoving)
            {
                // check if destination was reached yet
                if (math.distance(translation.Value, movement.destination) <= 0.01f)
                {
                    movement.isMoving = false;
                }
                // otherwise move towards destination
                else
                {
                    translation.Value = Movetowards(translation.Value, movement.destination, movement.speed * deltaTime);
                }
            }
            // we are not moving
            else
            {
                // move this time?
                float r = random.NextFloat(); // [0,1)
                if (r <= movement.moveProbability * deltaTime)
                {
                    // calculate random destination in moveDistance
                    float2 circle2D = random.NextFloat2Direction();
                    float3 direction = new float3(circle2D.x, 0, circle2D.y);
                    float3 destination = translation.Value + direction * movement.moveDistance;

                    // only go there if it's within a circle around start
                    // so we don't wander off into nirvana
                    if (math.distance(movement.startPosition, destination) <= movement.moveDistance)
                    {
                        movement.destination = destination;
                        movement.isMoving = true;
                    }
                }
            }
        })
        .Schedule();
    }

    public static float3 Movetowards(float3 current, float3 target, float step)
    {
        // calculate direction
        float3 direction = target - current;

        // calculate distance
        float distance = math.distance(target, current);

        // return target if we would move beyond it
        if (distance <= step)
            return target;

        // move closer
        return current + math.normalizesafe(direction) * step;
    }
}
