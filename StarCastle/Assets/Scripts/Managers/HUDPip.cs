using UnityEngine;
using UnityEngine.UI;

public class HUDPip : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    public void SetFill(float amount)
    {
        fillImage.fillAmount = Mathf.Clamp01(amount);
    }
}
