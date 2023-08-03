using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonItem : MonoBehaviour
{
    private int index;
    bool isMenu = false;

    public void SetButton(int i,bool isMenu)
    {
        this.isMenu = isMenu;
           index = i;
        transform.GetComponent<Button>().onClick.AddListener(() => OnButtonClicked());
    }

    private void OnButtonClicked()
    {
        if (isMenu)
        {
            MenuManager.instance.PressedItemButton(index, this);

        }
        else {
            ARManager.instance.PressedItemButton(index, this);
        }
    }

    public void EnableSelectedUI() {
        transform.GetChild(1).gameObject.SetActive(true);
    }

    public void DisableSelectedUI()
    {
        transform.GetChild(1).gameObject.SetActive(false);

    }


}
