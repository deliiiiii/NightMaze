using UnityEngine;

namespace GeneralPreview;

public static class ColorExt
{
    extension(Color self)
    {
        public Color SetAlpha(float a)
        {
            self.a = a;
            return self;
        }
    }
}