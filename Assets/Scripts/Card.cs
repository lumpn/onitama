public sealed class Card
{
    private readonly Int2[] moves;

    public Card(params Int2[] a)
    {
        moves = new Int2[a.Length];
        System.Array.Copy(a, moves, a.Length);
    }

    public Int2 GetMove(int i, int player)
    {
        return (player == 1) ? moves[i] : -moves[i];
    }

    public int NumMoves()
    {
        return moves.Length;
    }
}
