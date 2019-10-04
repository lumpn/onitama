using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Game : MonoBehaviour
{
    public enum Strategy
    {
        Random,
        Utility,
        MCTS,
    }

    private const int scoreUnit = 100;

    private const int scoreTile = 1;
    private const int scoreProtect = 1;
    private const int scoreThreat = 10;
    private const int scoreKill = 90;
    private const int scoreCheck = 20;
    private const int scoreCheckmate = 10000;

    private static readonly Vector3 cubeSize = new Vector3(0.8f, 0.8f, 0.8f);
    private static readonly Vector3 cardScale = new Vector3(0.5f, 0.5f, 0.5f);

    [SerializeField] private Strategy strategy1;
    [SerializeField] private Strategy strategy2;
    [SerializeField] private bool autoPlay;
    [SerializeField] private int numIterations;
    [System.NonSerialized] public Node node;

    IEnumerator Start()
    {
        var cards = CreateCards();

        for (int i = 1; i < cards.Count; i++)
        {
            int j = Random.Range(i, cards.Count);
            Card a = cards[i - 1];
            cards[i - 1] = cards[j];
            cards[j] = a;
        }

        var a1 = new Army(0, cards[0], cards[1]);
        var a2 = new Army(4, cards[2], cards[3]);
        var initalState = new BoardState(a1, a2, cards[4], 1);

        var root = new Node(null, initalState);

        node = root;
        using (var csv = File.CreateText("mcts.csv"))
        {
            csv.Write("id; state; a1; a2; u1; u2; wins; losses; visits; ratio; next");
            for (int i = 0; i < 20; i++)
            {
                csv.Write("; q{0:00}; n{0:00}", i);
            }
            csv.WriteLine();

            while (true)
            {
                Node nextNode = null;
                var strategy = (node.state.player == 1) ? strategy1 : strategy2;
                switch (strategy)
                {
                    case Strategy.Random:
                        nextNode = MCTS.RandomSearch(node);
                        break;
                    case Strategy.Utility:
                        nextNode = MCTS.UtilitySearch(node);
                        break;
                    case Strategy.MCTS:
                        nextNode = MCTS.MonteCarloTreeSearch(node, numIterations);
                        break;
                }
                PrintCSV(node, nextNode, csv);
                if (nextNode == null) break;

                yield return null;
                while (!autoPlay && !Input.anyKeyDown) yield return null;
                node = nextNode;
            }
        }

        using (var writer = File.CreateText("mcts.dot"))
        {
            writer.WriteLine("digraph mcts {");
            writer.WriteLine("node [style=filled];");
            //PrintAll(0, root, writer);
            PrintNode(0, root, writer);
            PrintMoves(node, writer);
            writer.WriteLine("}");

        }
        Debug.Log((node.state.player == 1) ? "Blue wins" : "Red wins");
    }

    private static void PrintCSV(Node node, Node nextNode, StreamWriter writer)
    {
        writer.Write(node.id);
        writer.Write("; ");
        writer.Write(node.state);
        writer.Write("; ");
        writer.Write(node.state.army1.Size);
        writer.Write("; ");
        writer.Write(node.state.army2.Size);
        writer.Write("; ");
        writer.Write(Game.ScoreOnlyPlayer(node.state, 1));
        writer.Write("; ");
        writer.Write(Game.ScoreOnlyPlayer(node.state, 2));
        writer.Write("; ");
        writer.Write(node.numWon);
        writer.Write("; ");
        writer.Write(node.numLost);
        writer.Write("; ");
        writer.Write(node.numVisits);
        writer.Write("; ");
        writer.Write(node.GetWinRatio(node.state.player));
        writer.Write("; ");
        writer.Write((nextNode != null) ? nextNode.id : 0);
        foreach (var c in node.GetChildren().OrderByDescending(c => c.numVisits))
        {
            writer.Write("; ");
            writer.Write(c.numLost);
            writer.Write("; ");
            writer.Write(c.numVisits);
        }
        writer.WriteLine();
    }

    private static void PrintMoves(Node node, StreamWriter writer)
    {
        if (node == null) return;
        writer.WriteLine("n{0} [shape={1}];", node.id, (node.state.player == 1) ? "house" : "invhouse");
        foreach (var c in node.GetChildren())
        {
            PrintNode(node.id, c, writer);
        }
        PrintMoves(node.parent, writer);
    }

    private static void PrintNode(int parentId, Node node, StreamWriter writer)
    {
        var player = node.state.player;
        var isWinning = node.numWon > node.numLost;
        var winningPlayer = isWinning ? player : Game.GetEnemy(player);
        var hue = (winningPlayer == 1) ? 0f : 0.66f;
        var saturation = (node.numVisits > 0) ? Mathf.Clamp01(node.GetWinRatio(winningPlayer) - 0.5f) : 0f;

        writer.Write("n");
        writer.Write(node.id);
        writer.Write(" [label=\"");
        writer.Write(node.numWon);
        writer.Write("\\n");
        writer.Write(node.numVisits);
        writer.Write("\", fillcolor=\"");
        writer.Write(hue);
        writer.Write(" ");
        writer.Write(saturation);
        writer.Write(" 1.0\"];");
        writer.WriteLine();

        writer.Write("n");
        writer.Write(parentId);
        writer.Write(" -> n");
        writer.Write(node.id);
        writer.Write(";");
        writer.WriteLine();
    }

    private static void PrintAll(int parentId, Node node, StreamWriter writer)
    {
        PrintNode(parentId, node, writer);
        foreach (var c in node.GetChildren())
        {
            PrintAll(node.id, c, writer);
        }
    }

    private static readonly Color[] arrowColors = { Color.green, Color.yellow, Color.magenta };

    void OnDrawGizmos()
    {
        if (node == null) return;
        var state = node.state;
        if (state == null) return;
        Gizmos.matrix = Matrix4x4.identity;
        Draw(state.army1, Color.red, new Vector3(2, 0, -2), Quaternion.identity);
        Draw(state.army2, Color.blue, new Vector3(2, 0, 6), Quaternion.Euler(0, 180, 0));

        var p = (state.player == 1) ? 6 : -2;
        var r = (state.player == 1) ? Quaternion.identity : Quaternion.Euler(0, 180, 0);
        Gizmos.matrix = Matrix4x4.TRS(new Vector3(p, 0, 2), r, cardScale);
        DrawCard(state.card);

        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = Color.green;

        var player = node.state.player;
        var oldArmy = Game.GetArmy(node.state, player);
        System.Func<Node, int> evaluate = null;
        var strategy = (node.state.player == 1) ? strategy1 : strategy2;
        switch (strategy)
        {
            case Strategy.Random:
                evaluate = (c => 0);
                break;
            case Strategy.Utility:
                evaluate = (c => c.utility);
                break;
            case Strategy.MCTS:
                evaluate = (c => c.numVisits);
                break;
        }
        foreach (var c in node.GetChildren().OrderByDescending(evaluate).Take(4))
        {
            var newArmy = Game.GetArmy(c.state, player);
            for (int i = 0; i < oldArmy.Size; i++)
            {
                var oldPos = oldArmy.GetPiece(i);
                var newPos = newArmy.GetPiece(i);
                if (oldPos != newPos)
                {
                    DrawArrow(ToWorldSpace(oldPos), ToWorldSpace(newPos));
                    Gizmos.color = Color.yellow;
                    break;
                }
            }
        }
    }

    private static void DrawArrow(Vector3 a, Vector3 b)
    {
        Gizmos.DrawLine(a, b);
        var ab = b - a;
        var normal = Vector3.Cross(ab, Vector3.up);
        Gizmos.DrawLine(b, b + normal / 4 - ab / 4);
        Gizmos.DrawLine(b, b - normal / 4 - ab / 4);
    }

    private static Vector3 ToWorldSpace(Int2 pos)
    {
        return new Vector3(pos.x, 0, pos.y);
    }


    private static void Draw(Army army, Color color, Vector3 offset, Quaternion rotation)
    {
        Gizmos.matrix = Matrix4x4.TRS(offset + new Vector3(-2, 0, 0), rotation, cardScale);
        DrawCard(army.c1);
        Gizmos.matrix = Matrix4x4.TRS(offset + new Vector3(2, 0, 0), rotation, cardScale);
        DrawCard(army.c2);
        Gizmos.matrix = Matrix4x4.identity;

        if (army.Size < 1) return;
        Gizmos.color = color;
        var master = army.GetPiece(0);
        Gizmos.DrawSphere(ToWorldSpace(master), 0.4f);

        for (int i = 1; i < army.Size; i++)
        {
            var piece = army.GetPiece(i);
            Gizmos.DrawCube(ToWorldSpace(piece), cubeSize);
        }
    }

    public static BoardState GetRandomMove(BoardState state)
    {
        var nextStates = new List<BoardState>();
        Next(state, nextStates);

        if (nextStates.Count == 0) return null;

        int id = Random.Range(0, nextStates.Count);
        return nextStates[id];
    }

    public static BoardState GetUtilityMove(BoardState state)
    {
        var nextStates = new List<BoardState>();
        Next(state, nextStates);

        if (nextStates.Count == 0) return null;
        return nextStates.ArgMaxTie(p => ScorePlayer(p, state.player));
    }

    public static BoardState ParallelGetBestMove(BoardState state, int numRecursions, out int score)
    {
        var nextStates = new List<BoardState>();
        Next(state, nextStates);

        var pulse = new object();
        var threadedStates = new ThreadedState[nextStates.Count];
        for (int i = 0; i < nextStates.Count; i++)
        {
            var nextState = nextStates[i];
            var threadedState = new ThreadedState(nextState, numRecursions - 1, pulse);
            System.Threading.ThreadPool.QueueUserWorkItem(GetBestMoveThreaded, threadedState);
            threadedStates[i] = threadedState;
        }

        BoardState bestMove = null;
        var bestScore = -999999;
        for (int i = 0; i < threadedStates.Length; i++)
        {
            var t = threadedStates[i];
            lock (pulse)
            {
                while (!t.done)
                {
                    System.Threading.Monitor.Wait(pulse);
                }
            }

            var enemyMove = t.bestMove;
            var enemyMoveScore = t.bestScore;
            var moveScore = -enemyMoveScore;
            if (moveScore >= bestScore)
            {
                bestScore = moveScore;
                bestMove = t.boardState;
            }
        }

        score = bestScore;
        return bestMove;
    }

    public sealed class ThreadedState
    {
        public readonly BoardState boardState;
        public readonly int numRecursions;
        public readonly object pulse;
        public BoardState bestMove;
        public int bestScore;
        public bool done;

        public ThreadedState(BoardState boardState, int numRecursions, object pulse)
        {
            this.boardState = boardState;
            this.numRecursions = numRecursions;
            this.pulse = pulse;
        }
    }

    public static void GetBestMoveThreaded(object obj)
    {
        var state = (ThreadedState)obj;
        int score;
        var move = GetBestMove(state.boardState, state.numRecursions, out score);
        state.bestMove = move;
        state.bestScore = score;
        state.done = true;
        var pulse = state.pulse;
        lock (pulse)
        {
            System.Threading.Monitor.Pulse(pulse);
        }
    }

    public static BoardState GetBestMove(BoardState state, int numRecursions, out int score)
    {
        if (numRecursions <= 0) return GetBestMove(state, out score);

        var nextStates = new List<BoardState>();
        Next(state, nextStates);

        BoardState bestMove = null;
        var bestScore = -999999;
        for (int i = 0; i < nextStates.Count; i++)
        {
            var nextState = nextStates[i];

            int enemyMoveScore;
            var enemyMove = GetBestMove(nextState, numRecursions - 1, out enemyMoveScore);
            var moveScore = -enemyMoveScore;
            if (moveScore >= bestScore)
            {
                bestScore = moveScore;
                bestMove = nextState;
            }
        }

        score = bestScore;
        return bestMove;
    }

    public static BoardState GetBestMove(BoardState state, out int score)
    {
        var nextStates = new List<BoardState>();
        Next(state, nextStates);

        BoardState bestMove = null;
        int bestScore = -999991;
        foreach (var nextState in nextStates)
        {
            var moveScore = ScorePlayer(nextState, state.player);
            if (moveScore > bestScore)
            {
                bestScore = moveScore;
                bestMove = nextState;
            }
        }

        score = bestScore;
        return bestMove;
    }

    public static int GetWinner(BoardState state)
    {
        return (state.army1.Size > state.army2.Size) ? 1 : 2;
    }

    public static int ScorePlayer(BoardState state, int player)
    {
        return ScoreOnlyPlayer(state, player) - ScoreOnlyPlayer(state, GetEnemy(player));
    }

    public static int ScoreOnlyPlayer(BoardState state, int player)
    {
        var activePlayer = (state.player == player);
        var army = GetArmy(state, player);
        var enemyArmy = GetArmy(state, GetEnemy(player));

        int score = army.Size * scoreUnit;

        var winPos = (player == 1) ? new Int2(2, 4) : new Int2(2, 0);
        for (int i = 0; i < army.Size; i++)
        {
            for (int j = 0; j < army.c1.NumMoves(); j++) score += ScoreMove(army.c1.GetMove(j, player), i, winPos, army, enemyArmy, activePlayer);
            for (int j = 0; j < army.c2.NumMoves(); j++) score += ScoreMove(army.c2.GetMove(j, player), i, winPos, army, enemyArmy, activePlayer);
        }

        return score;
    }

    public static void DrawCard(Card c)
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawCube(Vector3.zero, cubeSize);

        Gizmos.color = Color.white;
        for (int i = 0; i < c.NumMoves(); i++)
        {
            var m = c.GetMove(i, 1);
            Gizmos.DrawCube(new Vector3(m.x, 0, m.y), cubeSize);
        }
    }

    public static List<Card> CreateCards()
    {
        var l = new Int2(-1, 0);
        var lu = new Int2(-1, 1);
        var ld = new Int2(-1, -1);
        var ll = new Int2(-2, 0);
        var llu = new Int2(-2, 1);
        var r = new Int2(1, 0);
        var ru = new Int2(1, 1);
        var rd = new Int2(1, -1);
        var rr = new Int2(2, 0);
        var rru = new Int2(2, 1);
        var d = new Int2(0, -1);
        var u = new Int2(0, 1);
        var uu = new Int2(0, 2);
        var rooster = new Card(l, ld, r, ru);
        var mantis = new Card(lu, d, ru);
        var boar = new Card(l, u, r);
        var crane = new Card(ld, u, rd);
        var horse = new Card(l, u, d);
        var crab = new Card(ll, u, rr);
        var frog = new Card(ll, lu, rd);
        var rabbit = new Card(ld, ru, rr);
        var ox = new Card(u, r, d);
        var dragon = new Card(llu, ld, rd, rru);
        var eel = new Card(lu, ld, r);
        var monkey = new Card(lu, ld, ru, rd);
        var cobra = new Card(l, ru, rd);
        var elephant = new Card(l, lu, r, ru);
        var tiger = new Card(uu, d);
        var goose = new Card(l, lu, r, rd);

        var cards = new List<Card>(new[] { rooster, mantis, boar, crane, horse, crab, frog, rabbit, ox, dragon, eel, monkey, cobra, elephant, tiger, goose });
        return cards;
    }

    public static void Next(BoardState state, List<BoardState> result)
    {
        var army = GetArmy(state, state.player);

        for (int i = 0; i < army.Size; i++)
        {
            for (int j = 0; j < army.c1.NumMoves(); j++) ApplyMove(army.c1.GetMove(j, state.player), i, army, state, state.player, army.c1, army.c2, result);
            for (int j = 0; j < army.c2.NumMoves(); j++) ApplyMove(army.c2.GetMove(j, state.player), i, army, state, state.player, army.c2, army.c1, result);
        }
    }

    public static int ScoreMove(Int2 move, int id, Int2 winPos, Army army, Army enemyArmy, bool activePlayer)
    {
        var piece = army.GetPiece(id);
        var newPos = piece + move;
        if (IsOutOfBounds(newPos)) return 0;

        if (id == 0 && newPos == winPos)
        {
            return activePlayer ? scoreCheckmate : scoreCheck;
        }

        for (int i = 0; i < army.Size; i++)
        {
            if (army.GetPiece(i) == newPos)
            {
                return (i == 0) ? 0 : scoreProtect;
            }
        }

        for (int i = 0; i < enemyArmy.Size; i++)
        {
            if (enemyArmy.GetPiece(i) == newPos)
            {
                if (i == 0)
                {
                    return activePlayer ? scoreCheckmate : scoreCheck;
                }
                return activePlayer ? scoreKill : scoreThreat;
            }
        }

        return scoreTile;
    }

    public static void ApplyMove(Int2 move, int id, Army army, BoardState state, int player, Card moveCard, Card otherCard, List<BoardState> result)
    {
        var piece = army.GetPiece(id);
        var newPos = piece + move;
        if (IsOutOfBounds(newPos)) return;
        for (int i = 0; i < army.Size; i++)
        {
            if (army.GetPiece(i) == newPos) return;
        }

        var newArmy = new Army(army, id, newPos, otherCard, state.card);

        var enemy = GetEnemy(player);
        var enemyArmy = GetArmy(state, enemy);

        if (id == 0)
        {
            var winPos = (player == 1) ? new Int2(2, 4) : new Int2(2, 0);
            if (newPos == winPos)
            {
                var newEnemy = new Army(enemyArmy, 0);
                AddState(newArmy, newEnemy, moveCard, player, result);
                return;
            }
        }

        for (int i = 0; i < enemyArmy.Size; i++)
        {
            if (enemyArmy.GetPiece(i) == newPos)
            {
                var newEnemy = new Army(enemyArmy, i);
                AddState(newArmy, newEnemy, moveCard, player, result);
                return;
            }
        }

        AddState(newArmy, enemyArmy, moveCard, player, result);
    }

    public static void AddState(Army playerArmy, Army enemyArmy, Card moveCard, int player, List<BoardState> result)
    {
        var a1 = (player == 1) ? playerArmy : enemyArmy;
        var a2 = (player == 1) ? enemyArmy : playerArmy;
        var state = new BoardState(a1, a2, moveCard, GetEnemy(player));
        result.Add(state);
    }

    public static bool IsOutOfBounds(Int2 p)
    {
        return (p.x < 0 || p.x > 4 || p.y < 0 || p.y > 4);
    }

    public static int GetEnemy(int player)
    {
        return (player == 1) ? 2 : 1;
    }

    public static Army GetArmy(BoardState state, int player)
    {
        return (player == 1) ? state.army1 : state.army2;
    }
}
