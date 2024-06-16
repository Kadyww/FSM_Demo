using System.Collections.Generic;

public static class Extentions
{
    //list add unique
    public static void AddUnique<T>(this List<T> list, T item)
    {
        if (!list.Contains(item)) list.Add(item);
    }
}