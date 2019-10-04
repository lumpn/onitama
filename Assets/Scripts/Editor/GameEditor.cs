using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Game))]
public class GameEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var game = (Game)target;
        var node = game.node;
        if (node == null) return;
        node.Expand();

        foreach (var c in node.GetChildren().OrderByDescending(c => c.utility))
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField(c.state.army1.ToString());
            EditorGUILayout.TextField(c.state.army2.ToString());
            EditorGUILayout.IntField(Game.ScorePlayer(c.state, node.state.player));
            EditorGUILayout.EndHorizontal();
        }
    }
}
