//using UnityEngine;
//using UnityEngine.UI;

//public class InventoryUI : MonoBehaviour
//{
//    public Transform itemsParent;
//    public GameObject inventoryUI;
//    private Inventory inventory;

//    void Start()
//    {
//        if (inventoryUI != null) inventoryUI.SetActive(false); // Skryje inventář na začátku
//        inventory = FindObjectOfType<Inventory>();
//        if (inventory == null)
//        {
//            Debug.LogError("Nenalezen žádný inventář!");
//        }
//    }

//    void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.I))
//        {
//            inventoryUI.SetActive(!inventoryUI.activeSelf);
//            Debug.Log("Inventář byl " + (inventoryUI.activeSelf ? "otevřen" : "zavřen"));
//        }
//        inventoryUI.SetActive(!inventoryUI.activeSelf);
//    }
//}