using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AppManager : MonoBehaviour
{
    public static AppManager _instance { set; get; }

    public GameObject TilesCard;


    [SerializeField] Transform ItemsParent;
    [SerializeField] TilesContainers tilesContainers;


    private int selectedTextureIndex = 0;


    [SerializeField] GameObject ViewARButton;

    public static AppManager instance
    {
        get
        {
            // If the instance is null, try to find it in the scene
            if (_instance == null)
            {
                _instance = FindObjectOfType<AppManager>();

                // If it's still null, create a new GameObject and attach the script to it
                //if (_instance == null)
                //{
                //    GameObject singletonObject = new GameObject("AppManager");
                //    _instance = singletonObject.AddComponent<AppManager>();
                //}
            }

            return _instance;
        }

    }

    private void Awake()
    {
        ViewARButton.SetActive(false);
    }

    
    private void Start()
    {
        SpawnTilesItemsUI();
    }

    void SpawnTilesItemsUI() {
        for (int i = 0; i < tilesContainers.tileTextures.Length; i++)
        {
            GameObject go = Instantiate(TilesCard, ItemsParent);
            go.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = tilesContainers.tileTextures[i];
            go.transform.GetComponent<ButtonItem>().SetButton(i);
        }
    }

    ButtonItem buttonItem;
    public void PressedItemButton(int i,ButtonItem _buttonItem) {
        selectedTextureIndex = i;
        print("Pressed " + i);

        if (buttonItem!=null)
        {
            buttonItem.DisableSelectedUI();

        }
        buttonItem = _buttonItem;
        buttonItem.EnableSelectedUI();
        ViewARButton.SetActive(true);

    }


}
