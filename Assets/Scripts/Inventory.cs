using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public GameObject inventoryUI; // Odkaz na UI Panel v Canvasu
    public Transform slotHolder; // Odkaz na rodiče slotů v inventáři
    public GameObject inventorySlotPrefab; // Prefab slotu v inventáři

    public Text hintDisplayText; // Textové pole v panelu pro zobrazení textu nápovědy

    private List<GameObject> inventoryItems = new List<GameObject>();

    private List<string> hints = new List<string>();

    private void Start()
    {
        if (inventoryUI == null)
        {
            Debug.LogError("Chybí odkaz na inventářový UI panel!");
        }
        if (slotHolder == null)
        {
            Debug.LogError("Chybí odkaz na rodiče slotů v inventáři!");
        }
        if (hintDisplayText == null)
        {
            Debug.LogError("Chybí odkaz na Text komponentu pro zobrazení nápovědy!");
        }
        inventoryUI.SetActive(false);
    }

    public void ToggleInventoryUI()
    {
        if (inventoryUI == null)
        {
            Debug.LogError("Inventářový UI panel není přiřazen!");
            return;
        }

        // Přepne viditelnost UI panelu
        inventoryUI.SetActive(!inventoryUI.activeSelf);
    }

    public void AddHint(string hintText, Color hintColor)
    {
        // Vytvoření virtuálního objektu pro hint
        GameObject hintObject = new GameObject($"Hint: {hintText}");
        Hint hintComponent = hintObject.AddComponent<Hint>();
        hintComponent.SetHintText(hintText);

        // Nastavení barvy (není nutné, pokud není vizuální zobrazení)
        SpriteRenderer spriteRenderer = hintObject.AddComponent<SpriteRenderer>();
        spriteRenderer.color = hintColor;

        // Přidání do seznamu inventoryItems
        inventoryItems.Add(hintObject);
        hints.Add(hintText);
        UpdateInventoryUIForHint(hintText, hintColor);
    }


    private void UpdateInventoryUIForHint(string hintText, Color hintColor)
    {
        if (slotHolder == null) return;

        // Vytvoř nový slot pro nápovědu
        GameObject newItemSlot = Instantiate(inventorySlotPrefab, slotHolder);
        Image itemIcon = newItemSlot.GetComponentInChildren<Image>();
        Text itemNameText = newItemSlot.GetComponentInChildren<Text>();

        // Nastav barvu ikony
        if (itemIcon != null)
        {
            itemIcon.color = hintColor;
        }

        // Nastav název nápovědy
        if (itemNameText != null)
        {
            itemNameText.text = "Nápověda"; // Název nápovědy ve slotu
        }

        Button itemButton = newItemSlot.GetComponentInChildren<Button>();
        if (itemButton != null)
        {
            itemButton.onClick.AddListener(() => ViewHint(hintText));
        }
    }
    public void ViewHint(string hintText)
    {
        if (!string.IsNullOrEmpty(hintText))
        {
            StartCoroutine(DisplayHint(hintText));
        }
        else
        {
            Debug.LogWarning("Nápověda je prázdná nebo null.");
        }
    }
    private IEnumerator DisplayHint(string hintText)
    {
        // Zobrazí text nápovědy
        hintDisplayText.text = hintText;
        hintDisplayText.gameObject.SetActive(true);

        // Počká 3 sekundy
        yield return new WaitForSeconds(3f);

        // Skryje text nápovědy
        hintDisplayText.gameObject.SetActive(false);
    }

    public void AddItem(GameObject item)
    {
        if (item == null)
        {
            Debug.LogError("Pokus o přidání null objektu do inventáře.");
            return;
        }

        if (!item)
        {
            Debug.LogError("Předmět byl zničen před přidáním do inventáře!");
            return;
        }

        inventoryItems.Add(item);
        Debug.Log($"Přidávám předmět do inventáře: '{item.GetComponent<Item>()?.itemName}'");
        UpdateInventoryUI(item);
    }
    private void UpdateInventoryUI(GameObject item)
    {
        if (slotHolder == null) return;

        GameObject newItemSlot = Instantiate(inventorySlotPrefab, slotHolder);
        Image itemIcon = newItemSlot.GetComponentInChildren<Image>();
        Text itemNameText = newItemSlot.GetComponentInChildren<Text>();
        SpriteRenderer itemSpriteRenderer = item.GetComponent<SpriteRenderer>();
        Item itemComponent = item.GetComponent<Item>();

        if (itemIcon != null && itemSpriteRenderer != null)
        {
            itemIcon.sprite = itemSpriteRenderer.sprite;
            itemIcon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 50);
            itemIcon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50);
            itemIcon.color = itemSpriteRenderer.color; // Zkopíruj barvu ze SpriteRenderer do ikony
        }
        else
        {
            Debug.LogError("Nelze nastavit ikonu předmětu, chybí komponenty!");
            if (itemIcon != null)
            {
                // Nastavení výchozí ikony nebo prázdné ikony, pokud komponenty chybí
                itemIcon.sprite = null;
                itemIcon.color = Color.clear;
            }
        }

        if (itemNameText != null && itemComponent != null)
        {
            itemNameText.text = itemComponent.itemName; // Nastaví název předmětu
        }
        else
        {
            Debug.LogError("Nelze nastavit název předmětu, chybí komponenty!");
        }
    }
    public void RemoveItemByName(string itemName)
    {
        Debug.Log($"Mazání předmětu z inventáře: Hledám '{itemName}'");

        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i] == null)
            {
                inventoryItems.RemoveAt(i);
                i--;
                continue;
            }

            // Rozlišení mezi hinty a běžnými předměty
            Hint hintComponent = inventoryItems[i].GetComponent<Hint>();
            if (hintComponent != null)
            {
                Debug.Log($"Položka '{itemName}' je hint. Přeskakuji.");
                continue;
            }

            Item itemComponent = inventoryItems[i].GetComponent<Item>();
            if (itemComponent != null && itemComponent.itemName == itemName)
            {
                Debug.Log($"Odstraňuji předmět: {itemName}");
                Destroy(inventoryItems[i]);
                inventoryItems.RemoveAt(i);
                UpdateInventoryUIAfterRemoval();
                return;
            }
        }

        Debug.LogWarning($"Předmět '{itemName}' nebyl nalezen v inventáři.");
    }
    public bool HasItem(string itemName)
    {
        Debug.Log($"Kontrola předmětu v inventáři: Hledám '{itemName}'");

        foreach (GameObject item in inventoryItems)
        {
            if (item == null)
            {
                Debug.Log("Nalezen null objekt v inventáři, přeskočeno.");
                continue;
            }

            Item itemComponent = item.GetComponent<Item>();
            if (itemComponent != null && itemComponent.itemName == itemName)
            {
                Debug.Log($"Předmět '{itemName}' nalezen v inventáři.");
                return true;
            }
        }

        Debug.Log($"Předmět '{itemName}' není v inventáři.");
        return false;
    }
    private void UpdateInventoryUIAfterRemoval()
    {
        foreach (Transform child in slotHolder)
        {
            Destroy(child.gameObject); // Odstraníme všechny staré sloty
        }

        // Znovu přidáme všechny položky
        foreach (GameObject item in inventoryItems)
        {
            if (item.GetComponent<Hint>() != null)
            {
                // Pokud je to hint, aktualizujeme jako hint
                Hint hint = item.GetComponent<Hint>();
                UpdateInventoryUIForHint(hint.GetHintText(), hint.GetHintColor());
            }
            else
            {
                UpdateInventoryUI(item); // Běžné předměty
            }
        }
    }


}