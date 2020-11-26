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

        [ReadOnly] public int CamSize; // X size of camera.
        [ReadOnly] public int2 CenterPoint, BottomLeft;
        [ReadOnly] public NativeHashSet<int2> WallsInView;

        [WriteOnly] public NativeArray<bool> TileObservation;

        public void Execute(int index)
        {
            var x0 = CenterPoint.x;
            var y0 = CenterPoint.y;
        
            var target = BottomLeft + new int2(index % CamSize, index / CamSize);
            var x1 = target.x;
            var y1 = target.y;
        
            // Thanks wikipedia.
            var dx = math.abs(x1 - x0);
            var sx = x0 < x1 ? 1 : -1;
            var dy = -math.abs(y1 - y0);
            var sy = y0 < y1 ? 1 : -1;
            var err = dx + dy;  /* error value e_xy */
            while (true)
            {
                if (WallsInView.Contains(new int2(x0, y0)))
                {
                    TileObservation[index] = false;
                    return;
                }
                
                if (x0 == x1 && y0 == y1) 
                    break;
                var e2 = 2 * err;
                if (e2 >= dy) /* e_xy+e_x > 0 */
                    err += dy;
                x0 += sx;
                if (e2 <= dx) /* e_xy+e_y < 0 */
                    err += dx;
                y0 += sy;
            }

            TileObservation[index] = true;
        }
    }
}
