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

    private Coroutine activeHintCoroutine; // Sleduje běžící Coroutine
    private string currentDisplayedHint = ""; // Sleduje aktuálně zobrazenou nápovědu

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

    public void AddHint(Hint hint)
    {
        if (hint == null)
        {
            Debug.LogError("Předaný hint je null!");
            return;
        }

        // Ověříme, jestli už existuje nápověda se stejným textem
        foreach (GameObject existingHint in inventoryItems)
        {
            Hint existingHintComponent = existingHint.GetComponent<Hint>();
            if (existingHintComponent != null && existingHintComponent.hintText == hint.hintText)
            {
                Debug.Log($"Nápověda '{hint.hintText}' už existuje v inventáři.");
                return; // Nechceme přidávat stejný hint dvakrát
            }
        }

        // **Vytvoříme kopii hintu, aby se neztratil originální objekt ve scéně**
        GameObject newHintObject = Instantiate(hint.gameObject);
        Hint newHint = newHintObject.GetComponent<Hint>();

        // **Zajistíme, že kopie nemá fyziku a kolize, aby se chovala správně v inventáři**
        if (newHintObject.GetComponent<Rigidbody2D>())
        {
            Destroy(newHintObject.GetComponent<Rigidbody2D>());
        }
        if (newHintObject.GetComponent<Collider2D>())
        {
            Destroy(newHintObject.GetComponent<Collider2D>());
        }

        // **Přidáme kopii hintu do inventáře**
        inventoryItems.Add(newHintObject);
        hints.Add(newHint.hintText);

        // **Nastavíme unikátní jméno pro UI**
        string hintName = "Nápověda " + hints.Count;
        newHintObject.name = hintName;

        // **Aktualizujeme UI**
        UpdateInventoryUIForHint(newHint, hintName);

        Debug.Log($"Přidána nová nápověda: {newHint.hintText}");
    }





    private void UpdateInventoryUIForHint(Hint hint, string hintName)
    {
        if (slotHolder == null) return;

        GameObject newItemSlot = Instantiate(inventorySlotPrefab, slotHolder);
        Image itemIcon = newItemSlot.GetComponentInChildren<Image>();
        Text itemNameText = newItemSlot.GetComponentInChildren<Text>();

        if (itemIcon != null)
        {
            SpriteRenderer spriteRenderer = hint.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                itemIcon.sprite = spriteRenderer.sprite;
                itemIcon.color = spriteRenderer.color;
            }
            else
            {
                Debug.LogError("Hint nemá SpriteRenderer!");
                itemIcon.sprite = null;
                itemIcon.color = Color.clear;
            }
        }

        if (itemNameText != null)
        {
            itemNameText.text = hintName;
        }

        Button itemButton = newItemSlot.GetComponentInChildren<Button>();
        if (itemButton != null)
        {
            itemButton.onClick.AddListener(() => ViewHint(hint.hintText));
        }
    }







    public void ViewHint(string hintText)
    {
        // Vždy nastavíme nový text, i když je to stejná nápověda
        hintDisplayText.text = hintText;
        hintDisplayText.gameObject.SetActive(true);

        // Pokud už běží odpočet, zastavíme ho
        if (activeHintCoroutine != null)
        {
            StopCoroutine(activeHintCoroutine);
        }

        // Spustíme nový odpočet
        activeHintCoroutine = StartCoroutine(HideHintAfterDelay());
    }
    private IEnumerator HideHintAfterDelay()
    {
        yield return new WaitForSeconds(3f); // Počkej 3 sekundy
        hintDisplayText.gameObject.SetActive(false);
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
            itemIcon.color = itemSpriteRenderer.color;

            // Zajištění zachování poměru stran
            float spriteWidth = itemIcon.sprite.rect.width;
            float spriteHeight = itemIcon.sprite.rect.height;
            float aspectRatio = spriteWidth / spriteHeight;

            // Nastavení velikosti s ohledem na poměr stran
            float iconSize = 5f; // Nastav pevnou velikost pro nejdelší stranu
            if (aspectRatio > 1) // Širší než vyšší
            {
                itemIcon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, iconSize);
                itemIcon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, iconSize / aspectRatio);
            }
            else // Vyšší než širší nebo čtvercové
            {
                itemIcon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, iconSize);
                itemIcon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, iconSize * aspectRatio);
            }
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

            Hint hintComponent = inventoryItems[i].GetComponent<Hint>();
            if (hintComponent != null && inventoryItems[i].name == itemName)
            {
                Debug.Log($"Odstraňuji nápovědu: {itemName}");
                hints.Remove(hintComponent.hintText);
                Destroy(inventoryItems[i]);
                inventoryItems.RemoveAt(i);
                break;
            }

            Item itemComponent = inventoryItems[i].GetComponent<Item>();
            if (itemComponent != null && itemComponent.itemName == itemName)
            {
                Debug.Log($"Odstraňuji předmět: {itemName}");
                Destroy(inventoryItems[i]);
                inventoryItems.RemoveAt(i);
                break;
            }
        }

        // Po odstranění aktualizujeme UI
        UpdateInventoryUIAfterRemoval();
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
            Destroy(child.gameObject);
        }

        foreach (GameObject item in inventoryItems)
        {
            if (item != null)
            {
                Hint hint = item.GetComponent<Hint>();
                if (hint != null)
                {
                    UpdateInventoryUIForHint(hint, item.name);
                }
                else
                {
                    UpdateInventoryUI(item);
                }
            }
        }
    }





}