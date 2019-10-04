public sealed class Army
{
    public readonly Card c1, c2;
    private readonly Int2[] pieces;

    public Int2 GetPiece(int i)
    {
        return pieces[i];
    }

    public int Size
    {
        get { return pieces.Length; }
    }

    public Army(int row, Card c1, Card c2)
    {
        pieces = new Int2[5];
        pieces[0] = new Int2(2, row);
        pieces[1] = new Int2(0, row);
        pieces[2] = new Int2(1, row);
        pieces[3] = new Int2(3, row);
        pieces[4] = new Int2(4, row);
        this.c1 = c1;
        this.c2 = c2;
    }

    public Army(Army oldArmy, int replaced, Int2 newPos, Card c1, Card c2)
    {
        pieces = new Int2[oldArmy.pieces.Length];
        System.Array.Copy(oldArmy.pieces, pieces, oldArmy.pieces.Length);
        pieces[replaced] = newPos;
        this.c1 = c1;
        this.c2 = c2;
    }

    public Army(Army oldArmy, int removed)
    {
        this.c1 = oldArmy.c1;
        this.c2 = oldArmy.c2;

        if (removed == 0)
        {
            pieces = new Int2[0];
            return;
        }

        pieces = new Int2[oldArmy.pieces.Length - 1];

        int j = 0;
        for (int i = 0; i < oldArmy.pieces.Length; i++)
        {
            if (i == removed) continue;
            pieces[j++] = oldArmy.pieces[i];
        }
    }

    public override string ToString()
    {
        if (pieces.Length == 0)
        {
            return "defeated";
        }

        var sb = new System.Text.StringBuilder();
        sb.Append('(');
        sb.Append(pieces.Length);
        for (int i = 0; i < pieces.Length; i++)
        {
            sb.Append(", ");
            sb.Append(pieces[i]);
        }
        sb.Append(')');
        return sb.ToString();
    }
}
