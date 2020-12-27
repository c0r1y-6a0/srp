using UnityEngine;

namespace MySRP
{
    public class MeshBall : MonoBehaviour
    {

        static int baseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField]
        Mesh mesh = default;

        [SerializeField]
        Material material = default;

        const int c_BallCount = 1023;

        Matrix4x4[] matrices = new Matrix4x4[c_BallCount];
        Vector4[] baseColors = new Vector4[c_BallCount];

        MaterialPropertyBlock block;

        void Awake()
        {
            for (int i = 0; i < c_BallCount; i++)
            {
                matrices[i] = Matrix4x4.TRS(Random.insideUnitSphere * 10f, Quaternion.identity, Vector3.one);
                baseColors[i] = new Vector4(Random.value, Random.value, Random.value, 1f);
            }
        }

        void Update()
        {
            if (block == null)
            {
                block = new MaterialPropertyBlock();
                block.SetVectorArray(baseColorId, baseColors);
            }
            Graphics.DrawMeshInstanced(mesh, 0, material, matrices, c_BallCount, block);

/*
            for(int i = 0 ; i < c_BallCount; i++)
            {
                Graphics.DrawMesh(mesh, matrices[i], material, 0);
            }
            */
        }
    }

}
