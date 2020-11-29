using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Line_of_Sight
{
    [BurstCompile]
    public struct GetUnobstructedTiles : IJobParallelFor
    {
        // Used in line of sight calculations. Bresenham's line algorithm.

        [ReadOnly] public int CameraWidth;
        [ReadOnly] public float2 Origin, BottomLeft;
        [ReadOnly] public NativeHashSet<int2> WallsInView;

        [WriteOnly] public NativeArray<bool> WallVisibility;

        public void Execute(int index)
        {
            var end = BottomLeft + new float2(index % CameraWidth + 0.5f, index / CameraWidth + 0.5f);

            // https://gamedev.stackexchange.com/questions/81267/how-do-i-generalise-bresenhams-line-algorithm-to-floating-point-endpoints/182143#182143

            //Grid cells are 1.0 X 1.0.
            var x = math.floor(Origin.x);
            var y = math.floor(Origin.y);
            var diffX = end.x - Origin.x;
            var diffY = end.y - Origin.y;
            var stepX = math.sign(diffX);
            var stepY = math.sign(diffY);

            //Ray/Slope related maths.
            //Straight distance to the first vertical grid boundary.
            var xOffset = end.x > Origin.x ? math.ceil(Origin.x) - Origin.x : Origin.x - math.floor(Origin.x);
            //Straight distance to the first horizontal grid boundary.
            var yOffset = end.y > Origin.y ? math.ceil(Origin.y) - Origin.y : Origin.y - math.floor(Origin.y);
            //Angle of ray/slope.
            var angle = math.atan2(-diffY, diffX);
            //How far to move along the ray to cross the first vertical grid cell boundary.
            var tMaxX = xOffset / math.cos(angle);
            //How far to move along the ray to cross the first horizontal grid cell boundary.
            var tMaxY = yOffset / math.sin(angle);
            //How far to move along the ray to move horizontally 1 grid cell.
            var tDeltaX = 1 / math.cos(angle);
            //How far to move along the ray to move vertically 1 grid cell.
            var tDeltaY = 1 / math.sin(angle);

            //Travel one grid cell at a time.
            var manhattanDistance = (int) (math.abs(math.floor(end.x) - math.floor(Origin.x)) +
                                           math.abs(math.floor(end.y) - math.floor(Origin.y)));
            for (var t = 0; t <= manhattanDistance; ++t)
            {
                // If collision with wall tile, "early" exit.
                if (WallsInView.Contains(new int2((int) x, (int) y)))
                {
                    // End point at walls are designated as visible if center is seen.
                    WallVisibility[index] = t == manhattanDistance;
                    return;
                }

                //Only move in either X or Y coordinates, not both.
                if (math.abs(tMaxX) < math.abs(tMaxY))
                {
                    tMaxX += tDeltaX;
                    x += stepX;
                }
                else
                {
                    tMaxY += tDeltaY;
                    y += stepY;
                }
            }

            // All checks pass, visible.
            WallVisibility[index] = true;
        }
    }
}