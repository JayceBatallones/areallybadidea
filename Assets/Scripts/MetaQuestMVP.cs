// MVP for Meta Quest 3 App (Unity - C#)
// Features: UI Overlay, Microphone Capture, WebSocket Connection

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class MetaQuestMVP : MonoBehaviour
{
    public Text responseText; // UI Overlay Text
    private ClientWebSocket websocket;
    private AudioClip microphoneInput;
    private bool isRecording = false;
    
    void Start()
    {
        ConnectToWebSocket();
    }
    
    async void ConnectToWebSocket()
    {
        websocket = new ClientWebSocket();
        await websocket.ConnectAsync(new System.Uri("wss://your-backend-server.com"), CancellationToken.None);
        Debug.Log("WebSocket Connected");
        ListenForResponses();
    }
    
    async void ListenForResponses()
    {
        byte[] buffer = new byte[1024];
        while (websocket.State == WebSocketState.Open)
        {
            var result = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            responseText.text = message; // Display response
        }
    }
    
    public void StartRecording()
    {
        if (!isRecording)
        {
            microphoneInput = Microphone.Start(null, false, 5, 44100);
            isRecording = true;
            StartCoroutine(SendAudio());
        }
    }
    
    IEnumerator SendAudio()
    {
        yield return new WaitForSeconds(5);
        if (isRecording)
        {
            byte[] audioData = WavUtility.FromAudioClip(microphoneInput);
            await websocket.SendAsync(new ArraySegment<byte>(audioData), WebSocketMessageType.Binary, true, CancellationToken.None);
            Microphone.End(null);
            isRecording = false;
        }
    }
}
