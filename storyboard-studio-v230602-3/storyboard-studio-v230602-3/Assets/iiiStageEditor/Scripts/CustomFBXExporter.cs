using UnityEngine;
using System.Collections;
using System.IO;
//using UnityEditor;

public class CustomFBXExporter : MonoBehaviour
{
    public GameObject exportObject = null;
    public string filePath = "";
    public float zoom = 100.0f;
    public int rotationPlotQuarity = 120;
    public bool exportFbx = false;
    public bool exportTexture = false;

    private GameObject lstObj;
    private string lstName;

    private string expTxt0 = "";
    private string expTxt1 = "";
    private Transform[] tfs = new Transform[0];
    enum Type
    {
        Null,
        Mesh,
        Skin
    }
    private Type[] types = new Type[0];
    private AnimationClip clip;

    private bool isSkin = false;
    private int mdlCnt = 0;
    private int dfmCnt = 0;
    private int pseCnt = 0;

    void Update()
    {
        if (exportObject != lstObj)
        {
            lstObj = exportObject;
            filePath = "";
        }
        else if (exportObject != null && exportObject.name != lstName)
        {
            lstName = exportObject.name;
            filePath = "";
        }
        if (exportFbx)
        {
            exportFbx = false;
            ExportMain();
        }
        if (exportTexture)
        {
            exportTexture = false;
            ExportTexture();
        }
    }

    void ExportMain()
    {
        if (!exportObject) { return; }
        if (filePath == "") { filePath = exportObject.name; }

        SubMeshCheck();

        exportObject.transform.parent = null;
        int num;
        num = ObjCount(exportObject.transform, 0);
        tfs = new Transform[num];
        types = new Type[num];


        Dig(exportObject.transform, 0);

        expTxt0 = "";
        expTxt1 = "";

        //Body//
        AddExpTxt1("; Object properties");
        AddExpTxt1(";------------------------------------------------------------------");
        AddExpTxt1("");
        AddExpTxt1("Objects: {");
        ObjectsTxt(num);
        if (isSkin) { PoseTxt(num); }
        if (isSkin) { DeformerTxt(num); }
        AddExpTxt1("}");
        AddExpTxt1("");
        ConnectTxt(num);

        //Header//
        AddExpTxt0("; F 6.1 project file");
        AddExpTxt0("");
        AddExpTxt0("; Object definitions");
        AddExpTxt0(";------------------------------------------------------------------");
        AddExpTxt0("");
        AddExpTxt0("Definitions: {");
        AddExpTxt0("    Count: " + (mdlCnt + dfmCnt + pseCnt));
        if (mdlCnt > 0)
        {
            AddExpTxt0("    ObjectType: \"Model\" {");
            AddExpTxt0("        Count: " + mdlCnt);
            AddExpTxt0("    }");
        }
        if (dfmCnt > 0)
        {
            AddExpTxt0("    ObjectType: \"Deformer\" {");
            AddExpTxt0("        Count: " + dfmCnt);
            AddExpTxt0("    }");
        }
        if (pseCnt > 0)
        {
            AddExpTxt0("    ObjectType: \"Pose\" {");
            AddExpTxt0("        Count: " + pseCnt);
            AddExpTxt0("    }");
        }
        AddExpTxt0("}");
        AddExpTxt0("");

        GetAnimationClip();
        if (clip != null) { AnimationText(); }

        Export("Assets/" + filePath + ".fbx", expTxt0 + expTxt1);

        SubMeshEndProcess();
    }

    //Number of objects in the hierarchy//
    int ObjCount(Transform tf, int num)
    {
        num++;

        int i;
        for (i = 0; i < tf.childCount; i++)
        {
            num = ObjCount(tf.GetChild(i), num);
        }

        return num;
    }

    //Hierarchy expansion//
    int Dig(Transform tf, int no)
    {
        MeshRenderer mr = tf.GetComponent("MeshRenderer") as MeshRenderer;
        SkinnedMeshRenderer smr = tf.GetComponent("SkinnedMeshRenderer") as SkinnedMeshRenderer;

        tfs[no] = tf;
        if (mr)
        {
            types[no] = Type.Mesh;
        }
        else if (smr)
        {
            types[no] = Type.Skin;
            isSkin = true;
        }
        else
        {
            types[no] = Type.Null;
        }

        no++;

        int i;
        for (i = 0; i < tf.childCount; i++)
        {
            no = Dig(tf.GetChild(i), no);
        }

        return no;
    }

    //Add 1 line of text//
    void AddExpTxt0(string txt)
    {
        expTxt0 += txt + "\n";
    }
    void AddExpTxt1(string txt)
    {
        expTxt1 += txt + "\n";
    }

    //Object definition in scene//
    void ObjectsTxt(int num)
    {
        int i;
        for (i = 0; i < num; i++)
        {
            if (types[i] == Type.Null)
            {
                NullTxt(i);
            }
            else if (types[i] == Type.Mesh)
            {
                MeshTxt(i, types[i]);
            }
            else if (types[i] == Type.Skin)
            {
                MeshTxt(i, types[i]);
            }
        }
    }

    //Empty GameObject definition text//
    void NullTxt(int no)
    {
        Vector3 pos = VectorExponential(tfs[no].localPosition);
        pos.x *= -1;
        pos *= zoom;
        //回転軸順を調整 unity:zxy FBX:xyz//
        Quaternion rot = tfs[no].localRotation;
        Vector3 eul = VectorExponential(GetEulerXYZ(rot));
        eul.y = -eul.y;
        eul.z = -eul.z;
        Vector3 scl = VectorExponential(tfs[no].localScale);

        AddExpTxt1("    Model: \"Model::" + tfs[no].name + "\",\"Null\" {");
        AddExpTxt1("        Properties60: {");
        AddExpTxt1("            Property: \"Lcl Translation\", \"Lcl Translation\", \"A+\"," + VectorTxt(pos));
        AddExpTxt1("            Property: \"Lcl Rotation\", \"Lcl Rotation\", \"A+\"," + VectorTxt(eul));
        AddExpTxt1("            Property: \"Lcl Scaling\", \"Lcl Scaling\", \"A+\"," + VectorTxt(scl));
        AddExpTxt1("        }");
        AddExpTxt1("    }");

        mdlCnt++;
    }

