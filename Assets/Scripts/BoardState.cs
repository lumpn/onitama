public sealed class BoardState
{
    public readonly Army army1, army2;
    public readonly Card card;
    public readonly int player;

    public BoardState(Army a1, Army a2, Card c, int player)
    {
        this.army1 = a1;
        this.army2 = a2;
        this.card = c;
        this.player = player;
    }

    public override string ToString()
    {
        return string.Format("{0} -- {1}", army1, army2);
    }
}
