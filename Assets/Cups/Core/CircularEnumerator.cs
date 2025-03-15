using System.Collections.Generic;

class CircularEnumerator<T> : IEnumerator<T>
{
    private readonly List<T> list;
    int i = 0;

    public CircularEnumerator(List<T> list)
    {
        this.list = list;
    }

    public T Current => list[i];

    object System.Collections.IEnumerator.Current => this;

    public void Dispose()
    {

    }

    public bool MoveNext()
    {
        i = (i + 1) % list.Count;
        return true;
    }

    public void Reset()
    {
        i = 0;
    }
}