    //Mesh definition text//
    void MeshTxt(int no, Type type)
    {
        Vector3 pos = VectorExponential(tfs[no].localPosition);
        pos.x = -pos.x;
        pos *= zoom;
        //回転軸順を調整 unity:zxy FBX:xyz//
        Quaternion rot = tfs[no].localRotation;
        Vector3 eul = VectorExponential(GetEulerXYZ(rot));
        eul.y *= -1;
        eul.z *= -1;
        Vector3 siz = VectorExponential(tfs[no].localScale);

        AddExpTxt1("    Model: \"Model::" + tfs[no].name + "\",\"Mesh\" {");
        AddExpTxt1("        Properties60: {");
        AddExpTxt1("            Property: \"Lcl Translation\", \"Lcl Translation\", \"A+\"," + VectorTxt(pos));
        AddExpTxt1("            Property: \"Lcl Rotation\", \"Lcl Rotation\", \"A+\"," + VectorTxt(eul));
        AddExpTxt1("            Property: \"Lcl Scaling\", \"Lcl Scaling\", \"A+\"," + VectorTxt(siz));
        AddExpTxt1("        }");

        int i;
        Mesh mesh = null;
        if (type == Type.Mesh)
        {
            MeshFilter meshf = tfs[no].GetComponent("MeshFilter") as MeshFilter;
            mesh = meshf.sharedMesh;
        }
        else if (type == Type.Skin)
        {
            SkinnedMeshRenderer smr = tfs[no].GetComponent("SkinnedMeshRenderer") as SkinnedMeshRenderer;
            mesh = smr.sharedMesh;
        }

        //Vertices//
        Vector3[] vrts = mesh.vertices;
        string vrtTxt = "";
        for (i = 0; i < mesh.vertexCount; i++)
        {
            Vector3 vrt = VectorExponential(vrts[i]);
            vrt.x = -vrt.x;
            vrt *= zoom;
            if (i > 0)
                vrtTxt += ",";
            vrtTxt += VectorTxt(vrt);
        }
        AddExpTxt1("        Vertices: " + vrtTxt);

        //Triangles//
        int[] tris = mesh.triangles;
        string triTxt = "";
        for (i = 0; i < tris.Length / 3; i++)
        {
            if (i > 0)
                triTxt += ",";
            triTxt += tris[i * 3 + 2];
            triTxt += ",";
            triTxt += tris[i * 3 + 1];
            triTxt += ",";
            triTxt += -(tris[i * 3 + 0] + 1);
        }
        AddExpTxt1("        PolygonVertexIndex: " + triTxt);

        //Normals//
        Vector3[] nrms = mesh.normals;
        string nrmTxt = "";
        for (i = 0; i < nrms.Length; i++)
        {
            Vector3 nrm = nrms[i];
            nrm.x *= -1;
            if (i > 0)
                nrmTxt += ",";
            nrmTxt += VectorTxt(nrm);
        }
        AddExpTxt1("        LayerElementNormal: 0 {");
        AddExpTxt1("            MappingInformationType: \"ByVertice\"");
        AddExpTxt1("            ReferenceInformationType: \"Direct\"");
        AddExpTxt1("            Normals: " + nrmTxt);
        AddExpTxt1("        }");

        //UV//
        Vector2[] uvs = mesh.uv;
        if (uvs.Length > 0)
        {
            string uvTxt = "";
            string uviTxt = "";
            for (i = 0; i < uvs.Length; i++)
            {
                Vector2 uv = uvs[i];
                if (i > 0)
                    uvTxt += ",";
                uvTxt += FloatTxt(uv.x) + "," + FloatTxt(uv.y);

                if (i > 0)
                    uviTxt += ",";
                uviTxt += i;
            }
            AddExpTxt1("        LayerElementUV: 0 {");
            AddExpTxt1("            MappingInformationType: \"ByVertice\"");
            AddExpTxt1("            ReferenceInformationType: \"IndexToDirect\"");
            AddExpTxt1("            UV: " + uvTxt);
            AddExpTxt1("            UVIndex: " + uviTxt);
            AddExpTxt1("        }");
        }

        Vector2[] uv2s = mesh.uv2;
        if (uv2s.Length > 0)
        {
            if (uv2s.Length > 0)
            {
                string uvTxt = "";
                string uviTxt = "";
                for (i = 0; i < uv2s.Length; i++)
                {
                    Vector2 uv = uv2s[i];
                    if (i > 0)
                        uvTxt += ",";
                    uvTxt += FloatTxt(uv.x) + "," + FloatTxt(uv.y);

                    if (i > 0)
                        uviTxt += ",";
                    uviTxt += i;
                }
                AddExpTxt1("        LayerElementUV: 1 {");
                AddExpTxt1("            MappingInformationType: \"ByVertice\"");
                AddExpTxt1("            ReferenceInformationType: \"IndexToDirect\"");
                AddExpTxt1("            UV: " + uvTxt);
                AddExpTxt1("            UVIndex: " + uviTxt);
                AddExpTxt1("        }");
            }
        }

        //Vertex Colors//
        Color[] colrs = mesh.colors;
        if (colrs.Length > 0)
        {
            string colrTxt = "";
            string colriTxt = "";
            for (i = 0; i < colrs.Length; i++)
            {
                Color clr = colrs[i];
                if (i > 0)
                    colrTxt += ",";
                colrTxt += clr.r + "," + clr.g + "," + clr.b + "," + clr.a;

                if (i > 0)
                    colriTxt += ",";
                colriTxt += i;
            }

            AddExpTxt1("        LayerElementColor: 0 {");
            AddExpTxt1("            MappingInformationType: \"ByVertice\"");
            AddExpTxt1("            ReferenceInformationType: \"IndexToDirect\"");
            AddExpTxt1("            Colors: " + colrTxt);
            AddExpTxt1("            ColorIndex: " + colriTxt);
            AddExpTxt1("        }");

        }

        //Layer infomation//
        AddExpTxt1("        Layer: 0 {");
        AddExpTxt1("            LayerElement: {");
        AddExpTxt1("                Type: \"LayerElementNormal\"");
        AddExpTxt1("                TypedIndex: 0");
        AddExpTxt1("            }");
        if (uvs.Length > 0)
        {
            AddExpTxt1("            LayerElement: {");
            AddExpTxt1("                Type: \"LayerElementUV\"");
            AddExpTxt1("                TypedIndex: 0");
            AddExpTxt1("            }");
        }
        if (colrs.Length > 0)
        {
            AddExpTxt1("            LayerElement: {");
            AddExpTxt1("                Type: \"LayerElementColor\"");
            AddExpTxt1("                TypedIndex: 0");
            AddExpTxt1("            }");
        }
        AddExpTxt1("        }");
        if (uv2s.Length > 0)
        {
            AddExpTxt1("        Layer: 1 {");
            AddExpTxt1("            LayerElement: {");
            AddExpTxt1("                Type: \"LayerElementUV\"");
            AddExpTxt1("                TypedIndex: 1");
            AddExpTxt1("            }");
            AddExpTxt1("        }");
        }
        //AddExpTxt1("      NodeAttributeName: \"Geometry::" + exportObject.name + "\"");
        AddExpTxt1("    }");

        mdlCnt++;
    }

    void PoseTxt(int num)
    {
        int i;
        AddExpTxt1("    Pose: \"Pose::BIND_POSES\", \"BindPose\" {");
        AddExpTxt1("        NbPoseNodes: " + num);

        for (i = 0; i < num; i++)
        {
            Transform tf = tfs[i];
            Transform pr = tf.parent;
            if (!pr)
                pr = tf;
            Matrix4x4 mtx = exportObject.transform.localToWorldMatrix;
            Vector3 mtxVec = tf.position - exportObject.transform.position;
            mtxVec.x *= -1;
            mtx[0, 3] = mtxVec.x;
            mtx[1, 3] = mtxVec.y;
            mtx[2, 3] = mtxVec.z;

            mtx = MatrixExponential(mtx);
            string mtxTxt = MatrixTxt(mtx);

            AddExpTxt1("        PoseNode: {");
            AddExpTxt1("            Node: \"Model::" + tf.name + "\"");
            AddExpTxt1("            Matrix: " + mtxTxt);
            AddExpTxt1("        }");
        }
        AddExpTxt1("    }");
        pseCnt++;
    }

