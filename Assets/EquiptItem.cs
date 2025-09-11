using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

public class EquiptItem : MonoBehaviour
{
   public InputReader input;

    public float radius = 2f; //Ž�� �ݰ�
    public LayerMask interactMask; //Ž���� ���̾� ����ũ
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
            // �ʿ��ϴٸ� IInteractable üũ �� �߰� ����
            Destroy(h.gameObject);
            Debug.Log($"{h.gameObject.name} �������� �ı��Ǿ����ϴ�.");
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
