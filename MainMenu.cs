// MainMenuManager.cs - TARKISTA TÄMÄ
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public Button playButton;
    public Button quitButton;

    public EspOhjain espOhjain;

     private Button[] buttons;
    private int selectedIndex = 0;
    private float lastMoveTime = 0f;

      private float menuLoadTime;


    void Start()
    {
       
       menuLoadTime = Time.time;
        
        // Estä napit 1 sekunniksi
        playButton.interactable = false;
        quitButton.interactable = false;
        
        Invoke("EnableButtons", 3f);
      
      
       buttons = new Button[] { playButton, quitButton };
       
        // Varmista että napit löytyvät
        if (playButton == null)
        {
            Debug.LogError("PlayButton ei ole asetettu inspectorissa!");
            // Etsi nappi automaattisesti
            playButton = GameObject.Find("PlayButton")?.GetComponent<Button>();
        }
        
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners(); // Puhdasta vanhat kuuntelijat
            playButton.onClick.AddListener(PlayGame);
            Debug.Log("PlayButton kuuntelija asetettu");
        }
        
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);


        if (espOhjain == null)
            espOhjain = FindAnyObjectByType<EspOhjain>();

            HighlightButton(selectedIndex);
    }


         void EnableButtons()
    {
        playButton.interactable = true;
        quitButton.interactable = true;
    }


    void PlayGame()
    {
        Debug.Log("PlayGame nappia painettu!");

         TikkaHeitto.heittojaTehty = 0;
         TikkaHeitto.erassaHeitetty = 0;
        
        // Varmista että lataus tapahtuu
        SceneManager.LoadScene("PeliKentta"); // VAIHDA TÄMÄ PELISKENEN NIMEEN
        
        // Tai jos et tiedä nimeä, käytä build indexiä
        // SceneManager.LoadScene(1); // Jos GameScene on indeksissä 1
    }

     void Update()
    {
       
        if (Time.time - menuLoadTime < 1f)
            return;
       
        // LISÄÄ TÄMÄ: ESP-kontrollit
       
       
        if (espOhjain != null)
        {
            HandleESPInput();
        }
    }

       void HandleESPInput() // LISÄÄ TÄMÄ METODI
    {
        // Joystick liike
        float vertical = (espOhjain.espData.YlosArvo - espOhjain.espData.AlasArvo);
        
        if (Time.time - lastMoveTime > 0.3f)
        {
            if (vertical > 0.5f) // Ylös
            {
                selectedIndex = (selectedIndex - 1 + buttons.Length) % buttons.Length;
                HighlightButton(selectedIndex);
                lastMoveTime = Time.time;
            }
            else if (vertical < -0.5f) // Alas
            {
                selectedIndex = (selectedIndex + 1) % buttons.Length;
                HighlightButton(selectedIndex);
                lastMoveTime = Time.time;
            }

             if (espOhjain.espData.NapinArvo == 1)
        {
            if (selectedIndex == 0)
                PlayGame();
            else if (selectedIndex == 1)
                QuitGame();
        }
    }
        }

          void HighlightButton(int index) // LISÄÄ TÄMÄ METODI
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                if (i == index)
                    buttons[i].transform.localScale = Vector3.one * 1.1f;
                else
                    buttons[i].transform.localScale = Vector3.one;
            }
        }
    }

    void QuitGame()
    {
        Application.Quit();
    }
}