    //Deformer definition//
    void DeformerTxt(int num)
    {
        int i, j, k;

        Transform[] bnes = new Transform[0];
        BoneWeight[] wits = new BoneWeight[0];
        for (i = 0; i < num; i++)
        {
            if (types[i] == Type.Skin)
            {
                SkinnedMeshRenderer smr = tfs[i].GetComponent("SkinnedMeshRenderer") as SkinnedMeshRenderer;
                bnes = smr.bones;
                Mesh mesh = smr.sharedMesh;
                wits = mesh.boneWeights;

                AddExpTxt1("    Deformer: \"Deformer::Skin " + tfs[i].name + "\", \"Skin\" {");
                AddExpTxt1("        Type: \"Skin\"");
                AddExpTxt1("    }");
                dfmCnt++;

                for (j = 0; j < bnes.Length; j++)
                {
                    Transform bne = bnes[j];
                    string idxTxt = "";
                    string witTxt = "";
                    for (k = 0; k < wits.Length; k++)
                    {
                        BoneWeight wit = wits[k];
                        if (wit.boneIndex0 == j && wit.weight0 > 0)
                        {
                            if (idxTxt.Length > 0)
                                idxTxt += ",";
                            if (witTxt.Length > 0)
                                witTxt += ",";
                            idxTxt += k;
                            witTxt += FloatExponential(wit.weight0);
                        }
                        else if (wit.boneIndex1 == j && wit.weight1 > 0)
                        {
                            if (idxTxt.Length > 0)
                                idxTxt += ",";
                            if (witTxt.Length > 0)
                                witTxt += ",";
                            idxTxt += k;
                            witTxt += FloatExponential(wit.weight1);
                        }
                        else if (wit.boneIndex2 == j && wit.weight2 > 0)
                        {
                            if (idxTxt.Length > 0)
                                idxTxt += ",";
                            if (witTxt.Length > 0)
                                witTxt += ",";
                            idxTxt += k;
                            witTxt += FloatExponential(wit.weight2);
                        }
                        else if (wit.boneIndex3 == j && wit.weight3 > 0)
                        {
                            if (idxTxt.Length > 0)
                                idxTxt += ",";
                            if (witTxt.Length > 0)
                                witTxt += ",";
                            idxTxt += k;
                            witTxt += FloatExponential(wit.weight3);
                        }
                    }


                    //Create bone rotation matrix//
                    Transform parent = bne.parent;
                    if (!parent)
                        parent = bne;

                    Quaternion bRot = bne.rotation;
                    Vector3 eulr = bRot.eulerAngles;
                    eulr.y *= -1;
                    eulr.z *= -1;
                    bne.rotation = Quaternion.Euler(eulr);

                    Matrix4x4 mtx0 = tfs[i].transform.localToWorldMatrix * bne.worldToLocalMatrix;
                    //mtx0[0, 3] *= -1;//FBXとX方向が逆

                    Vector3 mtxVec = bne.position;
                    mtxVec.y *= -1;
                    mtxVec.z *= -1;
                    mtxVec = Quaternion.Inverse(bne.rotation) * mtxVec;
                    mtx0[0, 3] = mtxVec.x;
                    mtx0[1, 3] = mtxVec.y;
                    mtx0[2, 3] = mtxVec.z;

                    mtx0 = MatrixExponential(mtx0);
                    string tfTxt = MatrixTxt(mtx0);

                    bne.rotation = bRot;

                    Matrix4x4 mtx1 = parent.localToWorldMatrix * bne.worldToLocalMatrix;
                    mtx1 = MatrixExponential(mtx1);
                    string tflTxt = MatrixTxt(mtx1);

                    AddExpTxt1("    Deformer: \"SubDeformer::Cluster " + bne.name + "_" + tfs[i].name + "\", \"Cluster\" {");
                    AddExpTxt1("        Type: \"Cluster\"");
                    AddExpTxt1("        Indexes: " + idxTxt);
                    AddExpTxt1("        Weights: " + witTxt);
                    AddExpTxt1("        Transform: " + tfTxt);
                    AddExpTxt1("        TransformLink: " + tflTxt);
                    AddExpTxt1("    }");

                    dfmCnt++;
                }
            }
        }

    }

    //Connection definition//
    void ConnectTxt(int num)
    {
        AddExpTxt1("; Object connections");
        AddExpTxt1(";------------------------------------------------------------------");
        AddExpTxt1("");
        AddExpTxt1("Connections: {");

        int i, j;
        string name = "";
        string pname = "";
        Transform tf = null;
        Transform prt = null;
        for (i = 0; i < num; i++)
        {
            tf = tfs[i];
            prt = tf.parent;
            if (prt)
            {
                pname = prt.name;
            }
            else
            {
                pname = "Scene";
            }
            name = tfs[i].name;

            AddExpTxt1("    Connect: \"OO\", \"Model::" + name + "\", \"Model::" + pname + "\"");
        }

        Transform[] bnes = new Transform[0];
        int skinCount = 0;
        if (isSkin)
        {
            for (i = 0; i < num; i++)
            {
                if (types[i] == Type.Skin)
                {
                    tf = tfs[i];
                    SkinnedMeshRenderer smr = tf.GetComponent("SkinnedMeshRenderer") as SkinnedMeshRenderer;
                    if (!smr)
                        continue;

                    skinCount++;
                    string skinName = "Skin " + tf.name;
                    AddExpTxt1("    Connect: \"OO\", \"Deformer::" + skinName + "\", \"Model::" + tf.name + "\"");

                    bnes = smr.bones;
                    for (j = 0; j < bnes.Length; j++)
                    {
                        Transform bne = bnes[j];
                        AddExpTxt1("    Connect: \"OO\", \"SubDeformer::Cluster " + bne.name + "_" + tf.name + "\", \"Deformer::" + skinName + "\"");
                        AddExpTxt1("    Connect: \"OO\", \"Model::" + bne.name + "\", \"SubDeformer::Cluster " + bne.name + "_" + tf.name + "\"");
                    }
                }
            }
        }
        AddExpTxt1("}");
    }

    //Animation definition//
    void AnimationText()
    {
        AddExpTxt1(";Takes and animation section");
        AddExpTxt1(";------------------------------------------------------------------");
        AddExpTxt1("");
        AddExpTxt1("Takes:  {");
        AddExpTxt1("    Current: \"" + clip.name + "\"");
        AddExpTxt1("    Take: \"" + clip.name + "\" {");
        AddExpTxt1("        FileName: \"" + clip.name + ".tak\"");
        AddExpTxt1("        LocalTime: 0," + FloatExponential(clip.length));
        AddExpTxt1("        ReferenceTime: 0," + FloatExponential(clip.length));
        AddExpTxt1("");
        AddExpTxt1("        ;Models animation");
        AddExpTxt1("        ;----------------------------------------------------");
        AnimationModelText();
        AddExpTxt1("    }");
        AddExpTxt1("}");
    }

