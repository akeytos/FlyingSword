using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelSettings", menuName = "SwordSlide/Level Settings")]
public class LevelSettings : ScriptableObject
{
    public float chunkSize = 50f;
    public int viewDistance = 1;

    public string groundTag = "Ground";

    public bool showGizmos = true;
    public Color gizmoColor = Color.green;
}