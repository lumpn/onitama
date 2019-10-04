using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class TestMCTS
{
    [Test]
    public void TestUCT()
    {
        Assert.Greater(Node.UCT(1, 2, 2), Node.UCT(0, 2, 2)); // winning is better
        Assert.Greater(Node.UCT(2, 2, 2), Node.UCT(1, 2, 2)); // winning is better

        Assert.Greater(Node.UCT(0.5f, 2, 10), Node.UCT(0.5f, 4, 10)); // fewer visits is better
        Assert.Greater(Node.UCT(0.5f, 4, 10), Node.UCT(0.5f, 8, 10)); // fewer visits is better

        Assert.Greater(Node.UCT(0f, 2, 10), Node.UCT(0f, 4, 10)); // fewer visits is better
        Assert.Greater(Node.UCT(0f, 4, 10), Node.UCT(0f, 8, 10)); // fewer visits is better

        Assert.Greater(Node.UCT(1f, 1, 10), Node.UCT(1f, 2, 10)); // fewer visits is better
        Assert.Greater(Node.UCT(1f, 2, 10), Node.UCT(1f, 4, 10)); // fewer visits is better
        Assert.Greater(Node.UCT(1f, 4, 10), Node.UCT(1f, 8, 10)); // fewer visits is better

        Debug.Log(Node.UCT(4032, 9327, 12368));
        Debug.Log(Node.UCT(155, 231, 12368));
        Debug.Log(Node.UCT(152, 225, 12368));
        Debug.Log(Node.UCT(151, 223, 12368));
        Debug.Log(Node.UCT(137, 197, 12368));
    }

    [Test]
    public void TestArgMax()
    {
        int[] items = { 4, 2, 7, 5, 3, 8, 6, 9, 1 };
        Assert.AreEqual(9, items.ArgMax(p => p));
    }
}
