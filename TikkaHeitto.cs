using UnityEngine;
using System.Collections;
using UnityEngine.UI; 

public class TikkaHeitto : MonoBehaviour
{
   [Header("Heittoasetukset")]
 //   public float heittoVoima = 20f;
    public float pyorimisNopeus = 720f;
    public GameObject Tikka;

    


public float oletusAloitusEtaisyys = 4f;
public float pullThreshold = 0.08f;       // pienempi arvo herkkyyteen
public float releaseThreshold = 0.25f;    // kuinka paljon pitää laskea huipusta
public int requiredPullFrames = 3;          
public float returnTolerance = 0.1f;       // debounce vedolle

bool hasInitEta = false;
bool isPulled = false;
float vanhaEtaisyys = 0f;
float peakEtaisyys = 0f;                  // tallentaa vedon huipun
int pullFrameCount = 0;

public float minPullDistance = 0f; 
public int requiredReturnFrames = 3;

    
    
    [Header("ESP Heittovoima-asetukset")]
    public float minHeittoVoima = 10f;
    public float maxHeittoVoima = 30f;
    public float minEtaisyys = 0f;
    public float maxEtaisyys = 100f;

    //UI
    [Header("UI")]
    public Text currentScoreText;
    public Text totalScoreText;
    public Text dartsCounterText;
    private int currentPoints = 0; 
    public int totalPoints = 0;

    public Text finalScoreText;

    [Header("Game Over")]
    public GameOverUI gameOverUI;

    //UI

    public EspOhjain espOhjain;
    
    //private float vanhaEtaisyys;
    
    [Header("Voimakäyrä-asetukset")]
    public AnimationCurve voimaKayra = AnimationCurve.EaseInOut(0, 1, 1, 0.3f);

    [Header("Viittaukset")]
    public CrosshairMouseControl crosshair;
    public GameManager gameManager;
    public DartsScorer dartsScorer;

    [Header("Tikan asetukset")]
    public Vector3 aloitusSijainti = new Vector3(-12.17f, 2.7f, -13.39f);
    public Vector3 aloitusRotaatio = new Vector3(-84.626f, -75.741f, -14.379f);
    public Vector3 tikanKoko = new Vector3(1.489f, 1.489f, 1.489f);

    private Rigidbody rb;
    private bool onHeitetty = false;
    private bool onOsunut = false;

    public static int heittojaTehty = 0;   // kaikkien tikkaskriptien yhteinen laskuri - otettiin static pois TESTI
    public static int erassaHeitetty = 0;  // montako tikkaa kiinni taulussa - otettiin static pois TESTI
    public static int maxHeittoja = 6;     // yhteensä 6 heittoa

    [Header("Aiming Adjustment")]
    [Range(-2f, 5f)]
    public float horizontalAimAdjust = 0.1f;   // Vasen-oikea säätö
    
    [Range(-5f, 0.5f)]
    public float verticalAimAdjust = 0.15f;    // Ylös-alas säätö
    
    [Range(-0.3f, 0.3f)]
    public float baseHeightAdjust = 0.08f;     // Peruskorkeussäätö


    
      


    void Start()
    {
        if (Time.time < 1f) // TAI käytä flagia
        {
            heittojaTehty = 0;
            erassaHeitetty = 0;
        }
      
      
      
        transform.position = aloitusSijainti;
        transform.rotation = Quaternion.Euler(aloitusRotaatio);
        transform.localScale = tikanKoko;

        espOhjain = FindAnyObjectByType<EspOhjain>();


        rb = GetComponent<Rigidbody>();
        AlustaTikka();

          // Alustetaan käyrä oletusarvoilla jos se on null
        if (voimaKayra == null || voimaKayra.keys.Length == 0)
        {
            voimaKayra = AnimationCurve.EaseInOut(0, 1, 1, 0.3f);
        }

        
         if (espOhjain != null)
    {
        vanhaEtaisyys = espOhjain.espData.Etaisyys;
    }
    else
    {
        vanhaEtaisyys = oletusAloitusEtaisyys; // 4
    } 
     
     UpdateScoreDisplay(); //UI
     UpdateDartsCounter();
        
}

    void UpdateDartsCounter()
    {
        if (dartsCounterText != null)
        {
            dartsCounterText.text = $"Tikkoja heitetty: {heittojaTehty}/{maxHeittoja}";
        }
    }   
 
        void AlustaTikka()
        {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
            onHeitetty = false;
            onOsunut = false;
        }



