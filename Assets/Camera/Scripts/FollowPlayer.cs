using System.Collections;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 offset;
    private bool _moving = false;
    private void FixedUpdate()
    {
        if (_moving) return;
        
        if (player.position.y < -offset.y || player.position.y >= 10f) return;

        Vector3 pos = transform.position;
        pos.y = player.position.y + offset.y;
        Vector3 startPos = transform.position;
        _moving = true;
        StartCoroutine(MoveCamera(startPos, pos, 0.01f));
    }
    
    // Coroutine to lerp the player to the new position
    IEnumerator MoveCamera(Vector3 startPos, Vector3 endPos, float time)
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / time;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        _moving = false;
    }
}
