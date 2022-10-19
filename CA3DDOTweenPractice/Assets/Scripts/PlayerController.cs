using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    [SerializeField] FloatingJoystick joyStick;
    [Header("Movement Speed")]
    [SerializeField] float moveSpeed;
    float Horizontal;
    float Vertical;
    Vector3 direction;
    Rigidbody rb;
    float currentTurnAngle;
    float smoothTurnTime = 0.1f;
    [Header("Detect & Collect")]
    Collider[] colliders;
    [SerializeField] Transform detectTransform;
    [SerializeField] float detectionRange = 1;
    [SerializeField] LayerMask layer;
    [SerializeField] Transform holdTransform;
    [SerializeField] Transform storeTransform;
    [SerializeField] int itemCount = 0;
    [SerializeField] int itemStoreCount = 0;
    [SerializeField] float itemDistanceBetween = 0.5f;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnDrawGizmos() {
        Gizmos.color = new Color(0.7f,0.0f,0.0f,0.7f);
        Gizmos.DrawSphere(detectTransform.position, detectionRange);
    }

    void Update()
    {
        colliders = Physics.OverlapSphere(detectTransform.position, detectionRange, layer);
        foreach (var hit in colliders)
        {
            if (hit.CompareTag("Collectable")) {
                hit.tag = "Collected";
                hit.transform.parent = holdTransform;

                var seq = DOTween.Sequence();

                seq.Append(hit.transform.DOLocalJump(new Vector3(0, itemCount * itemDistanceBetween), 2, 1, 0.3f)
                .Join(hit.transform.DOScale(1.25f, 0.1f))
                .Insert(0.1f, hit.transform.DOScale(0.3f, 0.2f)));
                seq.AppendCallback(() => {
                    hit.transform.localRotation = Quaternion.Euler(0,0,0);
                });

                itemCount++;
            }

            if (hit.CompareTag("Storage") && itemCount > 0) {

                foreach (Transform item in holdTransform)
                {
                    item.tag = "Stored";
                    item.transform.parent = storeTransform;

                    var seq = DOTween.Sequence();

                    seq.Append(item.transform.DOLocalJump(new Vector3(0, itemStoreCount, 0),2, 1, 0.3f)
                    .Join(item.transform.DOScale(1.5f, 0.1f))
                    .Insert(0.1f, item.transform.DOScale(1, 0.2f)));
                    seq.AppendCallback(() => {
                        item.transform.localRotation = Quaternion.Euler(0,0,0);
                    });
                    itemStoreCount++;
                    itemCount--;
                }
            }
        }
    }

    private void FixedUpdate() {
        Horizontal = joyStick.Horizontal;
        Vertical = joyStick.Vertical;

        direction = new Vector3(Horizontal, 0, Vertical);
        if (direction.magnitude > 0.01f) {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float turnAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref currentTurnAngle, smoothTurnTime);

            transform.rotation = Quaternion.Euler(0, turnAngle, 0);

            rb.MovePosition(transform.position + (direction * moveSpeed * Time.deltaTime));
        }
    }
}
