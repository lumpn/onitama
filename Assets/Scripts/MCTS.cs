using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class Node
{
    private static readonly Node[] emptyChildren = new Node[0];
    private static int serial = 0;

    public readonly int id;
    public readonly Node parent;
    public readonly BoardState state;
    private int q, n;

    public int numVisits
    {
        get{ return n; }
    }

    public int numWon
    {
        get{ return q; }
    }

    public int numLost
    {
        get{ return n - q; }
    }

    private Node[] children;

    public Node[] GetChildren()
    {
        return children ?? emptyChildren;
    }

    public void Expand()
    {
        if (children != null) return;

        var nextStates = new List<BoardState>();
        Game.Next(state, nextStates);
        var len = nextStates.Count;
        children = new Node[len];
        for (int i = 0; i < len; i++)
        {
            children[i] = new Node(this, nextStates[i]);
        }
    }

    public int Rollout(bool useRandom)
    {
        var currentState = state;
        while (true)
        {
            var nextState = useRandom ? Game.GetRandomMove(currentState) : Game.GetUtilityMove(currentState);
            if (nextState == null)
            {
                return Game.GetWinner(currentState);
            }

            currentState = nextState;
        }
    }

    public bool expanded
    {
        get{ return children != null; }
    }

    public bool hasChildren
    {
        get { return children != null && children.Length > 0; }
    }

    public bool fullyVisited
    {
        get
        {
            foreach (var c in children)
            {
                if (!c.visited) return false;
            }
            return true;
        }
    }

    public bool visited
    {
        get{ return n > 0; }
    }

    public bool isRoot
    {
        get { return parent == null; }
    }


    // function for selecting the best child
    // node with highest number of visits
    public Node mostVistedChild
    {
        get
        {
            return children.ArgMax(c => c.n);
        }
    }

    public Node bestChildUTC
    {
        get
        {
            return children.ArgMax(c => UCT(c.GetWinRatio(state.player), c.n, n));
        }
    }

    public Node bestChildUtility
    {
        get
        { 
            return children.ArgMaxTie(c => Game.ScorePlayer(c.state, state.player));
        }
    }

    public static float UCT(int wins, int visits, int parentVisits)
    {
        return UCT((float)wins / visits, visits, parentVisits);
    }

    public static float UCT(float winRatio, int visits, int parentVisits)
    {
        return winRatio + 1.4f * Mathf.Sqrt(Mathf.Log(parentVisits) / visits);
    }

    public float GetWinRatio(int player)
    {
        int wins = (player == state.player) ? q : (n - q);
        return (float)wins / n;
    }

    public Node randomChild
    {
        get
        {
            var i = Random.Range(0, children.Length);
            return children[i];
        }
    }

    public Node firstUnvisitedChild
    {
        get
        {
            return children.FirstOrDefault(p => !p.visited);
        }
    }

    public void UpdateStats(bool won)
    {
        if (won) q++;
        n++;
    }

    public Node(Node parent, BoardState state)
    {
        this.id = serial++;
        this.parent = parent;
        this.state = state;
    }

    public int utility
    {
        get { return Game.ScorePlayer(state, Game.GetEnemy(state.player)); }
    }

    public override string ToString()
    {
        return string.Format("(q {0}, n {1}, state {2}, utility {3})", q, n, state, utility);
    }
}

public static class MCTS
{
    public static Node UtilitySearch(Node root)
    {
        root.Expand();
        if (!root.hasChildren) return null;
        return root.bestChildUtility;
    }

    public static Node RandomSearch(Node root)
    {
        root.Expand();
        if (!root.hasChildren) return null;
        return root.randomChild;
    }

    // main function for the Monte Carlo Tree Search
    public static Node MonteCarloTreeSearch(Node root, int numIterations)
    {
        var player = root.state.player;
        for (int i = 0; i < numIterations; i++)
        {
            var leaf = Traverse(root);
            var winner = leaf.Rollout((leaf.numVisits % 2) == 0);
            Backpropagate(root, leaf, winner);
        }

        return root.mostVistedChild;
    }

    //function for node traversal
    private static Node Traverse(Node node)
    {
        while (node.expanded && node.hasChildren && node.fullyVisited)
        {
            node = node.bestChildUTC;
        }

        node.Expand();

        // in case no children are present / node is terminal 
        return node.firstUnvisitedChild ?? node;
    }

    // function for backpropagation
    private static void Backpropagate(Node root, Node node, int winner)
    {
        if (node == null) return;
        node.UpdateStats(node.state.player == winner);

        if (node == root) return;
        Backpropagate(root, node.parent, winner);
    }
}
