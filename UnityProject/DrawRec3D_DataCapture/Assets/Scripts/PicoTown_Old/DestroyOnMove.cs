using UnityEngine;

public class DestroyOnMove : MonoBehaviour
{
    Vector3 initial_pos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initial_pos = gameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.transform.position != initial_pos)
        {
            gameObject.SetActive(false);
        }
    }
}
