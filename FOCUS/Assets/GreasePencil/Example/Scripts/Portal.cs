using UnityEngine;
using System.Collections;

public class Portal : MonoBehaviour
{
    [SerializeField]
    bool _destructive;
    public bool Destructive { get { return _destructive; } }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var ball = collision.GetComponent<Ball>();
        if (collision.GetComponent<Ball>())
        {
            ball.Remove(this);
        }
    }
}