    void Update()
    {
           
        
        if (espOhjain == null)
    {
        Debug.LogWarning("espOhjain puuttuu");
        return;
    }

   float nykyinenEtaisyys = espOhjain.espData.Etaisyys;

   
    // 1) Havaitse vetäminen: etäisyys kasvaa nopeasti
    if (!isPulled && (nykyinenEtaisyys - vanhaEtaisyys) > pullThreshold)
    {
        isPulled = true;
        // Debug.Log("Vipu vedetty");
    }

    // 2) Havaitse vapautus: jos oli vedetty ja etäisyys pienenee nopeasti -> heitä
    if (isPulled && (vanhaEtaisyys - nykyinenEtaisyys) > releaseThreshold && !onHeitetty)
    {
        float voima = LaskeHeittoVoimaEtaisyydenMukaan();
        HeitaTikka(voima);
        isPulled = false;
    }

    // Päivitä vanhaEtaisyys seuraavaa framea varten
    vanhaEtaisyys = nykyinenEtaisyys;

    // (Säilytä muu Update-logiikka tarvittaessa)

   
    
    if (!onHeitetty && crosshair != null)
    {
       
    }
             
    
         
       
    }

    void HeitaTikka(float voima)
    {
       if(onHeitetty) return;

        onHeitetty = true;
        rb.isKinematic = false;
        rb.useGravity = true;

        

        Vector3 heittoSuunta = crosshair.GetThrowDirection();
        heittoSuunta.y = -heittoSuunta.y; // korjaus
        heittoSuunta.x += -0.5f;

         heittoSuunta = AdjustAimForGravity(heittoSuunta);
        

        //rb.linearVelocity = heittoSuunta * heittoVoima;
        rb.linearVelocity = heittoSuunta * voima;
        rb.angularVelocity = transform.forward * Mathf.Deg2Rad * pyorimisNopeus;

    }

    Vector3 AdjustAimForGravity(Vector3 aimDirection)
    {
        // 1. Laske missä kohtaa ruutua crosshair on (0-1)
        Vector2 screenPos = crosshair.DrawPosition;
        float normalizedX = screenPos.x / Screen.width;  // 0=vasen, 1=oikea
        float normalizedY = screenPos.y / Screen.height; // 0=ala, 1=ylä
        
        // 2. Korjaa vaakasuuntaa hieman crosshairin mukaan
        float horizontalCorrection = (normalizedX - 0.5f) * horizontalAimAdjust;
        
        // 3. Korjaa pystysuuntaa ENEMMÄN:
        // - Mitä ylempänä ruudussa, sitä vähemmän korjataan ylöspäin
        // - Mitä alempana, sitä enemmän korjataan ylöspäin (painovoiman takia)
        float verticalCorrection = (0.5f - normalizedY) * verticalAimAdjust;
        
        // 4. Lisää peruskorkeuskorjaus (tikka lentää yleensä alas)
        verticalCorrection += baseHeightAdjust;
        
        // 5. Sovella korjaukset
        Vector3 correctedAim = new Vector3(
        aimDirection.x,                    // X pysyy samana (eteenpäin/taaksepäin)
        aimDirection.y + verticalCorrection, // Y korjaus (ylös/alas)
        aimDirection.z + horizontalCorrection
        );
        
        Debug.Log($"Aim adjust: H={horizontalCorrection:F3}, V={verticalCorrection:F3}");
        
        return correctedAim.normalized;
    }
    // Uusi metodi joka käyttää tallennettua etäisyyttä


    public float LaskeHeittoVoimaEtaisyydenMukaan()
    {
        if (espOhjain == null) return 0f;

        float etaisyys = espOhjain.espData.Etaisyys;
        
        // Rajataan etäisyys haluttuun väliin
        etaisyys = Mathf.Clamp(etaisyys, minEtaisyys, maxEtaisyys);
        
        // Lasketaan normalisoitu etäisyys (0-1 välille)
       float normalisoituEtaisyys = 1f - ((etaisyys - minEtaisyys) / (maxEtaisyys - minEtaisyys));
        
        // KÄYTTÖÖNOTTO: Käytetään AnimationCurvea saadaksesi epälineaarisen vaikutuksen
        float voimaKerroin = voimaKayra.Evaluate(normalisoituEtaisyys);
        
        // Sovelletaan kerrointa heittovoimaan
        float laskettuVoima = Mathf.Lerp(minHeittoVoima, maxHeittoVoima, voimaKerroin);
        
        Debug.Log($"Etäisyys: {etaisyys}, Normalisoitu: {normalisoituEtaisyys:F2}, " +
                  $"Voimakerroin: {voimaKerroin:F2}, Heittovoima: {laskettuVoima:F2}");
        
        return laskettuVoima;
    
    }


