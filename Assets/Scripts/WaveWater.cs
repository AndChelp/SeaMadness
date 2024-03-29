using UnityEngine;
[RequireComponent(typeof(MeshFilter))]
public class WaveWater : MonoBehaviour {
    public float waveX;

    public float waveLength = 10f;
    // public float waveZ = 0f;

    public float waveCount = 0.5f;
    public float waveSpeed = 10f;
    public float amplitude = 1f;
    MeshFilter _meshFilter;

    void Awake() {
        _meshFilter = GetComponent<MeshFilter>();
    }

    void Update() {
        var oldVertices = _meshFilter.mesh.vertices;
        var newVertices = new Vector3[oldVertices.Length];

        for (var i = 0; i < oldVertices.Length; i++) {
            var old = oldVertices[i];
            var f = waveCount * (old.x - waveSpeed * Time.timeSinceLevelLoad);
            old.y = amplitude * Mathf.Sin(f);
            newVertices[i] = new Vector3(old.x, old.y, old.z);
        }

        _meshFilter.mesh.vertices = newVertices;
        _meshFilter.mesh.RecalculateNormals();
    }
}