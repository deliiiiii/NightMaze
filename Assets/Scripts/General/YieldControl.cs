using System.Threading.Tasks;

namespace General
{
    public class YieldControl
    {
        public YieldControl(float yieldCount)
        {
            this.yieldCount = yieldCount;
        }

        readonly float yieldCount;
        int countNotYieldFrame;

        public async Task YieldFrames(float multi = 1)
        {
            var trueY = yieldCount * multi;
            if (trueY >= 1)
            {
                for (int y = 0; y < trueY; y++)
                {
                    await Task.Yield();
                }
                return;
            }

            countNotYieldFrame++;
            if (1f / countNotYieldFrame <= trueY)
            {
                countNotYieldFrame = 0;
                await Task.Yield();
            }
        }
    }
}