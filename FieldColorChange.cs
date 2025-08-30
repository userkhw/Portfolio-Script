using UnityEngine;

public class FieldColorChange : MonoBehaviour
{
    SpriteRenderer sr;
    float hue; // 색생값 (0~1)
    float saturation; // 채도
    float value; // 명도

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // 현재 색상을 HSV로 변환해서 초기값 가져오기
        Color.RGBToHSV(sr.color, out hue, out saturation, out value);
    }

    void Update()
    {
        if (PointerRotator.isGameStart)
        {
            hue += Time.unscaledDeltaTime * 0.01f;
            if (hue > 1f) hue -= 1f;
            sr.color = Color.HSVToRGB(hue, saturation, value);
        }
    }
}
