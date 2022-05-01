using UnityEngine;

namespace LowPolyWater
{
    public class LowPolyWater : MonoBehaviour
    {
        public float waveHeight = 0.5f;
        public float waveFrequency = 0.5f;
        public float waveLength = 0.75f;
        public Vector3 waveOriginPosition = new Vector3(0.0f, 0.0f, 0.0f);

        private MeshFilter _meshFilter;

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
        }

        private void FixedUpdate()
        {
            GenerateWaves();
        }

        private void GenerateWaves()
        {
            var vertices = _meshFilter.mesh.vertices;
            for (var i = 0; i < vertices.Length; i++)
            {
                var old = vertices[i];
                old.y = GetWaveYAt(old.x, old.z);
                vertices[i] = old;
            }

            _meshFilter.mesh.vertices = vertices;
            _meshFilter.mesh.RecalculateNormals();
        }

        public float GetWaveYAt(Vector3 vector)
        {
            return GetWaveYAt(vector.x, vector.z);
        }

        public float GetWaveYAt(float x, float z)
        {
            var distance = Vector3.Distance(new Vector3(x, 0f, z), waveOriginPosition) % waveLength / waveLength;
            return waveHeight * Mathf.Sin(Time.timeSinceLevelLoad * Mathf.PI * 2.0f * waveFrequency +
                                          (Mathf.PI * 2.0f * distance));
        }
    }
}