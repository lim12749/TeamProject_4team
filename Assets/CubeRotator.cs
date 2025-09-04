using UnityEngine;

public class CubeRotator : MonoBehaviour
{
    // 회전 속도를 설정할 수 있도록 public 변수로 선언
    public Vector3 rotationSpeed = new Vector3(0, 100, 0);

    void Update()
    {
        // 매 프레임마다 회전 (Time.deltaTime을 곱해서 프레임 독립적인 회전)
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}