    public AnimationData[] animationDatas;
    void AnimationModelText()
    {
        GetAnimationDatas();
        for (int i = 0; i < animationDatas.Length; i++)
        {
            var animData = animationDatas[i];
            var anmTf = exportObject.transform.Find(animData.path);
            string name = animData.name;
            AddExpTxt1("        Model: \"Model::" + name + "\" {");
            AddExpTxt1("            Version: 1.1");
            AddExpTxt1("            Channel: \"Transform\" {");
            if (animData.positionX.length > 0 && animData.positionY.length > 0 && animData.positionZ.length > 0)
            {
                var defaultPos = Vector3.zero;
                if (anmTf != null)
                {
                    defaultPos = anmTf.localPosition;
                }
                AddExpTxt1("                Channel: \"T\" {");
                AddExpTxt1("                    Channel: \"X\" {");
                AddExpTxt1("                        Default: " + FloatExponential(defaultPos.x * zoom));
                AddExpTxt1("                        KeyVer: 4005");
                AddExpTxt1("                        KeyCount: " + animData.positionX.keys.Length);
                AddExpTxt1("                        Key:" + KeyText(animData.positionX.keys, zoom));
                AddExpTxt1("                        Color: 1,0,0");
                AddExpTxt1("                    }");
                AddExpTxt1("                    Channel: \"Y\" {");
                AddExpTxt1("                        Default: " + FloatExponential(defaultPos.y * zoom));
                AddExpTxt1("                        KeyVer: 4005");
                AddExpTxt1("                        KeyCount: " + animData.positionY.keys.Length);
                AddExpTxt1("                        Key:" + KeyText(animData.positionY.keys, zoom));
                AddExpTxt1("                        Color: 0,1,0");
                AddExpTxt1("                    }");
                AddExpTxt1("                    Channel: \"Z\" {");
                AddExpTxt1("                        Default: " + FloatExponential(defaultPos.z * zoom));
                AddExpTxt1("                        KeyVer: 4005");
                AddExpTxt1("                        KeyCount: " + animData.positionZ.keys.Length);
                AddExpTxt1("                        Key:" + KeyText(animData.positionZ.keys, zoom));
                AddExpTxt1("                        Color: 0,0,1");
                AddExpTxt1("                    }");
                AddExpTxt1("                    LayerType: 1");
                AddExpTxt1("                }");
            }
            if (animData.eulerX.length > 0 && animData.eulerY.length > 0 && animData.eulerZ.length > 0)
            {
                var eul = Vector3.zero;
                if (anmTf != null)
                {
                    eul = GetEulerXYZ(anmTf.localRotation);
                    eul.y = -eul.y;
                    eul.z = -eul.z;
                }
                AddExpTxt1("                Channel: \"R\" {");
                AddExpTxt1("                    Channel: \"X\" {");
                AddExpTxt1("                        Default: " + FloatExponential(eul.x));
                AddExpTxt1("                        KeyVer: 4005");
                AddExpTxt1("                        KeyCount: " + animData.eulerX.keys.Length);
                AddExpTxt1("                        Key:" + KeyText(animData.eulerX.keys, 1));
                AddExpTxt1("                        Color: 1,0,0");
                AddExpTxt1("                    }");
                AddExpTxt1("                    Channel: \"Y\" {");
                AddExpTxt1("                        Default: " + FloatExponential(eul.y));
                AddExpTxt1("                        KeyVer: 4005");
                AddExpTxt1("                        KeyCount: " + animData.eulerY.keys.Length);
                AddExpTxt1("                        Key:" + KeyText(animData.eulerY.keys, 1));
                AddExpTxt1("                        Color: 0,1,0");
                AddExpTxt1("                    }");
                AddExpTxt1("                    Channel: \"Z\" {");
                AddExpTxt1("                        Default: " + FloatExponential(eul.z));
                AddExpTxt1("                        KeyVer: 4005");
                AddExpTxt1("                        KeyCount: " + animData.eulerZ.keys.Length);
                AddExpTxt1("                        Key:" + KeyText(animData.eulerZ.keys, 1));
                AddExpTxt1("                        Color: 0,0,1");
                AddExpTxt1("                    }");
                AddExpTxt1("                    LayerType: 2");
                AddExpTxt1("                }");
            }
            if (animData.scaleX.length > 0 && animData.scaleY.length > 0 && animData.scaleZ.length > 0)
            {
                var defaultScl = Vector3.zero;
                if (anmTf != null)
                {
                    defaultScl = anmTf.localPosition;
                }
                AddExpTxt1("                Channel: \"S\" {");
                AddExpTxt1("                    Channel: \"X\" {");
                AddExpTxt1("                        Default: " + FloatExponential(defaultScl.x));
                AddExpTxt1("                        KeyVer: 4005");
                AddExpTxt1("                        KeyCount: " + animData.scaleX.keys.Length);
                AddExpTxt1("                        Key:" + KeyText(animData.scaleX.keys, 1));
                AddExpTxt1("                        Color: 1,0,0");
                AddExpTxt1("                    }");
                AddExpTxt1("                    Channel: \"Y\" {");
                AddExpTxt1("                        Default: " + FloatExponential(defaultScl.y));
                AddExpTxt1("                        KeyVer: 4005");
                AddExpTxt1("                        KeyCount: " + animData.scaleY.keys.Length);
                AddExpTxt1("                        Key:" + KeyText(animData.scaleY.keys, 1));
                AddExpTxt1("                        Color: 0,1,0");
                AddExpTxt1("                    }");
                AddExpTxt1("                    Channel: \"Z\" {");
                AddExpTxt1("                        Default: " + FloatExponential(defaultScl.z));
                AddExpTxt1("                        KeyVer: 4005");
                AddExpTxt1("                        KeyCount: " + animData.scaleZ.keys.Length);
                AddExpTxt1("                        Key:" + KeyText(animData.scaleZ.keys, 1));
                AddExpTxt1("                        Color: 0,0,1");
                AddExpTxt1("                    }");
                AddExpTxt1("                    LayerType: 3");
                AddExpTxt1("                }");
                AddExpTxt1("            }");
            }
            AddExpTxt1("            Channel: \"Visibility\" {");
            AddExpTxt1("                Default: 1");
            AddExpTxt1("                Color: 0.75,0,0");
            AddExpTxt1("                LayerType: 1");
            AddExpTxt1("            }");
            AddExpTxt1("        }");
        }
    }

