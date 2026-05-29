using UnityEngine;

public class GroundIndicator : MonoBehaviour
{
    public void Show(bool show)
    {
        gameObject.SetActive(show);
    }

    public void SetIndicatorPosition(Vector3 position)
    {
        transform.position = position;
    }

}
