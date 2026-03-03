namespace GeneralPreview;

public static class ObjectExt
{
    extension(object self)
    {
        public MyOption<T> Of<T>() => self is T t ? t : None;
    }
}