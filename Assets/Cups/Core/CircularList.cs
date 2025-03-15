using System.Collections;
using System.Collections.Generic;

public class CircularList<T> : List<T>, IEnumerable<T>
{
    public new IEnumerator<T> GetEnumerator()
    {
        return new CircularEnumerator<T>(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new CircularEnumerator<T>(this);
    }
}