using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class PlantGenerator : MonoBehaviour
{
    ////////////////////////////// Private Attributes ///////////////////////////////////
    
    private List<Segment> segments;
    private GameObject plant;

    private bool isPaused = true;

    /////////////////////////////////////////////////////////////////////////////////////


    ////////////////////////////// Public Attriutes ////////////////////////////////////

    public int randomSeed = 55;
    public float rotateSpeed = 0.5f;
    public int maxSideBudsPerNode = 3;
    public int growthCycles = 6;
    public float growthLength = 6f;
    public float dieProbability = 0.02f;
    public float pauseProbability = 0.1f;
    public float tropism = 0f;

    ////////////////////////////////////////////////////////////////////////////////////

    // Start is called before the first frame update
    void Start()
    {
        // Set random seed
        Random.InitState(randomSeed);

        // Clear segments
        segments = new List<Segment>();

        // Generate plant mesh
        plant = generatePlant();
        plant.transform.position = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            isPaused = !isPaused;
        }
        if (!isPaused) {
            plant.transform.Rotate(0, rotateSpeed, 0);
        }
    }

    ///////////////////////////////// Classes //////////////////////////////////////////

    // Points where the tree will grow from
    public class Bud
    {
        public Vector3 pos;
        public Vector3 direction;
        public float die_prob;
        public float pause_prob;
        public int age;
        public int order;
        public bool isDead;
        public PlantGenerator plantGenerator;

        public Bud(Vector3 pos, Vector3 dir, int age, int order, PlantGenerator plantGenerator) {
            this.pos = pos;
            this.direction = dir;
            this.die_prob = plantGenerator.dieProbability;
            this.pause_prob = plantGenerator.pauseProbability;
            this.age = age;
            this.order = order;
            this.isDead = false;
            this.plantGenerator = plantGenerator;
        }

        public Vector3 getPos() {
            return this.pos;
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

    // Sections of the plant that hold buds
    public class Node
    {
        public Vector3 pos;
        public List<Bud> sideBuds;
        public Bud tipBud;

        public Node(Vector3 pos, Bud tipBud) {
            this.pos = pos;
            this.sideBuds = new List<Bud>();
            this.tipBud = tipBud;
        }

        public Bud getTipBud() {
            return this.tipBud;
        }

        public List<Bud> getSideBuds() {
            return this.sideBuds;
        }

        public void addSideBud(Bud bud) {
            this.sideBuds.Add(bud);
        }
    }

    // An entire path of nodes of a plant
    public class Segment
    {
        List<Node> nodes;
        Mesh segMesh;

        PlantGenerator plantGenerator;

        public Segment(Node node, PlantGenerator plantGenerator) {
            this.nodes = new List<Node>();
            nodes.Add(node);
            segMesh = new Mesh();
            this.plantGenerator = plantGenerator;
        }

        public List<Node> getNodes() {
            return this.nodes;
        }

        public Mesh getMesh() {
            return this.segMesh;
        }

        public void setMesh(Mesh mesh) {
            this.segMesh = mesh;
        }

        // Grows the segment by adding a new node in the direction
        // of the tipNode's tipBud
        // Returns the new tipNode
        public Node grow() {
            Node tipNode = nodes[nodes.Count - 1];
            Bud tipBud = tipNode.getTipBud();
            int segOrder = tipBud.getOrder();
            int age = tipBud.getAge();

            Vector3 oldTipPos = tipBud.getPos();
            Vector3 growthDir = tipBud.getDir();

            // Get segment's vertices and triangles of mesh
            Vector3[] oldVerts = segMesh.vertices;
            int[] oldTris = segMesh.triangles;

            int oldVertLength = oldVerts.Length;
            int oldTriLength = oldTris.Length;

            // Create new lists of vertices and tris
            Vector3[] newVerts = new Vector3[oldVertLength + 7];
            int[] newTris = new int[oldTriLength + (18 * 3)];

            // Check if segment is not just a cylinder base in mesh
            // If it is multiple cylinders, "cut off" the top cap of the mesh
            int stopCopyIndex = (oldVertLength > 7) ? oldTriLength - 18 - 1 : oldTriLength - 1;

            for (int v = 0; v < oldVertLength; v++) {
                newVerts[v] = oldVerts[v];
            }
            for (int t = 0; t <= stopCopyIndex; t++) {
                newTris[t] = oldTris[t];
            }

            // Calculate new vertices
            Vector3 T = growthDir.normalized;
            Vector3 V = (T.x == 1f) ? new Vector3(0f, 1f, 0f) : new Vector3(1f, 0f, 0f);
            Vector3 N = -1f * (Vector3.Cross(T, V)).normalized;
            Vector3 B = Vector3.Cross(T, N);

            Vector3 newTipPos = oldTipPos + (T * plantGenerator.growthLength);

            //newVerts[oldVertLength] = newTipPos;
            //newVerts[oldVertLength + 1] = newTipPos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * 0f), 0f, Mathf.Sin(Mathf.Deg2Rad * 0f));
            //newVerts[oldVertLength + 2] = newTipPos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * 60f), 0f, Mathf.Sin(Mathf.Deg2Rad * 60f));
            //newVerts[oldVertLength + 3] = newTipPos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * 120f), 0f, Mathf.Sin(Mathf.Deg2Rad * 120f));
            //newVerts[oldVertLength + 4] = newTipPos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * 180f), 0f, Mathf.Sin(Mathf.Deg2Rad * 180f));
            //newVerts[oldVertLength + 5] = newTipPos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * 240f), 0f, Mathf.Sin(Mathf.Deg2Rad * 240f));
            //newVerts[oldVertLength + 6] = newTipPos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * 300f), 0f, Mathf.Sin(Mathf.Deg2Rad * 300f));

            newVerts[oldVertLength] = newTipPos;
            newVerts[oldVertLength + 1] = newTipPos + (((N * Mathf.Cos(Mathf.Deg2Rad * 0f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 0f))) / ((segOrder + age) * 1f / 2f));
            newVerts[oldVertLength + 2] = newTipPos + (((N * Mathf.Cos(Mathf.Deg2Rad * 60f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 60f))) / ((segOrder + age) * 1f / 2f));
            newVerts[oldVertLength + 3] = newTipPos + (((N * Mathf.Cos(Mathf.Deg2Rad * 120f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 120f))) / ((segOrder + age) * 1f / 2f));
            newVerts[oldVertLength + 4] = newTipPos + (((N * Mathf.Cos(Mathf.Deg2Rad * 180f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 180f))) / ((segOrder + age) * 1f / 2f));
            newVerts[oldVertLength + 5] = newTipPos + (((N * Mathf.Cos(Mathf.Deg2Rad * 240f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 240f))) / ((segOrder + age) * 1f / 2f));
            newVerts[oldVertLength + 6] = newTipPos + (((N * Mathf.Cos(Mathf.Deg2Rad * 300f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 300f))) / ((segOrder + age) * 1f / 2f));

            int newVertsLength = newVerts.Length;

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

            // Make new new tipBud and new tipNode and add it to segment's node list
            //Vector3 newTipDir = new Vector3((Random.value * 2f) - 1f, 1f + Random.value, (Random.value * 2f) - 1f);
            //Vector3 newTipDir = new Vector3(Random.value - 0.5f, 0.25f + Random.value, Random.value - 0.5f);
            Vector3 newTipDir = T;
            if (segOrder > 1) {
                newTipDir += new Vector3(0f, plantGenerator.tropism, 0);
            }
            Bud newTipBud = new Bud(newTipPos, newTipDir, age + 1, segOrder, plantGenerator);
            Node newTipNode = new Node(newTipPos, newTipBud);

            // Add a number of branches at the new node
            int numBranches = Random.Range(0, plantGenerator.maxSideBudsPerNode);
            for (int i = 0; i < numBranches; i++) {
                Vector3 dir = new Vector3((Random.value * 2f) - 1f, Random.value / 4f, (Random.value * 2f) - 1f);
                //Vector3 dir = new Vector3(1, 1, 1);
                Vector3 sideBudPos = newTipPos - T;
                Bud sideBud = new Bud(sideBudPos, dir, age, segOrder + 1, plantGenerator);
                newTipNode.addSideBud(sideBud);
            }

            nodes.Add(newTipNode);

            // Set the new vertices and triangles for the mesh
            segMesh.vertices = newVerts;
            segMesh.triangles = newTris;
            segMesh.RecalculateNormals();

            return newTipNode;
        }
    }


    ////////////////////////////////////////////////////////////////////////////////////

    ///////////////////////////////// Functions ////////////////////////////////////////

    // Begins a new segnment by create a tip bud, a node, and the segment object
    // Also creates the mesh for the base of the first cylinder
    public void startSegment(Vector3 pos, int age, int order) {
        // If this is the first segment of the plant, make its direction up
        Vector3 dir;
        if (segments.Count == 0) {
            dir = new Vector3(0f, 1f, 0f);
        }
        else {
            dir = new Vector3((Random.value * 2f) - 1f, 0.1f + Random.value, (Random.value * 2f) - 1f);
            //dir = new Vector3(1f, 1f, 1f);
            //dir = new Vector3((Random.value * 2f) - 1f, Random.value / 1.5f, (Random.value * 2f) - 1f);
            //dir = new Vector3(Random.value - 0.5f, 1f + Random.value, Random.value - 0.5f);
        }
        Bud tipBud = new Bud(pos, dir, age, order, this);
        Node startNode = new Node(pos, tipBud);
        Segment newSegment = new Segment(startNode, this);

        // Create cylinder base in mesh
        Mesh startMesh = new Mesh();
        Vector3[] verts = new Vector3[7];
        int[] tris = new int[6 * 3];

        Vector3 T = dir.normalized;
        Vector3 V = (T.x == 1f) ? new Vector3(0f, 1f, 0f) : new Vector3(1f, 0f, 0f);
        Vector3 N = -1f * (Vector3.Cross(T, V)).normalized;
        Vector3 B = Vector3.Cross(T, N);

        //verts[0] = pos;
        //verts[1] = pos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * 0f), 0f, Mathf.Sin(Mathf.Deg2Rad * 0f));
        //verts[2] = pos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * 60f), 0f, Mathf.Sin(Mathf.Deg2Rad * 60f));
        //verts[3] = pos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * 120f), 0f, Mathf.Sin(Mathf.Deg2Rad * 120f));
        //verts[4] = pos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * 180f), 0f, Mathf.Sin(Mathf.Deg2Rad * 180f));
        //verts[5] = pos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * 240f), 0f, Mathf.Sin(Mathf.Deg2Rad * 240f));
        //verts[6] = pos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * 300f), 0f, Mathf.Sin(Mathf.Deg2Rad * 300f));

        verts[0] = pos;
        verts[1] = pos + (((N * Mathf.Cos(Mathf.Deg2Rad * 0f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 0f))) / ((age + order) * 1f / 2f));
        verts[2] = pos + (((N * Mathf.Cos(Mathf.Deg2Rad * 60f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 60f))) / ((age + order) * 1f / 2f));
        verts[3] = pos + (((N * Mathf.Cos(Mathf.Deg2Rad * 120f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 120f))) / ((age + order) * 1f / 2f));
        verts[4] = pos + (((N * Mathf.Cos(Mathf.Deg2Rad * 180f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 180f))) / ((age + order) * 1f / 2f));
        verts[5] = pos + (((N * Mathf.Cos(Mathf.Deg2Rad * 240f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 240f))) / ((age + order) * 1f / 2f));
        verts[6] = pos + (((N * Mathf.Cos(Mathf.Deg2Rad * 300f)) + (B * Mathf.Sin(Mathf.Deg2Rad * 300f))) / ((age + order) * 1f / 2f));

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

        newSegment.setMesh(startMesh);

        // Add to global list of segments
        segments.Add(newSegment);
    }

    // Method for generating a plant object
    public GameObject generatePlant() {

        // Create the first segment
        startSegment(new Vector3(0f, 0f, 0f), 1, 1);

        // Grow segments
        for (int i = 0; i < growthCycles; i++) {

            // List of new nodes being made
            List<Node> newNodes = new List<Node>();

            // Grow each segment
            foreach (Segment seg in segments) {
                List<Node> segNodes = seg.getNodes();
                Bud tipBud = segNodes[segNodes.Count - 1].getTipBud();

                int tipBudAge = tipBud.getAge();

                // Check if bud should die -> if so then set bud to die
                // If it should not die, check if its already dead or should pause growth
                float deathValue = Random.value;
                if (deathValue <= tipBud.getDiePob() + (0.015f * tipBudAge)) {
                    //string message = deathValue.ToString() + ", " + (tipBud.getDiePob() + (0.015f * tipBudAge)).ToString();
                    //Debug.Log(message);
                    tipBud.setDead();
                }
                if (!tipBud.getIsDead() && Random.value > tipBud.getPause()) {
                    Node newNode = seg.grow();
                    newNodes.Add(newNode);
                }
            }

            //print(newNodes[0].getTipBud().getOrder());

            // Start new segments at new nodes' side buds
            foreach (Node newNode in newNodes) {
                foreach (Bud sidebud in newNode.getSideBuds()) {
                    startSegment(sidebud.getPos(), sidebud.getAge(), sidebud.getOrder());
                }
            }
        }

        GameObject plant = new GameObject();
        plant.name = this.name + "_Generated";

        // Create the segment objects
        foreach (Segment seg in segments) {
            GameObject segment = new GameObject();
            segment.name = "Segment";
            segment.AddComponent<MeshFilter>();
            segment.AddComponent<MeshRenderer>();
            segment.GetComponent<MeshFilter>().mesh = seg.getMesh();
            Renderer rend = segment.GetComponent<Renderer>();
            rend.material.color = new Color(0.5f, 0.2f, 0.05f, 1.0f);  // bark color
            segment.transform.parent = plant.transform;

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
