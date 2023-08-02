using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager _instance { set; get; }


    [SerializeField] CanvasGroup splashMenu;
    [SerializeField] CanvasGroup selectMenu;


    public GameObject TilesCard;




    [SerializeField] Transform ItemsParent;
    [SerializeField] TilesContainers tilesContainers;


    private int selectedTextureIndex = 0;


    [SerializeField] GameObject ViewARButton;

    public static MenuManager instance
    {
        get
        {
            // If the instance is null, try to find it in the scene
            if (_instance == null)
            {
                _instance = FindObjectOfType<MenuManager>();

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
        splashMenu.alpha = 1;
        selectMenu.alpha = 0;
        StartCoroutine(TransitionToSelectTexture(2));
    }

    
    private void Start()
    {
        SpawnTilesItemsUI();
    }

    IEnumerator TransitionToSelectTexture(float delay) {
        yield return new WaitForSeconds(delay);

        float f = 0;
        while (f<1)
        {
            f += Time.deltaTime*2;
            splashMenu.alpha = 1-f;
            selectMenu.alpha = f;
            yield return new WaitForEndOfFrame();
        }

        splashMenu.gameObject.SetActive(false);
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

    public void Pressed_ViewAR()
    {

        LoadingManager.instance.LoadSceneAsync(1);
    }


}
