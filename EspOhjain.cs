using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using System.Net.WebSockets;
using System;
using System.Text;



public class EspOhjain : MonoBehaviour
{
    [SerializeField]
    string serverilleMenevaViesti = "Heippatirallaa";

    [SerializeField]
    int NapinArvo;

    [SerializeField]
    int Etaisyys;

    [SerializeField]
    int YlosArvo;

    [SerializeField]
    int AlasArvo;

    [SerializeField]
    int OikealleArvo;

    [SerializeField]
    int VasenArvo;


    public ESPDatat espData = new ESPDatat();

    CancellationTokenSource yhteysPoletti;
    // Start is called before the first frame update
    void Start()
    {
        TaskinKaynnistys();
    }

    void TaskinKaynnistys()
    {
        Debug.Log("Taskin käynnistys on käynnistetty");
        var sokettiYhteys = Task.Run(() => ESPYhteys());
    }

    async Task ESPYhteys()
    {
        Debug.Log("ESPYHteys käynnistetty");

        using( ClientWebSocket soketti = new ClientWebSocket())
        {
            Uri osoite = new Uri("ws://172.16.200.56:80"); //Muista vaihtaa omaan osoitteeseen
            yhteysPoletti = new CancellationTokenSource();
            // Kun kaikki toimii, laita seuraava rivi kommentteihin.
           //yhteysPoletti.CancelAfter(20000);
            await soketti.ConnectAsync(osoite, yhteysPoletti.Token);
            Debug.Log("Soketti yhteys ESP32 luotu");
            while( soketti.State == WebSocketState.Open && 
                    yhteysPoletti.IsCancellationRequested == false)
            {
                    // VIestin lähetys arduinon suuntaan
                    if( serverilleMenevaViesti.Length > 0 )
                    {
                        ArraySegment<byte> viesti = new ArraySegment<byte>(Encoding.UTF8.GetBytes(serverilleMenevaViesti));
                        await soketti.SendAsync(viesti, WebSocketMessageType.Text, true, yhteysPoletti.Token );
                        serverilleMenevaViesti = "";
                    }

                    // Viestin vastaanotto
                    var vastaanOtto = new byte[250];
                    int offset = 0;
                    int datayhdessapaketissa = 10;
                    while(yhteysPoletti.IsCancellationRequested == false)
                    {
                       ArraySegment<byte> vastaanotettavaviesti = new ArraySegment<byte>(
                            vastaanOtto, offset, datayhdessapaketissa ); 
                       WebSocketReceiveResult tulokset = 
                            await soketti.ReceiveAsync(vastaanotettavaviesti, yhteysPoletti.Token);
                        offset = offset + tulokset.Count; 
                        if( tulokset.EndOfMessage )
                            {
                            break;
                            }
                    }
                    string vastaanotettuviesti = Encoding.UTF8.GetString( vastaanOtto, 0, offset );
//                    Debug.Log("Vastaanotettiin viesti: " + vastaanotettuviesti);
                    espData = JsonUtility.FromJson<ESPDatat>(vastaanotettuviesti);
                    //NapinArvo = int.Parse( vastaanotettuviesti );
            }
            yhteysPoletti.Dispose();
        }
    }

    void OnDisable()
    {
        Debug.Log("OnDisable, poistetaan Thread");
        if ( yhteysPoletti != null ) yhteysPoletti.Cancel();
       
        
    }
    
    // Update is called once per frame
    void Update()
    {

    }
}