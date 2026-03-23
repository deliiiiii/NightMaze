namespace GeneralPreview;

public static class FloatExt
{
    extension(float)
    {
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
    }
}