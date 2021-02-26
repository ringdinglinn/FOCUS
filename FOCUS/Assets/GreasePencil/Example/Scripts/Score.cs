using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    [SerializeField]
    Text _score;

    void OnEnable()
    {
        Ball.AddScoreListener(OnScoreChanged);
    }

    void OnDisable()
    {
        Ball.RemoveScoreListener(OnScoreChanged);
    }

    // Update is called once per frame
    void OnScoreChanged(string score)
    {
        _score.text = score;
    }

    IEnumerator ScoreCoroutine()
    {
        var phase = 1f;
        var startScale = transform.localScale;

        while (phase > 0f)
        {
            yield return null;
            phase -= Time.deltaTime * 5f;
            _score.transform.localScale = Vector3.Lerp(Vector3.zero, startScale, phase * phase);
        }
    }
}
