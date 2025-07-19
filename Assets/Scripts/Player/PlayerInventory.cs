using System.Collections.Generic;
using UnityEngine;

public class ItemInfo
{
    public InventoryItem item;
    public int count;

    public ItemInfo(InventoryItem pItem, int pCount) 
    {
        item = pItem;
        count = pCount;
    }
}

public class PlayerInventory : MonoBehaviour
{
    public const int maxItems = 14;
    private bool activeGUI = false;

    private GameObject itemsUI;
    private List<ItemInfo>[] items;
    public int[,] itemNum = new int[maxItems, 2];

    [Header("PickCam Configuration")]
    public PickUICam pickUICam;

    private void Awake()  // Use Awake para garantir que a inicialização ocorra antes de Start de outros scripts
    {
        // Inicializar o array items com o tamanho de 3 (para os 3 tipos de itens)
        items = new List<ItemInfo>[3];

        // Inicializar as listas em cada índice do array items
        for (int i = 0; i < items.Length; i++)
        {
            items[i] = new List<ItemInfo>();
        }
    }

    private void Start()
    {
        // Exemplo de inicialização de itens do tipo menu
        InventoryItem[] menuItems = gameObject.GetComponents<InventoryItem>();
        for (int i = 0; i < menuItems.Length; i++)
        {
            items[2].Add(new ItemInfo(menuItems[i], 1));
        }
    }

    public void AddFirstItem(InventoryItem item)
    {
        if (item == null) 
        {
            Debug.LogError("Item é nulo ao tentar adicionar a AddFirstItem.");
            return;
        }

        int index = item.type;
        if (IsValidIndex(index))
        {
            for (int i = 0; i < items[index].Count; i++)
            {
                if (items[index][i].item.itemName == item.itemName)
                {
                    items[index][i].count++;
                    return;
                }
            }
            items[index].Insert(0, new ItemInfo(item, 1));
        }
    }

    public void AddItem(InventoryItem item)
    {
        if (item == null) 
        {
            Debug.LogError("Item é nulo ao tentar adicionar a AddItem.");
            return;
        }

        int index = item.type;

        if (IsValidIndex(index))
        {
            for (int i = 0; i < items[index].Count; i++)
            {
                if (items[index][i].item.itemName == item.itemName)
                {
                    items[index][i].count++;
                    return;
                }
            }
            items[index].Add(new ItemInfo(item, 1));
        }
    }

    public void RemoveItem(int index, int type)
    {
        if (IsValidIndex(type) && index >= 0 && index < items[type].Count)
        {
            items[type].RemoveAt(index);
        }
    }

    public void RemoveItem(string name)
    {
        ItemInfo found = null;
        foreach (ItemInfo item in items[0])
        {
            if (item.item.itemName == name)
            {
                if (item.count > 1) item.count--;
                else found = item;
            }
        }
        items[0].Remove(found);
    }

    public List<ItemInfo>[] Items
    {
        get { return items; }
    }

    public List<ItemInfo> CombineItems()
    {
        List<ItemInfo> combineItems = new List<ItemInfo>();
        foreach (ItemInfo item in items[0])
        {
            if (item.item.GetType() == typeof(CombineItem))
            {
                combineItems.Add(item);
            }
        }
        return combineItems;
    }

    private bool IsValidIndex(int index)
    {
        if (index < 0 || index >= items.Length)
        {
            Debug.LogError("Índice inválido ao acessar items.");
            return false;
        }

        if (items[index] == null)
        {
            Debug.LogError("Lista de itens no índice " + index + " não foi inicializada.");
            return false;
        }

        return true;
    }

    private void ShowItemInPickCam(InventoryItem item)
    {
        if (pickUICam != null && item != null)
        {
            pickUICam.SetAndEnable(item.gameObject);
        }
    }
}