using UnityEngine;
using UnityEngine.SceneManagement;

public class Coin : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public KeyCode deleteKey = KeyCode.E;

    void Update()
    {
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);

        if (Input.GetKeyDown(deleteKey))
        {
            Destroy(gameObject);
            Debug.Log("������ �����Ǿ����ϴ�.");
            ScoreManager.Instance.AddScore(1);
        }
    }
}