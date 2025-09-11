using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

public class EquiptItem : MonoBehaviour
{
   public InputReader input;

    public float radius = 2f; //탐지 반경
    public LayerMask interactMask; //탐지할 레이어 마스크
    public Transform center;

    void OnEnable()
    {
        if (input != null)
            input.InteractPressed += OnInteractPressed;
    }

    void OnDisable()
    {
        if (input != null)
            input.InteractPressed -= OnInteractPressed;
    }

    void OnInteractPressed()
    {
        Vector3 cpos = center.position;
        Collider[] hits = Physics.OverlapSphere(cpos, radius, interactMask, QueryTriggerInteraction.Collide);

        foreach (var h in hits)
        {
            // 필요하다면 IInteractable 체크 등 추가 가능
            Destroy(h.gameObject);
            Debug.Log($"{h.gameObject.name} 아이템이 파괴되었습니다.");
        }
    }

    private void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        var at = center ? center.position : transform.position;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(at, radius);   
    }
}