    string KeyText(Keyframe[] keys, float _zoom)
    {
        string text = "";
        int num = keys.Length;
        for (int i = 0; i < num; i++)
        {
            text += ((double)keys[i].time * 46232344158).ToString();//FBXの謎定数46232344158
            text += ",";
            text += FloatExponential(keys[i].value * _zoom);
            text += ",U,s,";
            text += FloatExponential(keys[i].outTangent * _zoom);
            text += ",";
            if (i < num - 1)
            {
                text += FloatExponential(keys[i + 1].inTangent * _zoom);
            }
            else
            {
                text += "0";
            }
            text += ",n";
            if (i < num - 1)
            {
                text += ",";
            }
        }
        return text;
    }

    [System.Serializable]
    public class AnimationData
    {
        public string name;
        public string path;
        public AnimationCurve positionX;
        public AnimationCurve positionY;
        public AnimationCurve positionZ;
        public AnimationCurve eulerX;
        public AnimationCurve eulerY;
        public AnimationCurve eulerZ;
        public AnimationCurve rotationX;
        public AnimationCurve rotationY;
        public AnimationCurve rotationZ;
        public AnimationCurve rotationW;
        public AnimationCurve scaleX;
        public AnimationCurve scaleY;
        public AnimationCurve scaleZ;
        public AnimationData(string _path)
        {
            name = System.IO.Path.GetFileName(_path);
            path = _path;
            positionX = new AnimationCurve();
            positionY = new AnimationCurve();
            positionZ = new AnimationCurve();
            eulerX = new AnimationCurve();
            eulerY = new AnimationCurve();
            eulerZ = new AnimationCurve();
            rotationX = new AnimationCurve();
            rotationY = new AnimationCurve();
            rotationZ = new AnimationCurve();
            rotationW = new AnimationCurve();
            scaleX = new AnimationCurve();
            scaleY = new AnimationCurve();
            scaleZ = new AnimationCurve();
        }
    }

    void GetAnimationDatas()
    {
        AnimationData[] _animDatas = new AnimationData[100];
        int pt = 0;

        /*
        var curveBindings = AnimationUtility.GetCurveBindings(clip);
        for (int i = 0; i < curveBindings.Length; i++)
        {


            var binding = curveBindings[i];
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);


            if (binding.type != typeof(Transform)) { continue; }
            var datPath = binding.path;
            int hitPt = -1;
            for (int j = 0; j < pt; j++)
            {
                if (_animDatas[j].path == datPath)
                {
                    hitPt = j;
                    break;
                }
            }
            if (hitPt == -1)
            {
                hitPt = pt;
                pt++;
                _animDatas[hitPt] = new AnimationData(datPath);
            }
            var propertyName = binding.propertyName;
            if (propertyName == "m_LocalPosition.x")
            {
                _animDatas[hitPt].positionX = CurveReverse(curve);
            }
            else if (propertyName == "m_LocalPosition.y")
            {
                _animDatas[hitPt].positionY = curve;
            }
            else if (propertyName == "m_LocalPosition.z")
            {
                _animDatas[hitPt].positionZ = curve;
            }
            else if (propertyName == "localEulerAnglesRaw.x")
            {
                _animDatas[hitPt].eulerX = curve;
            }
            else if (propertyName == "localEulerAnglesRaw.y")
            {
                _animDatas[hitPt].eulerY = curve;
            }
            else if (propertyName == "localEulerAnglesRaw.z")
            {
                _animDatas[hitPt].eulerZ = curve;
            }
            else if (propertyName == "m_LocalRotation.x")
            {
                _animDatas[hitPt].rotationX = curve;
            }
            else if (propertyName == "m_LocalRotation.y")
            {
                _animDatas[hitPt].rotationY = curve;
            }
            else if (propertyName == "m_LocalRotation.z")
            {
                _animDatas[hitPt].rotationZ = curve;
            }
            else if (propertyName == "m_LocalRotation.w")
            {
                _animDatas[hitPt].rotationW = curve;
            }
            else if (propertyName == "m_LocalScale.x")
            {
                _animDatas[hitPt].scaleX = curve;
            }
            else if (propertyName == "m_LocalScale.y")
            {
                _animDatas[hitPt].scaleY = curve;
            }
            else if (propertyName == "m_LocalScale.z")
            {
                _animDatas[hitPt].scaleZ = curve;
            }
            else
            {
                Debug.Log("AnimationPropertyName = " + propertyName);
            }
        }
        */
        animationDatas = new AnimationData[pt];
        for (int i = 0; i < pt; i++)
        {
            animationDatas[i] = _animDatas[i];
        }

        //Plot rotation once
        for (int i = 0; i < pt; i++)
        {
            var anmData = animationDatas[i];
            if (anmData.eulerX.length > 0 && anmData.eulerY.length > 0 && anmData.eulerZ.length > 0)
            {
                float anmLength = animationDatas[i].eulerX.keys[animationDatas[i].eulerX.keys.Length - 1].time;
                int numFrame = Mathf.FloorToInt(anmLength * rotationPlotQuarity) + 1;
                var keysX = new Keyframe[numFrame];
                var keysY = new Keyframe[numFrame];
                var keysZ = new Keyframe[numFrame];
                for (int j = 0; j < numFrame; j++)
                {
                    float t = anmLength * j / (numFrame - 1);
                    var rot = Quaternion.Euler(anmData.eulerX.Evaluate(t), anmData.eulerY.Evaluate(t), anmData.eulerZ.Evaluate(t));
                    var eul = GetEulerXYZ(rot);
                    eul.y = -eul.y;
                    eul.z = -eul.z;
                    keysX[j] = new Keyframe();
                    keysX[j].time = t;
                    keysX[j].value = eul.x;
                    keysY[j] = new Keyframe();
                    keysY[j].time = t;
                    keysY[j].value = eul.y;
                    keysZ[j] = new Keyframe();
                    keysZ[j].time = t;
                    keysZ[j].value = eul.z;
                }
                anmData.eulerX = new AnimationCurve(Softkeys(keysX));
                anmData.eulerY = new AnimationCurve(Softkeys(keysY));
                anmData.eulerZ = new AnimationCurve(Softkeys(keysZ));
            }
            else if (anmData.rotationX.length > 0 && anmData.rotationY.length > 0 && anmData.rotationZ.length > 0 && anmData.rotationW.length > 0)
            {
                float anmLength = animationDatas[i].rotationX.keys[animationDatas[i].rotationX.keys.Length - 1].time;
                int numFrame = Mathf.FloorToInt(anmLength * rotationPlotQuarity) + 1;
                var keysX = new Keyframe[numFrame];
                var keysY = new Keyframe[numFrame];
                var keysZ = new Keyframe[numFrame];
                for (int j = 0; j < numFrame; j++)
                {
                    float t = anmLength * j / (numFrame - 1);
                    var rot = new Quaternion(anmData.rotationX.Evaluate(t), anmData.rotationY.Evaluate(t), anmData.rotationZ.Evaluate(t), anmData.rotationW.Evaluate(t));
                    var eul = GetEulerXYZ(rot);
                    eul.y = -eul.y;
                    eul.z = -eul.z;
                    keysX[j] = new Keyframe();
                    keysX[j].time = t;
                    keysX[j].value = eul.x;
                    keysY[j] = new Keyframe();
                    keysY[j].time = t;
                    keysY[j].value = eul.y;
                    keysZ[j] = new Keyframe();
                    keysZ[j].time = t;
                    keysZ[j].value = eul.z;
                }
                anmData.eulerX = new AnimationCurve(Softkeys(keysX));
                anmData.eulerY = new AnimationCurve(Softkeys(keysY));
                anmData.eulerZ = new AnimationCurve(Softkeys(keysZ));
            }
        }
    }