    void OnCollisionEnter(Collision collision)
    {
        if (onOsunut) return;

        if (collision.gameObject.CompareTag("Target"))
        {
            onOsunut = true;
            rb.isKinematic = true;
            rb.useGravity = false;
            transform.SetParent(collision.transform);

             heittojaTehty++;
             erassaHeitetty++;

              UpdateDartsCounter();

            //UI
        if (dartsScorer != null)
            {
            Vector3 osumaPiste = collision.contacts[0].point;
            int pisteet = dartsScorer.LaskePisteet(osumaPiste);
            currentPoints = pisteet;      // Vaihtuu aina uuteen
            totalPoints += pisteet;       // Kasvaa koko ajan
            UpdateScoreDisplay();
            } //UI
             
            if (dartsScorer != null)
            {
                Vector3 osumaPiste = collision.contacts[0].point;
                dartsScorer.LaskePisteet(osumaPiste);
            }

           
            

            if (heittojaTehty >= maxHeittoja)
            {
              
                /*GameOverUI ui = FindAnyObjectByType<GameOverUI>();
                if (ui != null) 
                    ui.ShowGameOver(totalPoints);*/
                    
                       //Invoke("NaytaGameOver", 3f);
               
                Debug.Log("[TikkaHeitto] Kaikki heitot tehty!");
                
                Invoke("ShowDelayedGameOver", 3f);
                
                return;
            }

            if (erassaHeitetty >= 3)
            {
                // Odota hetki ennen kuin poistetaan tikat
                Debug.Log("[TikkaHeitto] 3 tikkaa kiinni, odotetaan ennen poistamista...");
                Invoke("TyhjennaTauluJaJatka", 1.5f); // esim. 3 sekunnin viive
                return; // EI luoda uutta tikkaa heti
            }

            // Luo uusi tikka jos ei vielä 3 kiinni
            Invoke("LuoUusiTikka", 1f);

           
        }
    }

    void ShowDelayedGameOver()
{
    GameOverUI ui = FindAnyObjectByType<GameOverUI>();
    if (ui != null) 
    {
        ui.ShowGameOver(totalPoints);
    }
}

    void TyhjennaTauluJaJatka()
    {
        foreach (var tikka in GameObject.FindGameObjectsWithTag("Dart"))
        {
            Destroy(tikka);
        }
        erassaHeitetty = 0;

        // Luo uusi tikka seuraavaa heittoa varten
        UpdateDartsCounter();
        LuoUusiTikka();
    }

   void LuoUusiTikka()
{
    GameObject uusiTikka = Instantiate(gameObject, aloitusSijainti, Quaternion.Euler(aloitusRotaatio));
    uusiTikka.transform.localScale = tikanKoko;

    // Alusta uuden tikan skripti
    TikkaHeitto uusiSkripti = uusiTikka.GetComponent<TikkaHeitto>();
    if (uusiSkripti != null)
    {
        uusiSkripti.enabled = true;
        uusiSkripti.rb = uusiTikka.GetComponent<Rigidbody>();
        uusiSkripti.onHeitetty = false;
        uusiSkripti.onOsunut = false;

     

        //UI
        uusiSkripti.currentPoints = this.currentPoints;
        uusiSkripti.totalPoints = this.totalPoints;
        uusiSkripti.UpdateScoreDisplay();
        //UI
       
       
       
    }

}

public void Init()
{
    onHeitetty = false;
    onOsunut = false;
    isPulled = false;
    hasInitEta = false;
    vanhaEtaisyys = oletusAloitusEtaisyys;
    peakEtaisyys = oletusAloitusEtaisyys;
    pullFrameCount = 0;

    if (rb == null) rb = GetComponent<Rigidbody>();
    if (rb != null)
    {
        rb.isKinematic = true;
        rb.useGravity = false;
    }
}
//UI
void UpdateScoreDisplay()
{
    if (currentScoreText != null)
        currentScoreText.text = $"Heittopisteet: {currentPoints}";

    if (totalScoreText != null)
    totalScoreText.text = $"Kokonaispisteet: {totalPoints}";
}   
//UI



}


















