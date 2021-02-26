using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Ball : MonoBehaviour
{
    static List<Ball> _allBalls = new List<Ball>();
    public static List<Ball> AllBalls { get { return _allBalls; } }
    static int _score;

    public static void AddScoreListener(UnityAction<string> onScoreChanged)
    {
        onScoreChanged.Invoke(_score.ToString());
        onScore += onScoreChanged;
    }

    public static void RemoveScoreListener(UnityAction<string> onScoreChanged)
    {
        onScore -= onScoreChanged;
    }

    public static event UnityAction<string> onScore;

    void Start()
    {
        _score = 0;
        onScore.Invoke(_score.ToString());
    }

    private void OnEnable()
    {
        if (!_allBalls.Contains(this))
            _allBalls.Add(this);
    }

    private void OnDisable()
    {
        _allBalls.Remove(this);
    }

    public void Remove(Portal portal)
    {
        StartCoroutine(RemoveRoutine(portal));
    }

    private IEnumerator RemoveRoutine(Portal portal)
    {
        var phase = 1f;
        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.bodyType = RigidbodyType2D.Kinematic;

        var start = transform.position;
        var startScale = transform.localScale;
        var endScale = portal.Destructive ? (Vector3.one * 1.5f) : Vector3.zero;

        while (phase > 0f)
        {
            yield return null;
            phase -= Time.deltaTime * 5f;

            if (!portal.Destructive)
                transform.position = Vector3.Lerp(portal.transform.position, start, phase);

            transform.localScale = Vector3.Lerp(endScale, startScale, phase * phase);
        }

        if (!portal.Destructive)
        {
            ++_score;
            onScore.Invoke(_score.ToString());
        }

        Destroy(gameObject);
    }
}
