﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class PlantGenerator_NodeOnly : MonoBehaviour
{
    ////////////////////////////// Private Attributes ///////////////////////////////////

    private List<Branch> branches;
    private GameObject plant;

    private bool isPaused = true;

    /////////////////////////////////////////////////////////////////////////////////////


    ////////////////////////////// Public Attriutes ////////////////////////////////////

    public int randomSeed = 55;
    public float rotateSpeed = 0.5f;
    public int branchOrderMax = 3;
    public int growthCycles = 6;

    //public int maxSubBranchesPerNode = 3;
    //public int numRingsBetweenNodes = 8;
    //public int cyclesBetweenBranching = 6;
    //public float growthLength = 6f;
    //public float thickness = 3f;
    //public float branchShrinkness = 0.75f;
    //public float dieProbability = 0.02f;
    //public float pauseProbability = 0.1f;
    //public float tropism = 0f;
    //public bool spiraling = true;
    //public float spiralAngle = 60f;
    //public float spiralStartAngle = 0f;
    //public float wiggleFactor = 0.05f;

    public static BranchParam order1 = new BranchParam("Order 1", 1, 1, 1, 6, 6, 3f, 0.75f, 0.02f, 0.1f, 0f, true, 60f, 0f, 0.05f);
    public static BranchParam order2 = new BranchParam("Order 2", 1, 1, 1, 6, 6, 3f, 0.75f, 0.02f, 0.1f, 0f, true, 60f, 0f, 0.05f);
    public static BranchParam order3 = new BranchParam("Order 3", 1, 1, 1, 6, 6, 3f, 0.75f, 0.02f, 0.1f, 0f, true, 60f, 0f, 0.05f);
    public BranchParam[] branchParameters = { order1, order2, order3 };
    //public BranchParam[] branchParameters;

    ////////////////////////////////////////////////////////////////////////////////////

    // Start is called before the first frame update
    void Start() {
        //branchParameters = new BranchParam[branchOrderMax];
        //for (int i = 0; i < branchOrderMax; i++) {
        //    branchParameters[i] = new BranchParam("Order " + (i + 1).ToString(), 1, 1, 1, 6, 6, 3f, 0.75f, 0.02f, 0.1f, 0f, true, 60f, 0f, 0.05f);
        //}

        // Set random seed
        Random.InitState(randomSeed);
         
        // Clear branches
        branches = new List<Branch>();

        // Generate plant mesh
        plant = generatePlant();
        plant.transform.position = this.transform.position;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            isPaused = !isPaused;
        }
        if (!isPaused) {
            plant.transform.Rotate(0, rotateSpeed, 0);
        }
    }

    ///////////////////////////////// Classes //////////////////////////////////////////

    // Points where the tree will grow from
    public class Node
    {
        public Vector3 pos;
        public List<Node> subBranches;
        public Vector3 direction;
        public float die_prob;
        public float pause_prob;
        public int age;
        public int order;
        public bool isDead;
        public PlantGenerator_NodeOnly plantGenerator;

        public Node(Vector3 pos, Vector3 dir, int age, int order, PlantGenerator_NodeOnly plantGenerator) {
            this.pos = pos;
            this.subBranches = new List<Node>();
            this.direction = dir;
            this.die_prob = plantGenerator.branchParameters[order - 1].dieProbability;
            this.pause_prob = plantGenerator.branchParameters[order - 1].pauseProbability;
            this.age = age;
            this.order = order;
            this.isDead = false;
            this.plantGenerator = plantGenerator;
        }

        public Vector3 getPos() {
            return this.pos;
        }

        public List<Node> getSubBranches() {
            return this.subBranches;
        }

        public void addSubBranch(Node branch) {
            this.subBranches.Add(branch);
        }

        public Vector3 getDir() {
            return this.direction;
        }

        public int getOrder() {
            return this.order;
        }

        public float getPause() {
            return this.pause_prob;
        }

        public float getDiePob() {
            return this.die_prob;
        }

        public int getAge() {
            return this.age;
        }

        public bool getIsDead() {
            return this.isDead;
        }

        public void setDead() {
            this.isDead = true;
        }

    }

    // An entire path of nodes of a plant
    public class Branch
    {
        List<Node> nodes;
        Mesh branchMesh;

        int subBranchCount;

        PlantGenerator_NodeOnly plantGenerator;

        public Branch(Node node, PlantGenerator_NodeOnly plantGenerator) {
            this.nodes = new List<Node>();
            nodes.Add(node);
            branchMesh = new Mesh();
            subBranchCount = 0;
            this.plantGenerator = plantGenerator;
        }

        public List<Node> getNodes() {
            return this.nodes;
        }

        public Mesh getMesh() {
            return this.branchMesh;
        }

        public void setMesh(Mesh mesh) {
            this.branchMesh = mesh;
        }

        // Grows the branch by adding a new node in the direction
        // of the tipNode's tipBud
        // Returns the new tipNode
        public Node grow() {
            Node tipNode = nodes[nodes.Count - 1];
            int branchOrder = tipNode.getOrder();
            int age = tipNode.getAge();

            Vector3 oldTipPos = tipNode.getPos();
            Vector3 growthDir = tipNode.getDir();

            // Get branch's vertices and triangles of mesh
            Vector3[] oldVerts = branchMesh.vertices;
            int[] oldTris = branchMesh.triangles;

            int oldVertLength = oldVerts.Length;
            int oldTriLength = oldTris.Length;

            // Get the number of rings between nodes
            int numRings = plantGenerator.branchParameters[branchOrder - 1].numRingsBetweenNodes;

            // Create new lists of vertices and tris
            Vector3[] newVerts = new Vector3[oldVertLength + (6 * numRings) + 7];
            int[] newTris = new int[oldTriLength + (12 * numRings * 3) + (18 * 3)];

            // Check if branch is not just a cylinder base in mesh
            // If it is multiple cylinders, "cut off" the top cap of the mesh
            int stopCopyIndex = (oldVertLength > 7) ? oldTriLength - 18 - 1 : oldTriLength - 1;

            for (int v = 0; v < oldVertLength; v++) {
                newVerts[v] = oldVerts[v];
            }
            //for (int t = 0; t <= stopCopyIndex; t++) {
            //    newTris[t] = oldTris[t];
            //}
            for (int t = 0; t < oldTriLength; t++) {
                newTris[t] = oldTris[t];
            }

            // Calculate new vertices
            Vector3 T = growthDir.normalized;
            Vector3 V = (T.x == 1f) ? new Vector3(0f, 1f, 0f) : new Vector3(1f, 0f, 0f);
            Vector3 N = -1f * (Vector3.Cross(T, V)).normalized;
            Vector3 B = Vector3.Cross(T, N);

            // Get the order-specific parameters
            float growthLength = plantGenerator.branchParameters[branchOrder - 1].growthLength;
            float thickness = plantGenerator.branchParameters[branchOrder - 1].thickness;
            float branchShrinkness = plantGenerator.branchParameters[branchOrder - 1].branchShrinkness;
            float cyclesBetweenBranching = plantGenerator.branchParameters[branchOrder - 1].cyclesBetweenBranching;
            int maxSubBranchesPerNode = plantGenerator.branchParameters[branchOrder - 1].maxSubBranchesPerNode;
            bool spiraling = plantGenerator.branchParameters[branchOrder - 1].spiraling;
            float spiralAngle = plantGenerator.branchParameters[branchOrder - 1].spiralAngle;
            float spiralStartAngle = plantGenerator.branchParameters[branchOrder - 1].spiralStartAngle;
            float wiggleFactor = plantGenerator.branchParameters[branchOrder - 1].wiggleFactor;

            // Add the rings
            Vector3 newTipPos;
            int newVertsLength;

            for (int r = 1; r <= numRings; r++) {
                // Tropism
                if (branchOrder > 1) {
                    T += new Vector3(0f, plantGenerator.branchParameters[branchOrder - 1].tropism, 0);
                }

                // Add a wiggle as the branch grows
                T = T + (Random.Range(-wiggleFactor, wiggleFactor) * N) + (Random.Range(-wiggleFactor, wiggleFactor) * B);
                V = (T.x == 1f) ? new Vector3(0f, 1f, 0f) : new Vector3(1f, 0f, 0f);
                N = -1f * (Vector3.Cross(T, V)).normalized;
                B = Vector3.Cross(T, N);

                // Verts
                newTipPos = oldTipPos + (T * growthLength / ((float)numRings + 1));

                newVerts[oldVertLength] = newTipPos + (thickness * ((N * Mathf.Cos(Mathf.Deg2Rad * 0f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 0f))) / ((branchOrder + age) * branchShrinkness));
                newVerts[oldVertLength + 1] = newTipPos + (thickness * ((N * Mathf.Cos(Mathf.Deg2Rad * 60f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 60f))) / ((branchOrder + age) * branchShrinkness));
                newVerts[oldVertLength + 2] = newTipPos + (thickness * ((N * Mathf.Cos(Mathf.Deg2Rad * 120f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 120f))) / ((branchOrder + age) * branchShrinkness));
                newVerts[oldVertLength + 3] = newTipPos + (thickness * ((N * Mathf.Cos(Mathf.Deg2Rad * 180f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 180f))) / ((branchOrder + age) * branchShrinkness));
                newVerts[oldVertLength + 4] = newTipPos + (thickness * ((N * Mathf.Cos(Mathf.Deg2Rad * 240f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 240f))) / ((branchOrder + age) * branchShrinkness));
                newVerts[oldVertLength + 5] = newTipPos + (thickness * ((N * Mathf.Cos(Mathf.Deg2Rad * 300f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 300f))) / ((branchOrder + age) * branchShrinkness));

                newVertsLength = oldVertLength + 6;

                // Tris
                // Sides
                newTris[oldTriLength] = newVertsLength - 7;
                newTris[oldTriLength + 1] = newVertsLength - 12;
                newTris[oldTriLength + 2] = newVertsLength - 6;

                newTris[oldTriLength + 3] = newVertsLength - 12;
                newTris[oldTriLength + 4] = newVertsLength - 5;
                newTris[oldTriLength + 5] = newVertsLength - 6;

                newTris[oldTriLength + 6] = newVertsLength - 12;
                newTris[oldTriLength + 7] = newVertsLength - 11;
                newTris[oldTriLength + 8] = newVertsLength - 5;

                newTris[oldTriLength + 9] = newVertsLength - 11;
                newTris[oldTriLength + 10] = newVertsLength - 4;
                newTris[oldTriLength + 11] = newVertsLength - 5;

                newTris[oldTriLength + 12] = newVertsLength - 11;
                newTris[oldTriLength + 13] = newVertsLength - 10;
                newTris[oldTriLength + 14] = newVertsLength - 4;

                newTris[oldTriLength + 15] = newVertsLength - 10;
                newTris[oldTriLength + 16] = newVertsLength - 3;
                newTris[oldTriLength + 17] = newVertsLength - 4;

                newTris[oldTriLength + 18] = newVertsLength - 10;
                newTris[oldTriLength + 19] = newVertsLength - 9;
                newTris[oldTriLength + 20] = newVertsLength - 3;

                newTris[oldTriLength + 21] = newVertsLength - 9;
                newTris[oldTriLength + 22] = newVertsLength - 2;
                newTris[oldTriLength + 23] = newVertsLength - 3;

                newTris[oldTriLength + 24] = newVertsLength - 9;
                newTris[oldTriLength + 25] = newVertsLength - 8;
                newTris[oldTriLength + 26] = newVertsLength - 2;

                newTris[oldTriLength + 27] = newVertsLength - 8;
                newTris[oldTriLength + 28] = newVertsLength - 1;
                newTris[oldTriLength + 29] = newVertsLength - 2;

                newTris[oldTriLength + 30] = newVertsLength - 8;
                newTris[oldTriLength + 31] = newVertsLength - 7;
                newTris[oldTriLength + 32] = newVertsLength - 1;

                newTris[oldTriLength + 33] = newVertsLength - 7;
                newTris[oldTriLength + 34] = newVertsLength - 6;
                newTris[oldTriLength + 35] = newVertsLength - 1;

                oldTipPos = newTipPos;
                oldVertLength = newVertsLength;
                oldTriLength += 36;
            }

            // Create the new cap

            // Tropism
            if (branchOrder > 1) {
                T += new Vector3(0f, plantGenerator.branchParameters[branchOrder - 1].tropism, 0);
            }

            // Add a wiggle as the branch grows
            T = T + (Random.Range(-wiggleFactor, wiggleFactor) * N) + (Random.Range(-wiggleFactor, wiggleFactor) * B);
            V = (T.x == 1f) ? new Vector3(0f, 1f, 0f) : new Vector3(1f, 0f, 0f);
            N = -1f * (Vector3.Cross(T, V)).normalized;
            B = Vector3.Cross(T, N);

            newTipPos = oldTipPos + (T * growthLength / (numRings + 1));

            //newVerts[oldVertLength] = newTipPos;
            //newVerts[oldVertLength + 1] = newTipPos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * 0f), 0f, Mathf.Sin(Mathf.Deg2Rad * 0f));
            //newVerts[oldVertLength + 2] = newTipPos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * 60f), 0f, Mathf.Sin(Mathf.Deg2Rad * 60f));
            //newVerts[oldVertLength + 3] = newTipPos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * 120f), 0f, Mathf.Sin(Mathf.Deg2Rad * 120f));
            //newVerts[oldVertLength + 4] = newTipPos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * 180f), 0f, Mathf.Sin(Mathf.Deg2Rad * 180f));
            //newVerts[oldVertLength + 5] = newTipPos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * 240f), 0f, Mathf.Sin(Mathf.Deg2Rad * 240f));
            //newVerts[oldVertLength + 6] = newTipPos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * 300f), 0f, Mathf.Sin(Mathf.Deg2Rad * 300f));

            newVerts[oldVertLength] = newTipPos;
            newVerts[oldVertLength + 1] = newTipPos + (thickness * ((N * Mathf.Cos(Mathf.Deg2Rad * 0f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 0f))) / ((branchOrder + age) * branchShrinkness));
            newVerts[oldVertLength + 2] = newTipPos + (thickness * ((N * Mathf.Cos(Mathf.Deg2Rad * 60f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 60f))) / ((branchOrder + age) * branchShrinkness));
            newVerts[oldVertLength + 3] = newTipPos + (thickness * ((N * Mathf.Cos(Mathf.Deg2Rad * 120f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 120f))) / ((branchOrder + age) * branchShrinkness));
            newVerts[oldVertLength + 4] = newTipPos + (thickness * ((N * Mathf.Cos(Mathf.Deg2Rad * 180f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 180f))) / ((branchOrder + age) * branchShrinkness));
            newVerts[oldVertLength + 5] = newTipPos + (thickness * ((N * Mathf.Cos(Mathf.Deg2Rad * 240f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 240f))) / ((branchOrder + age) * branchShrinkness));
            newVerts[oldVertLength + 6] = newTipPos + (thickness * ((N * Mathf.Cos(Mathf.Deg2Rad * 300f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 300f))) / ((branchOrder + age) * branchShrinkness));

            newVertsLength = newVerts.Length;

            // Calculate new tris
            // Sides
            newTris[oldTriLength] = newVertsLength - 13;
            newTris[oldTriLength + 1] = newVertsLength - 12;
            newTris[oldTriLength + 2] = newVertsLength - 6;

            newTris[oldTriLength + 3] = newVertsLength - 12;
            newTris[oldTriLength + 4] = newVertsLength - 5;
            newTris[oldTriLength + 5] = newVertsLength - 6;

            newTris[oldTriLength + 6] = newVertsLength - 12;
            newTris[oldTriLength + 7] = newVertsLength - 11;
            newTris[oldTriLength + 8] = newVertsLength - 5;

            newTris[oldTriLength + 9] = newVertsLength - 11;
            newTris[oldTriLength + 10] = newVertsLength - 4;
            newTris[oldTriLength + 11] = newVertsLength - 5;

            newTris[oldTriLength + 12] = newVertsLength - 11;
            newTris[oldTriLength + 13] = newVertsLength - 10;
            newTris[oldTriLength + 14] = newVertsLength - 4;

            newTris[oldTriLength + 15] = newVertsLength - 10;
            newTris[oldTriLength + 16] = newVertsLength - 3;
            newTris[oldTriLength + 17] = newVertsLength - 4;

            newTris[oldTriLength + 18] = newVertsLength - 10;
            newTris[oldTriLength + 19] = newVertsLength - 9;
            newTris[oldTriLength + 20] = newVertsLength - 3;

            newTris[oldTriLength + 21] = newVertsLength - 9;
            newTris[oldTriLength + 22] = newVertsLength - 2;
            newTris[oldTriLength + 23] = newVertsLength - 3;

            newTris[oldTriLength + 24] = newVertsLength - 9;
            newTris[oldTriLength + 25] = newVertsLength - 8;
            newTris[oldTriLength + 26] = newVertsLength - 2;

            newTris[oldTriLength + 27] = newVertsLength - 8;
            newTris[oldTriLength + 28] = newVertsLength - 1;
            newTris[oldTriLength + 29] = newVertsLength - 2;

            newTris[oldTriLength + 30] = newVertsLength - 8;
            newTris[oldTriLength + 31] = newVertsLength - 13;
            newTris[oldTriLength + 32] = newVertsLength - 1;

            newTris[oldTriLength + 33] = newVertsLength - 13;
            newTris[oldTriLength + 34] = newVertsLength - 6;
            newTris[oldTriLength + 35] = newVertsLength - 1;

            // New cap
            newTris[oldTriLength + 36] = newVertsLength - 7;
            newTris[oldTriLength + 37] = newVertsLength - 6;
            newTris[oldTriLength + 38] = newVertsLength - 5;

            newTris[oldTriLength + 39] = newVertsLength - 7;
            newTris[oldTriLength + 40] = newVertsLength - 5;
            newTris[oldTriLength + 41] = newVertsLength - 4;

            newTris[oldTriLength + 42] = newVertsLength - 7;
            newTris[oldTriLength + 43] = newVertsLength - 4;
            newTris[oldTriLength + 44] = newVertsLength - 3;

            newTris[oldTriLength + 45] = newVertsLength - 7;
            newTris[oldTriLength + 46] = newVertsLength - 3;
            newTris[oldTriLength + 47] = newVertsLength - 2;

            newTris[oldTriLength + 48] = newVertsLength - 7;
            newTris[oldTriLength + 49] = newVertsLength - 2;
            newTris[oldTriLength + 50] = newVertsLength - 1;

            newTris[oldTriLength + 51] = newVertsLength - 7;
            newTris[oldTriLength + 52] = newVertsLength - 1;
            newTris[oldTriLength + 53] = newVertsLength - 6;

            // Make newtipNode and add it to branch's node list
            //Vector3 newTipDir = new Vector3((Random.value * 2f) - 1f, 1f + Random.value, (Random.value * 2f) - 1f);
            //Vector3 newTipDir = new Vector3(Random.value - 0.5f, 0.25f + Random.value, Random.value - 0.5f);

            // Tropism
            Vector3 newTipDir = T;
            if (branchOrder > 1) {
                newTipDir += new Vector3(0f, plantGenerator.branchParameters[branchOrder - 1].tropism, 0);
            }

            // Create a new tipNode
            Node newTipNode = new Node(newTipPos, newTipDir, age + 1, branchOrder, plantGenerator);

            // Add a number of branches at the new node if enough growth cycles have gone by
            if (branchOrder < plantGenerator.branchOrderMax) { // Make sure the the plant is not branching too deep
                if ((nodes.Count + 1) % cyclesBetweenBranching == 0) { // Make sure branch can occur at ever nth node
                    //int numBranches = Random.Range(0, maxSubBranchesPerNode + 1);  // TODO: Fix randomness here
                    int numBranches = maxSubBranchesPerNode;
                    for (int i = 0; i < numBranches; i++) {
                        Vector3 dir = new Vector3(0f, 0f, 0f);
                        if (spiraling) {  // Spiral the branches
                            //float spiralAngle = plantGenerator.spiralAngle;
                            //dir = new Vector3(Mathf.Cos(Mathf.Deg2Rad * spiralAngle * subBranchCount), Random.value / 4f, Mathf.Sin(Mathf.Deg2Rad * spiralAngle * subBranchCount));
                            float branchSpiralAngle = Mathf.Deg2Rad * spiralAngle * subBranchCount;
                            float symmetryAngle = i * Mathf.Deg2Rad * 360f / numBranches;
                            float startAngle = Mathf.Deg2Rad * spiralStartAngle;
                            dir = (N * Mathf.Cos(branchSpiralAngle + symmetryAngle + startAngle)) + (B * Mathf.Sin(branchSpiralAngle + symmetryAngle + startAngle));
                        }
                        else {
                            dir = new Vector3((Random.value * 2f) - 1f, Random.value / 4f, (Random.value * 2f) - 1f);
                        }
                        //Vector3 dir = new Vector3(1, 1, 1);
                        Vector3 subBranchPos = newTipPos - T;
                        Node subBranchNode = new Node(subBranchPos, dir, age + 1, branchOrder + 1, plantGenerator);
                        newTipNode.addSubBranch(subBranchNode);
                        if (i == numBranches - 1 && numBranches > 0) {
                            subBranchCount++;
                        }
                    }
                }
            }

            nodes.Add(newTipNode);

            // Set the new vertices and triangles for the mesh
            branchMesh.vertices = newVerts;
            branchMesh.triangles = newTris;
            branchMesh.RecalculateNormals();

            return newTipNode;
        }
    }


    ////////////////////////////////////////////////////////////////////////////////////

    ///////////////////////////////// Functions ////////////////////////////////////////

    // Begins a new branch by create a tip node and the branch object
    // Also creates the mesh for the base of the first cylinder
    public void startBranch(Vector3 pos, Vector3 dir, int age, int order) {
        // If this is the first branch of the plant, make its direction up
        Vector3 direct;
        if (branches.Count == 0) {
            direct = new Vector3(0f, 1f, 0f);
        } 
        else {
            //direct = new Vector3((Random.value * 2f) - 1f, 0.1f + Random.value, (Random.value * 2f) - 1f);
            direct = dir;
        }
        Node startNode = new Node(pos, direct, age, order, this);
        Branch newBranch = new Branch(startNode, this);

        // Create cylinder base in mesh
        Mesh startMesh = new Mesh();
        Vector3[] verts = new Vector3[7];
        int[] tris = new int[6 * 3];

        Vector3 T = dir.normalized;
        Vector3 V = (T.x == 1f) ? new Vector3(0f, 1f, 0f) : new Vector3(1f, 0f, 0f);
        Vector3 N = -1f * (Vector3.Cross(T, V)).normalized;
        Vector3 B = Vector3.Cross(T, N);

        float thickness = branchParameters[order - 1].thickness;
        float branchShrinkness = branchParameters[order - 1].branchShrinkness;

        //verts[0] = pos;
        //verts[1] = pos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * 0f), 0f, Mathf.Sin(Mathf.Deg2Rad * 0f));
        //verts[2] = pos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * 60f), 0f, Mathf.Sin(Mathf.Deg2Rad * 60f));
        //verts[3] = pos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * 120f), 0f, Mathf.Sin(Mathf.Deg2Rad * 120f));
        //verts[4] = pos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * 180f), 0f, Mathf.Sin(Mathf.Deg2Rad * 180f));
        //verts[5] = pos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * 240f), 0f, Mathf.Sin(Mathf.Deg2Rad * 240f));
        //verts[6] = pos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * 300f), 0f, Mathf.Sin(Mathf.Deg2Rad * 300f));

        verts[0] = pos;
        verts[1] = pos + (thickness * ((N * Mathf.Cos(Mathf.Deg2Rad * 0f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 0f))) / ((age + order) * branchShrinkness));
        verts[2] = pos + (thickness * ((N * Mathf.Cos(Mathf.Deg2Rad * 60f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 60f))) / ((age + order) * branchShrinkness));
        verts[3] = pos + (thickness * ((N * Mathf.Cos(Mathf.Deg2Rad * 120f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 120f))) / ((age + order) * branchShrinkness));
        verts[4] = pos + (thickness * ((N * Mathf.Cos(Mathf.Deg2Rad * 180f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 180f))) / ((age + order) * branchShrinkness));
        verts[5] = pos + (thickness * ((N * Mathf.Cos(Mathf.Deg2Rad * 240f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 240f))) / ((age + order) * branchShrinkness));
        verts[6] = pos + (thickness * ((N * Mathf.Cos(Mathf.Deg2Rad * 300f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 300f))) / ((age + order) * branchShrinkness));

        tris[0] = 0;
        tris[1] = 2;
        tris[2] = 1;

        tris[3] = 0;
        tris[4] = 3;
        tris[5] = 2;

        tris[6] = 0;
        tris[7] = 4;
        tris[8] = 3;

        tris[9] = 0;
        tris[10] = 5;
        tris[11] = 4;

        tris[12] = 0;
        tris[13] = 6;
        tris[14] = 5;

        tris[15] = 0;
        tris[16] = 1;
        tris[17] = 6;

        startMesh.vertices = verts;
        startMesh.triangles = tris;
        startMesh.RecalculateNormals();

        newBranch.setMesh(startMesh);

        // Add to global list of branches
        branches.Add(newBranch);
    }

    // Method for generating a plant object
    public GameObject generatePlant() {

        // Create the first branch
        startBranch(new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0f), 1, 1);

        // Grow branches
        for (int i = 0; i < growthCycles; i++) {

            // List of new nodes being made
            List<Node> newBranchNodes = new List<Node>();

            // Grow each branch
            foreach (Branch branch in branches) {
                List<Node> branchNodes = branch.getNodes();
                Node tipNode = branchNodes[branchNodes.Count - 1];

                int tipNodeAge = tipNode.getAge();

                //// Check if node should die -> if so then set node to die
                //// If it should not die, check if its already dead or should pause growth
                //float deathValue = Random.value;
                //if (deathValue > tipNode.getDiePob() + (0.001f * tipNodeAge)) {
                //    //string message = deathValue.ToString() + ", " + (tipBud.getDiePob() + (0.015f * tipBudAge)).ToString();
                //    //Debug.Log(message);
                //    tipNode.setDead();
                //}
                //if (!tipNode.getIsDead() && Random.value > tipNode.getPause()) {
                //    Node newNode = branch.grow();
                //    foreach(Node subBranchNode in newNode.getSubBranches()) {
                //        newBranchNodes.Add(subBranchNode);
                //    }
                //}

                //////////// TODO: Fix randomness here /////////////////////

                Node newNode = branch.grow();
                foreach (Node subBranchNode in newNode.getSubBranches()) {
                    newBranchNodes.Add(subBranchNode);
                }
            }

            //print(newNodes[0].getTipBud().getOrder());

            // Start new branches at new nodes' side buds
            foreach (Node subBranchStart in newBranchNodes) {
                startBranch(subBranchStart.getPos(), subBranchStart.getDir(), subBranchStart.getAge(), subBranchStart.getOrder());
            }
        }

        GameObject plant = new GameObject();
        plant.name = this.name + "_Generated";

        // Create the branch objects
        foreach (Branch branch in branches) {
            GameObject branchObj = new GameObject();
            branchObj.name = "Branch";
            branchObj.AddComponent<MeshFilter>();
            branchObj.AddComponent<MeshRenderer>();
            branchObj.GetComponent<MeshFilter>().mesh = branch.getMesh();
            Renderer rend = branchObj.GetComponent<Renderer>();
            rend.material.color = new Color(0.5f, 0.2f, 0.05f, 1.0f);  // bark color
            branchObj.transform.parent = plant.transform;

            //// Create leaf spheres at the end of each segment
            //List<Node> segNodes = seg.getNodes();
            //Bud segTipBud = segNodes[segNodes.Count - 1].getTipBud();
            //if (segTipBud.getOrder() > 3) {
            //    GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //    leaf.transform.position = segTipBud.getPos();
            //    //leaf.transform.localScale = new Vector3(5f / segTipBud.getOrder(), 5f / segTipBud.getOrder(), 5f / segTipBud.getOrder());
            //    leaf.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);

            //    // Set leaf color
            //    Color color = new Color(1f, 1f, 1f);
            //    if (leafColor == "Green") {
            //        color = new Color(0f, Random.value, 0f, 1f);                                     // light green
            //    }
            //    else if (leafColor == "Autumn") {
            //        color = new Color(0.3f + Random.value, Random.value - 0.5f, 0f, 1f);     // autumn colors
            //    }
            //    else if (leafColor == "Sakura") {
            //        float pinkRandom = Random.Range(0.5f, 0.8f);
            //        color = new Color(1f, pinkRandom, pinkRandom, 1f);                                   // pink
            //    }
            //    leaf.GetComponent<Renderer>().material.color = color;
            //    leaf.transform.parent = plant.transform;

            //}
        }

        return plant;
    }

    ////////////////////////////////////////////////////////////////////////////////////

}