    //Soften key frame slope
    public Keyframe[] Softkeys(Keyframe[] keys)
    {
        int numKeys = keys.Length;
        for (int i = 0; i < numKeys; i++)
        {
            float bt, bv, at, av;
            if (i == 0) { bt = keys[i].time; bv = keys[i].value; } else { bt = keys[i - 1].time; bv = keys[i - 1].value; }
            if (i < numKeys - 1) { at = keys[i + 1].time; av = keys[i + 1].value; } else { at = keys[i].time; av = keys[i].value; }
            float frameLength = at - bt;
            float tan;
            if (frameLength != 0) { tan = (av - bv) / frameLength; } else { tan = 0f; }
            keys[i].inTangent = tan;
            keys[i].outTangent = tan;
        }
        return keys;
    }

    AnimationCurve CurveReverse(AnimationCurve curve)
    {
        var keys = curve.keys;
        for (int i = 0; i < keys.Length; i++)
        {
            keys[i].value = -keys[i].value;
            keys[i].inTangent = -keys[i].inTangent;
            keys[i].outTangent = -keys[i].outTangent;
        }
        curve.keys = keys;
        return curve;
    }

    Vector3 VectorExponential(Vector3 vec)
    {
        return new Vector3(FloatExponential(vec.x), FloatExponential(vec.y), FloatExponential(vec.z));
    }

    Matrix4x4 MatrixExponential(Matrix4x4 mtx)
    {
        Vector4 mc0 = mtx.GetColumn(0);
        Vector4 mc1 = mtx.GetColumn(1);
        Vector4 mc2 = mtx.GetColumn(2);
        Vector4 mc3 = mtx.GetColumn(3);
        mc0[0] = FloatExponential(mc0[0]);
        mc0[1] = FloatExponential(mc0[1]);
        mc0[2] = FloatExponential(mc0[2]);
        mc0[3] = FloatExponential(mc0[3]);
        mc1[0] = FloatExponential(mc1[0]);
        mc1[1] = FloatExponential(mc1[1]);
        mc1[2] = FloatExponential(mc1[2]);
        mc1[3] = FloatExponential(mc1[3]);
        mc2[0] = FloatExponential(mc2[0]);
        mc2[1] = FloatExponential(mc2[1]);
        mc2[2] = FloatExponential(mc2[2]);
        mc2[3] = FloatExponential(mc2[3]);
        mc3[0] = FloatExponential(mc3[0]);
        mc3[1] = FloatExponential(mc3[1]);
        mc3[2] = FloatExponential(mc3[2]);
        mc3[3] = FloatExponential(mc3[3]);
        mtx.SetColumn(0, mc0);
        mtx.SetColumn(1, mc1);
        mtx.SetColumn(2, mc2);
        mtx.SetColumn(3, mc3);
        return mtx;
    }

    float FloatExponential(float f)
    {
        string s0 = "" + f;
        int len = s0.Length;
        int i, j;
        for (i = 0; i < len; i++)
        {
            if (s0.Substring(i, 1) == "E")
            {
                int exp;
                if (s0.Substring(i + 1, 1) == "+")
                {
                    exp = System.Int32.Parse(s0.Substring(i + 2, len - i - 2));
                    for (j = 0; j < exp; j++)
                    {
                        f *= 10;
                    }
                    i = len;
                }
                else
                {
                    exp = System.Int32.Parse(s0.Substring(i + 2, len - i - 2));
                    for (j = 0; j < exp; j++)
                    {
                        f /= 10;
                    }
                    i = len;
                }
            }
        }

        if (Mathf.Abs(f % 1) < 0.0001f || Mathf.Abs(f % 1) > 0.9999f)
        {
            f = Mathf.Round(f);
        }

        return f;
    }

    string VectorTxt(Vector3 vec)
    {
        string vecTxt = FloatTxt(vec.x) + "," + FloatTxt(vec.y) + "," + FloatTxt(vec.z);
        return vecTxt;
    }

    string MatrixTxt(Matrix4x4 mtx)
    {
        Vector4 mc0 = mtx.GetColumn(0);
        Vector4 mc1 = mtx.GetColumn(1);
        Vector4 mc2 = mtx.GetColumn(2);
        Vector4 mc3 = mtx.GetColumn(3);

        string mtxTxt = FloatTxt(mc0[0]) + "," + FloatTxt(mc0[1]) + "," + FloatTxt(mc0[2]) + "," + FloatTxt(mc0[3]) +
            "," + FloatTxt(mc1[0]) + "," + FloatTxt(mc1[1]) + "," + FloatTxt(mc1[2]) + "," + FloatTxt(mc1[3]) +
            "," + FloatTxt(mc2[0]) + "," + FloatTxt(mc2[1]) + "," + FloatTxt(mc2[2]) + "," + FloatTxt(mc2[3]) +
            "," + FloatTxt(mc3[0] * zoom) + "," + FloatTxt(mc3[1] * zoom) + "," + FloatTxt(mc3[2] * zoom) + "," + FloatTxt(mc3[3]);
        return mtxTxt;
    }

    string FloatTxt(float f)
    {
        return "" + FloatExponential(f);
    }

    //Sub mesh process--------------------------------------------------------------------------------
    class DivisionSubMeshState
    {
        public Transform baseObj;
        public Transform parent;
        public Transform[] divisionObjs;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public DivisionSubMeshState(Transform _baseObj)
        {
            baseObj = _baseObj;
            parent = _baseObj.parent;
            divisionObjs = new Transform[0];
            position = _baseObj.transform.localPosition;
            rotation = _baseObj.transform.localRotation;
            scale = _baseObj.transform.localScale;
        }
    }
    private DivisionSubMeshState[] divStates;

