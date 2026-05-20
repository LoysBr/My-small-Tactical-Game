using UnityEngine;

public interface ITacticalGroundStrategy
{
    enum IndicationType
    {
        SimpleSelection,
        MovementPreview,
        MovementConfirmation,
    }

    /// <summary>
    /// Give a feedback about a ground Position for a Character
    /// </summary>
    public Vector3 IndicateCharacterGroundLocation(Ray screenPointToRay, IndicationType indicationType);

    public Vector3 GetRandomGroundLocation();
}
