using TMPro;
using UnityEngine;

public class MyUIScript : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text m_jumpCountText;

    public void UpdateJumpCountText(int jumpCount)
    {
        m_jumpCountText.text = jumpCount.ToString();
    }
}
