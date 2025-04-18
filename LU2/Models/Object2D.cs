namespace LU2.Models;

public class Object2D
{
    public string Id { get; set; } = "";
    public int PrefabId { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float ScaleX { get; set; }
    public float ScaleY { get; set; }
    public float RotationZ { get; set; }
    public int SortingLayer { get; set; }
    public string EnvironmentId { get; set; } = "";
}