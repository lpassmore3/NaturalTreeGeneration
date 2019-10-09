using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(PlantGenerator_NodeOnly))]
public class BranchParametersEditor : Editor
{
    PlantGenerator_NodeOnly t;
    SerializedObject GetTarget;
    SerializedProperty BranchParameterList;
    int ListSize;

    private static GUIContent
        addOrderButton = new GUIContent("+", "Add order"),
        removeOrderButton = new GUIContent("-", "Remove order");

    void OnEnable() {
        t = (PlantGenerator_NodeOnly)target;
        GetTarget = new SerializedObject(t);
        BranchParameterList = GetTarget.FindProperty("branchParameters"); // Find the List in our script and create a refrence of it
    }

    public override void OnInspectorGUI() {
        GetTarget.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("transparentCubePrefab"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("randomSeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rotateSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("branchOrderMax"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("growthCycles"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("clusterGridNumCells"));
        EditorGUILayout.IntField("Order Depth", BranchParameterList.arraySize);
        EditorGUILayout.PropertyField(BranchParameterList);

        EditorGUI.indentLevel += 1;
        for (int i = 0; i < BranchParameterList.arraySize; i++) {
            SerializedProperty branchParamObject = BranchParameterList.GetArrayElementAtIndex(i);
            SerializedProperty orderName = branchParamObject.FindPropertyRelative("orderName");
            SerializedProperty order = branchParamObject.FindPropertyRelative("order");
            SerializedProperty maxSubBranchesPerNode = branchParamObject.FindPropertyRelative("maxSubBranchesPerNode");
            SerializedProperty cyclesBetweenBranching = branchParamObject.FindPropertyRelative("cyclesBetweenBranching");
            SerializedProperty growthLength = branchParamObject.FindPropertyRelative("growthLength");
            SerializedProperty numRingsBetweenNodes = branchParamObject.FindPropertyRelative("numRingsBetweenNodes");
            SerializedProperty thickness = branchParamObject.FindPropertyRelative("thickness");
            SerializedProperty branchShrinkness = branchParamObject.FindPropertyRelative("branchShrinkness");
            SerializedProperty dieProbability = branchParamObject.FindPropertyRelative("dieProbability");
            SerializedProperty pauseProbability = branchParamObject.FindPropertyRelative("pauseProbability");
            SerializedProperty tropism = branchParamObject.FindPropertyRelative("tropism");
            SerializedProperty spiraling = branchParamObject.FindPropertyRelative("spiraling");
            SerializedProperty spiralAngle = branchParamObject.FindPropertyRelative("spiralAngle");
            SerializedProperty spiralStartAngle = branchParamObject.FindPropertyRelative("spiralStartAngle");
            SerializedProperty wiggleFactor = branchParamObject.FindPropertyRelative("wiggleFactor");
            SerializedProperty densityThreshold = branchParamObject.FindPropertyRelative("densityThreshold");

            EditorGUILayout.LabelField("Order " + (i + 1).ToString());
            EditorGUI.indentLevel += 1;
            EditorGUILayout.PropertyField(order);
            t.branchParameters[i].maxSubBranchesPerNode = EditorGUILayout.IntField("Max Sub Branches Per Node", maxSubBranchesPerNode.intValue);
            t.branchParameters[i].cyclesBetweenBranching = EditorGUILayout.IntField("Cycles Between Branching", cyclesBetweenBranching.intValue);
            t.branchParameters[i].growthLength = EditorGUILayout.FloatField("Growth Length", growthLength.floatValue);
            t.branchParameters[i].numRingsBetweenNodes = EditorGUILayout.IntField("Num Rings Between Nodes", numRingsBetweenNodes.intValue);
            t.branchParameters[i].thickness = EditorGUILayout.FloatField("Thickness", thickness.floatValue);
            t.branchParameters[i].branchShrinkness = EditorGUILayout.FloatField("Branch Shrinkness", branchShrinkness.floatValue);
            t.branchParameters[i].dieProbability = EditorGUILayout.FloatField("Die Probability", dieProbability.floatValue);
            t.branchParameters[i].pauseProbability = EditorGUILayout.FloatField("Pause Probability", pauseProbability.floatValue);
            t.branchParameters[i].tropism = EditorGUILayout.FloatField("Tropism", tropism.floatValue);
            t.branchParameters[i].spiraling = EditorGUILayout.Toggle("Spiraling", spiraling.boolValue);
            t.branchParameters[i].spiralAngle = EditorGUILayout.FloatField("Spiral Angle", spiralAngle.floatValue);
            t.branchParameters[i].spiralStartAngle = EditorGUILayout.FloatField("Spiral Start Angle", spiralStartAngle.floatValue);
            t.branchParameters[i].wiggleFactor = EditorGUILayout.FloatField("Wiggle Factor", wiggleFactor.floatValue);
            t.branchParameters[i].densityThreshold = EditorGUILayout.FloatField("Density Threshold", densityThreshold.floatValue);

            EditorGUI.indentLevel -= 1;
        }

        if (GUILayout.Button(addOrderButton)) {
            t.AddNewOrder();
            ListSize = t.GetOrderCount();
        }
        if (GUILayout.Button(removeOrderButton)) {
            t.RemoveOrder(BranchParameterList.arraySize - 1);
            ListSize = t.GetOrderCount();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
