using UnityEngine;

// Gentle scale pulse so cassette pickups read as "grab me".
public class Pulse : MonoBehaviour
{
    Vector3 baseScale;
    float t;

    void Awake() => baseScale = transform.localScale;

    void Update()
    {
        t += Time.deltaTime * 5f;
        transform.localScale = baseScale * (1f + 0.08f * Mathf.Sin(t));
    }
}