    //Split objects including submeshes into multiple mesh objects
    void SubMeshCheck()
    {
        divStates = new DivisionSubMeshState[0];
        var mfilts = exportObject.GetComponentsInChildren<MeshFilter>();
        if (mfilts.Length > 0)
        {
            divStates = new DivisionSubMeshState[mfilts.Length];
            for (int i = 0; i < mfilts.Length; i++)
            {
                var mesh = mfilts[i].sharedMesh;
                if (mesh == null) { continue; }
                if (mesh.subMeshCount > 1)
                {
                    divStates[i] = DivisionSubMesh(mfilts[i].gameObject);
                    //サブメッシュオブジェをWorldRootに置き、分割オブジェを代わりに置く
                    if (divStates[i].divisionObjs.Length > 0)
                    {
                        for (int j = 0; j < divStates[i].divisionObjs.Length; j++)
                        {
                            divStates[i].divisionObjs[j].SetParent(divStates[i].parent);
                            divStates[i].divisionObjs[j].localPosition = divStates[i].position;
                            divStates[i].divisionObjs[j].localRotation = divStates[i].rotation;
                            divStates[i].divisionObjs[j].localScale = divStates[i].scale;
                        }
                        divStates[i].baseObj.SetParent(null);
                    }
                }
            }
            return;
        }
        var srends = exportObject.GetComponentsInChildren<SkinnedMeshRenderer>();
        if (srends.Length > 0)
        {
            divStates = new DivisionSubMeshState[srends.Length];
            for (int i = 0; i < srends.Length; i++)
            {
                var mesh = srends[i].sharedMesh;
                if (mesh == null) { continue; }
                if (mesh.subMeshCount > 1)
                {
                    divStates[i] = DivisionSubMesh(srends[i].gameObject);
                    //サブメッシュオブジェをWorldRootに置き、分割オブジェを代わりに置く
                    if (divStates[i].divisionObjs.Length > 0)
                    {
                        for (int j = 0; j < divStates[i].divisionObjs.Length; j++)
                        {
                            divStates[i].divisionObjs[j].SetParent(divStates[i].parent);
                            divStates[i].divisionObjs[j].localPosition = divStates[i].position;
                            divStates[i].divisionObjs[j].localRotation = divStates[i].rotation;
                            divStates[i].divisionObjs[j].localScale = divStates[i].scale;
                        }
                        divStates[i].baseObj.SetParent(null);
                    }
                }
            }
        }
    }

    //Sub mesh object split
    DivisionSubMeshState DivisionSubMesh(GameObject meshObject)
    {
        var ret = new DivisionSubMeshState(meshObject.transform);
        if (meshObject == null) { return ret; }
        var _mrend = meshObject.GetComponent<MeshRenderer>();
        var _mfilt = meshObject.GetComponent<MeshFilter>();
        var _srend = meshObject.GetComponent<SkinnedMeshRenderer>();
        Material[] mats = new Material[0];
        Type type;
        Mesh _mesh = null;
        if (_mfilt != null)
        {
            if (_mrend == null) { return ret; }
            _mesh = _mfilt.sharedMesh;
            mats = _mrend.sharedMaterials;
            type = Type.Mesh;
        }
        else
        {
            if (_srend == null) { return ret; }
            _mesh = _srend.sharedMesh;
            mats = _srend.sharedMaterials;
            type = Type.Skin;
        }
        if (_mesh == null) { return ret; }
        int meshCount = _mesh.subMeshCount;
        if (meshCount <= 1) { return ret; }
        ret.divisionObjs = new Transform[meshCount];

        for (int i = 0; i < meshCount; i++)
        {
            Mesh mesh = new Mesh();
            mesh.name = _mesh.name + " " + i;
            GameObject gameObj = new GameObject(meshObject.name + " " + i);
            if (type == Type.Mesh)
            {
                var mrend = gameObj.AddComponent<MeshRenderer>();
                if (mats.Length <= i + 1)
                {
                    mrend.sharedMaterial = mats[i];
                }
                else if (mats.Length > 0)
                {
                    mrend.sharedMaterial = mats[0];
                }
                var mfilt = gameObj.AddComponent<MeshFilter>();
                mfilt.sharedMesh = mesh;
            }
            else if (type == Type.Skin)
            {
                var srend = gameObj.AddComponent<SkinnedMeshRenderer>();
                if (mats.Length <= i + 1)
                {
                    srend.sharedMaterial = mats[i];
                }
                else if (mats.Length > 0)
                {
                    srend.sharedMaterial = mats[0];
                }
                srend.bones = _srend.bones;
                srend.rootBone = _srend.rootBone;
                srend.sharedMesh = mesh;
            }

            Vector3[] _verts = _mesh.vertices;
            Vector2[] _uvs = _mesh.uv;
            Vector2[] _uv2s = _mesh.uv2;
            Vector2[] _uv3s = _mesh.uv3;
            Vector2[] _uv4s = _mesh.uv4;
            Vector3[] _norms = _mesh.normals;
            Color32[] _colrs = _mesh.colors32;
            Vector4[] _tans = _mesh.tangents;
            BoneWeight[] _weits = _mesh.boneWeights;
            Matrix4x4[] _poses = _mesh.bindposes;
            int[] tris = _mesh.GetTriangles(i);
            //使っている頂点を調べる
            bool[] uses = new bool[_mesh.vertexCount];
            for (int j = 0; j < tris.Length; j++)
            {
                uses[tris[j]] = true;
            }
            //使わない頂点を省いたindex
            int[] idxs = new int[_mesh.vertexCount];
            int pt = 0;
            for (int j = 0; j < _mesh.vertexCount; j++)
            {
                if (uses[j])
                {
                    idxs[j] = pt;
                    pt++;
                }
            }
            //Reconstruct only with vertices used
            Vector3[] verts = new Vector3[pt];
            Vector2[] uvs = new Vector2[pt];
            Vector2[] uv2s = new Vector2[0];
            Vector2[] uv3s = new Vector2[0];
            Vector2[] uv4s = new Vector2[0];
            if (_uv2s.Length > 0) { uv2s = new Vector2[pt]; }
            if (_uv3s.Length > 0) { uv3s = new Vector2[pt]; }
            if (_uv4s.Length > 0) { uv4s = new Vector2[pt]; }
            Vector3[] norms = new Vector3[0];
            if (_norms.Length > 0) { norms = new Vector3[pt]; }
            Color32[] colrs = new Color32[0];
            if (_colrs.Length > 0) { colrs = new Color32[pt]; }
            Vector4[] tans = new Vector4[0];
            if (_tans.Length > 0) { tans = new Vector4[pt]; }
            BoneWeight[] weits = new BoneWeight[0];
            if (_weits.Length > 0) { weits = new BoneWeight[pt]; }

            for (int j = 0; j < tris.Length; j++)
            {
                tris[j] = idxs[tris[j]];
            }
            for (int j = 0; j < _mesh.vertexCount; j++)
            {
                if (uses[j])
                {
                    verts[idxs[j]] = _verts[j];
                    uvs[idxs[j]] = _uvs[j];
                    if (_uv2s.Length > 0) { uv2s[idxs[j]] = _uv2s[j]; }
                    if (_uv3s.Length > 0) { uv3s[idxs[j]] = _uv3s[j]; }
                    if (_uv4s.Length > 0) { uv4s[idxs[j]] = _uv4s[j]; }
                    if (_norms.Length > 0) { norms[idxs[j]] = _norms[j]; }
                    if (_colrs.Length > 0) { colrs[idxs[j]] = _colrs[j]; }
                    if (_tans.Length > 0) { tans[idxs[j]] = _tans[j]; }
                    if (_weits.Length > 0) { weits[idxs[j]] = _weits[j]; }
                }
            }

            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = tris;
            if (uv2s.Length > 0) { mesh.uv2 = uv2s; }
            if (uv3s.Length > 0) { mesh.uv3 = uv3s; }
            if (uv4s.Length > 0) { mesh.uv4 = uv4s; }
            if (norms.Length > 0) { mesh.normals = norms; }
            if (colrs.Length > 0) { mesh.colors32 = colrs; }
            if (tans.Length > 0) { mesh.tangents = tans; }
            if (weits.Length > 0) { mesh.boneWeights = weits; }
            if (_poses.Length > 0) { mesh.bindposes = _poses; }
            ret.divisionObjs[i] = gameObj.transform;
        }
        return ret;
    }

