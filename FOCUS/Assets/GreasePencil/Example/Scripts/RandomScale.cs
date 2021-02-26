using UnityEngine;

public class RandomScale : MonoBehaviour
{
    [SerializeField] float _speed = 3f;
    [SerializeField] Vector3 _minScale = Vector3.one;
    [SerializeField] Vector3 _maxScale = Vector3.one;
    [SerializeField] bool _useUnscaledTime;

    float _seed;

    private void Start()
    {
        _seed = Random.value * 500f;
    }

    void Update()
    {
        float time = (_useUnscaledTime ? Time.unscaledTime : Time.time) * _speed;

        transform.localScale = new Vector3(
            Mathf.Lerp(_minScale.x, _maxScale.x, Mathf.PerlinNoise(time, _seed)),
            Mathf.Lerp(_minScale.y, _maxScale.y, Mathf.PerlinNoise(time, _seed+0.2f)),
            Mathf.Lerp(_minScale.z, _maxScale.z, Mathf.PerlinNoise(time, _seed+0.3f))
        );
    }
}
