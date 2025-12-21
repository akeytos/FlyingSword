using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenuManager : MonoBehaviour
{

    [Header("Panel Referanslarý")]
    public GameObject selectionPanel; 
    public GameObject mainButtonsPanel; 

    public void Click_Play()
    {


        LoadingManager.nextSceneName = "levelBlockout + UI";

            SceneManager.LoadScene("Loading");
    }

    public void Click_Choose()
    {
        
        mainButtonsPanel.SetActive(false);

        
        selectionPanel.SetActive(true);
    }
    public void Click_Quit()
    {
        Debug.Log("Oyundan çýkýldý");
        Application.Quit();
    }

    public void Click_Inventory()
    {
        Debug.Log("Envanter açýlacak...");
     
    }

    public void Click_Shop()
    {
        Debug.Log("Dükkan açýlacak...");

    }
    public void Click_Settings()
    {
        Debug.Log("Ayarlar paneli açýlýyor...");
        
    }

    public void Click_Language()
    {
        Debug.Log("Dil seçimi deðiþtirilecek...");
    
    }
    public void Click_Discord()
    {
        Application.OpenURL("https://discord.gg/swordslide");
    }

    public void Click_Steam()
    {
        Application.OpenURL("https://store.steampowered.com/app/swordslide");
    }

    public void Click_BackFromSelection()
    {
        selectionPanel.SetActive(false);

        mainButtonsPanel.SetActive(true);
    }
}