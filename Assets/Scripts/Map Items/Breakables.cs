using TreeEditor;
using UnityEngine;

public class Breakables : MonoBehaviour
{
    public bool shouldDropItem;
    public GameObject[] itemsToDrop;
    public float itemDropPercent;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void DropItem()
    {
        Destroy(gameObject);

        AudioManager.instance.PlaySFX(0);

        if (shouldDropItem)
        {
            float dropChance = Random.Range(0f, 100f);

            if (dropChance < itemDropPercent)
            {
                int randomItem = Random.Range(0, itemsToDrop.Length);

                Instantiate(itemsToDrop[randomItem], transform.position, transform.rotation);
            }
        }
    }
}