[System.Serializable]
public class BranchParam
{
    public string orderName;
    public int order;
    public int maxSubBranchesPerNode = 3;
    public int cyclesBetweenBranching = 6;
    public float growthLength = 6f;
    public int numRingsBetweenNodes = 8;
    public float thickness = 3f;
    public float branchShrinkness = 0.75f;
    public float dieProbability = 0.02f;
    public float pauseProbability = 0.1f;
    public float tropism = 0f;
    public bool spiraling = true;
    public float spiralAngle = 60f;
    public float spiralStartAngle = 0f;
    public float wiggleFactor = 0.05f;

    public BranchParam(string name, int ord, int maxBranch, int betweenBranch, float growL, int nRings, float thick, float shrink, float die, float pause, float trop, bool spiral, float sAngle, float sStartAngle, float wiggle) {
        orderName = name;
        order = ord;
        maxSubBranchesPerNode = maxBranch;
        cyclesBetweenBranching = betweenBranch;
        growthLength = growL;
        numRingsBetweenNodes = nRings;
        thickness = thick;
        branchShrinkness = shrink;
        dieProbability = die;
        pauseProbability = pause;
        tropism = trop;
        spiraling = spiral;
        spiralAngle = sAngle;
        spiralStartAngle = sStartAngle;
        wiggleFactor = wiggle;

}

    public BranchParam() {
    }
}
