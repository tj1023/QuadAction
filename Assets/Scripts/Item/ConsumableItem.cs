using UnityEngine;

public class ConsumableItem : MonoBehaviour, ICollectible
{
    private enum ConsumableType
    {
        Ammo,
        Coin,
        Heart
    }

    [Header("Item Settings")]
    [SerializeField] private ConsumableType type;
    [SerializeField] private int value;

    private void Update()
    {
        transform.Rotate(Vector3.up * (30 * Time.deltaTime));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collect(other.gameObject);
        }
    }

    public void Collect(GameObject collector)
    {
        switch (type)
        {
            case ConsumableType.Ammo:
                break;
            case ConsumableType.Coin:
                break;
            case ConsumableType.Heart:
                break;
        }
        
        Destroy(gameObject);
    }
}