using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonItem : MonoBehaviour
{
    private int index;


    public void SetButton(int i)
    {
        index = i;
        transform.GetComponent<Button>().onClick.AddListener(() => OnButtonClicked());
    }

    private void OnButtonClicked()
    {
        MenuManager.instance.PressedItemButton(index,this);
    }

    public void EnableSelectedUI() {
        transform.GetChild(1).gameObject.SetActive(true);
    }

    public void DisableSelectedUI()
    {
        transform.GetChild(1).gameObject.SetActive(false);

    }


}
