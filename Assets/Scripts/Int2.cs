public struct Int2
{
    public readonly int x, y;

    public Int2(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override string ToString()
    {
        return string.Format("({0}, {1})", x, y);
    }

    public static Int2 operator -(Int2 v)
    {
        return new Int2(-v.x, -v.y);
    }

    public static Int2 operator +(Int2 a, Int2 b)
    {
        return new Int2(
            a.x + b.x,
            a.y + b.y);
    }

    public static bool operator ==(Int2 a, Int2 b)
    {
        return (
            a.x == b.x &&
            a.y == b.y);
    }

    public static bool operator !=(Int2 a, Int2 b)
    {
        return !(a == b);
    }
}