    //Place the exploded object in WorldRoot and return the original submesh object
    void SubMeshEndProcess()
    {
        if (divStates != null)
        {
            for (int i = 0; i < divStates.Length; i++)
            {
                if (divStates[i] != null && divStates[i].divisionObjs.Length > 0)
                {
                    for (int j = 0; j < divStates[i].divisionObjs.Length; j++)
                    {
                        divStates[i].divisionObjs[j].SetParent(null);
                    }
                    divStates[i].baseObj.SetParent(divStates[i].parent);
                }
            }
        }
    }

    //Euler of rotation axis XYZ//
    public Vector3 GetEulerXYZ(Quaternion rot)
    {
        if (rot == Quaternion.identity)
            return Vector3.zero;
        Vector3 vec = rot * Vector3.right;
        float x = vec.x;
        float y = vec.y;
        float z = vec.z;
        float zRad = 0f;
        if (Mathf.Abs(x) > Mathf.Abs(y))
        {
            if (x > 0.00001f)
            {
                zRad = Mathf.Atan(y / x);
            }
            else if (x < -0.00001f)
            {
                zRad = Mathf.Atan(y / x) + 3.14159265f;
                if (zRad > 3.14159265f)
                    zRad -= 6.2831853f;
            }
            else if (y > 0.00001f)
            {
                zRad = -1.5707963f;
            }
            else
            {
                zRad = -1.5707963f;
            }
        }
        else
        {
            if (y > 0.00001f)
            {
                zRad = -Mathf.Atan(x / y) + 1.5707963f;
            }
            else if (y < -0.00001f)
            {
                zRad = -Mathf.Atan(x / y) - 1.5707963f;
            }
        }

        x = x * Mathf.Cos(-zRad) - y * Mathf.Sin(-zRad);

        float yRad = 0f;
        if (Mathf.Abs(x) > Mathf.Abs(z))
        {
            if (x > 0.00001f)
            {
                yRad = -Mathf.Atan(z / x);
            }
            else if (x < -0.00001f)
            {
                yRad = Mathf.Atan(z / x) + 3.14159265f;
                if (yRad > 3.14159265f)
                    yRad -= 6.2831853f;
            }
        }
        else
        {
            if (z > 0.00001f)
            {
                yRad = Mathf.Atan(x / z) - 1.5707963f;
            }
            else if (z < -0.00001f)
            {
                yRad = Mathf.Atan(x / z) + 1.5707963f;
            }
        }

        float yAng = yRad * 57.2957795f;
        float zAng = zRad * 57.2957795f;
        Quaternion xRot = Quaternion.Inverse(Quaternion.Euler(0, 0, zAng) * Quaternion.Euler(0, yAng, 0)) * rot;
        Vector3 xEul = xRot.eulerAngles;
        float xAng = xEul.x;
        if (Mathf.Abs(Mathf.DeltaAngle(xEul.y, 0)) > 136 && Mathf.Abs(Mathf.DeltaAngle(xEul.z, 0)) > 136)
            xAng = xAng * -1 + 180;

        if (xAng < -180)
            xAng = xAng % 360 + 360;
        if (xAng > 180)
            xAng = xAng % 360 - 360;
        if (yAng < -180)
            yAng = yAng % 360 + 360;
        if (yAng > 180)
            yAng = yAng % 360 - 360;
        if (zAng < -180)
            zAng = zAng % 360 + 360;
        if (zAng > 180)
            zAng = zAng % 360 - 360;

        return new Vector3(xAng, yAng, zAng);
    }

    //Extract clip from Animator or Animation on Root
    void GetAnimationClip()
    {
        clip = null;
        var animator = exportObject.GetComponent<Animator>();
        if (animator != null)
        {
            //var info = animator.GetCurrentAnimatorStateInfo(0);
            if (animator.runtimeAnimatorController != null)
            {
                var clips = animator.runtimeAnimatorController.animationClips;
                if (clips.Length > 0)
                {
                    clip = clips[0];
                }
            }
            return;
        }
        var animation = exportObject.GetComponent<Animation>();
        if (animation != null)
        {
            clip = animation.clip;
        }
    }

    void ExportTexture()
    {
        if (!exportObject) { return; }
        if (filePath == "") { filePath = exportObject.name; }
        var directory = System.IO.Path.GetDirectoryName(filePath);
        if (directory.Length != 0) { directory += "/"; }
        var rends = exportObject.GetComponents<Renderer>();
        for (int i = 0; i < rends.Length; i++)
        {
            var mats = rends[i].sharedMaterials;
            for (int j = 0; j < mats.Length; j++)
            {
                if (mats[i].HasProperty("_MainTex"))
                {
                    Texture2D tex = (Texture2D)mats[i].GetTexture("_MainTex");
                    if (tex != null)
                    {
                        var path = "Assets/" + System.IO.Path.GetDirectoryName(filePath) + tex.name + ".png";
                        Texture2D tex2 = TextreClone(tex);
                        Debug.Log(path);
                        System.IO.File.WriteAllBytes(path, tex2.EncodeToPNG());
                    }
                }
                if (mats[i].HasProperty("_DetailTex"))
                {
                    Texture2D tex = (Texture2D)mats[i].GetTexture("_DetailTex");
                    if (tex != null)
                    {
                        var path = "Assets/" + System.IO.Path.GetDirectoryName(filePath) + tex.name + ".png";
                        Texture2D tex2 = TextreClone(tex);
                        System.IO.File.WriteAllBytes(path, tex2.EncodeToPNG());
                    }
                }
                if (mats[i].HasProperty("_Texture"))
                {
                    Texture2D tex = (Texture2D)mats[i].GetTexture("_Texture");
                    if (tex != null)
                    {
                        var path = "Assets/" + System.IO.Path.GetDirectoryName(filePath) + tex.name + ".png";
                        Texture2D tex2 = TextreClone(tex);
                        System.IO.File.WriteAllBytes(path, tex2.EncodeToPNG());
                    }
                }
            }
        }
    }

    Texture2D TextreClone(Texture2D tex)
    {
        var temp = RenderTexture.GetTemporary(tex.width, tex.height);
        Graphics.Blit(tex, temp);
        var copy = new Texture2D(tex.width, tex.height);
        copy.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        copy.Apply();
        RenderTexture.ReleaseTemporary(temp);
        return copy;
    }

    void Export(string path, string str)
    {
        //AssetDatabase.DeleteAsset(path);
        StreamWriter sw;
        sw = new StreamWriter(path, true);
        sw.Write(str);
        sw.Close();
    }
}