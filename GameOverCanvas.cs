using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // LISÄÄ TÄMÄ
using System.Collections;


public class GameOverUI : MonoBehaviour
{
    [Header("Game Over UI")]
    [Header("Värit")]
    public Color normaaliVari = Color.black;
    public Color korostusVari = Color.yellow;
    public TextMeshProUGUI backToMenuText; // MUUTA TÄMÄ
    public TextMeshProUGUI restartText; // MUUTA TÄMÄ

    public GameObject gameOverPanel;
    public Text finalScoreText;
    public Button backToMenuButton;
    public Button restartButton;

    public EspOhjain espOhjain;

    private GameManager gameManager; // Lisää tämä
    private TikkaHeitto tikkaHeitto;

    private Button[] buttons;
    private int selectedIndex = 0;
    private float lastMoveTime = 0f;

    void Start()
    {


         buttons = new Button[] { backToMenuButton, restartButton };

         if (espOhjain == null)
            espOhjain = FindAnyObjectByType<EspOhjain>();
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(BackToMainMenu);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);



        // Etsi GameManager
        gameManager = FindAnyObjectByType<GameManager>();
            
        tikkaHeitto = FindAnyObjectByType<TikkaHeitto>();
        
        // Piilota panel aluksi
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Aseta nappien kuuntelijat
        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(BackToMainMenu);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
    }

        void Update()
    {
        // LISÄÄ: Vain kun panel on aktiivinen
        if (gameOverPanel != null && gameOverPanel.activeSelf)
        {
            // ESP-kontrollit
            if (espOhjain != null)
            {
                HandleESPInput();
            }
        }
    }
    
    void HandleESPInput()
    {
        float vertical = (espOhjain.espData.YlosArvo - espOhjain.espData.AlasArvo);
        
        if (Time.time - lastMoveTime > 0.3f)
        {
            if (vertical > 0.5f)
            {
                selectedIndex = (selectedIndex - 1 + buttons.Length) % buttons.Length;
                HighlightButton(selectedIndex); // TÄMÄ METODI PITÄÄ OLLA
                lastMoveTime = Time.time;
            }
            else if (vertical < -0.5f)
            {
                selectedIndex = (selectedIndex + 1) % buttons.Length;
                HighlightButton(selectedIndex); // TÄMÄ METODI PITÄÄ OLLA
                lastMoveTime = Time.time;
            }

            
        }
        
        
        if (espOhjain.espData.NapinArvo == 1)
        {
            if (selectedIndex == 0)
                BackToMainMenu();
            else if (selectedIndex == 1)
                RestartGame();
        }

        if (espOhjain.espData.NapinArvo == 1)
    {
        // Estä uudelleenpainaminen 1 sekunniksi
        StartCoroutine(ButtonPressCooldown());
    }

    }

    IEnumerator ButtonPressCooldown()
{
    // Poista napit käytöstä heti
    backToMenuButton.interactable = false;
    restartButton.interactable = false;
    
    // Odota 1 sekunti
    yield return new WaitForSeconds(1f);
    
    // Toimi vasta sitten
    if (selectedIndex == 0)
        BackToMainMenu();
    else if (selectedIndex == 1)
        RestartGame();
}


    void HighlightButton(int index)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                if (i == index)
                {
                    buttons[i].transform.localScale = Vector3.one * 1.1f;
                    // Vaihda tekstin väri
                    if (backToMenuText != null && restartText != null)
                    {
                        backToMenuText.color = (i == 0) ? korostusVari : normaaliVari;
                        restartText.color = (i == 1) ? korostusVari : normaaliVari;
                    }
                }
                else
                {
                    buttons[i].transform.localScale = Vector3.one;
                }
            }
        }
    }

    


 public void ShowGameOver(int totalScore)
    {
        // Hae pisteet suoraan TikkaHeittosta
        if (finalScoreText != null)
    {
        finalScoreText.text = $"Heitit pisteitä: {totalScore}";
        Debug.Log($"Teksti asetettu: '{finalScoreText.text}'");
    }
    
    if (gameOverPanel != null)
    {
        gameOverPanel.SetActive(true);

        selectedIndex = 0;
        HighlightButton(selectedIndex); 
       
    }
    }

   public void LoadMainMenu()
    {
        // Lataa scene 1 sekunnin päästä
        Invoke(nameof(BackToMainMenu), 3f);
    }
   
   
    void BackToMainMenu()
    {
           
        SceneManager.LoadScene("MainMenu");
    }


    


    void RestartGame()
    {
         TikkaHeitto.heittojaTehty = 0;
         TikkaHeitto.erassaHeitetty = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}