using static Tensorflow.Binding;
using static Tensorflow.KerasApi;
using Tensorflow;
using UnityEngine;
using Tensorflow.Keras.Engine;
using System;
using System.Collections.Generic;
using Tensorflow.Training;
using System.Linq;
using Unity.VisualScripting;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Net.Http;
using System.Text;
using System.IO;
using UnityEngine.UIElements;
using Grpc.Net.Client;
using Grpc.Core;
using Grpc.Net.Client.Web;
using Routeguide;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Runtime.CompilerServices;
using Ddpg;

class DdpgTrainer : MonoBehaviour
{


    [SerializeField] string host = "http://localhost:8000";

    private float timer = 0;

    int trainingInterval = 500;

    private int frameCount = 0;
    RouteGuide.RouteGuideClient client;
    public static DdpgTrainer I { get; private set; }

    public float noise = 1;
    public float resetThreshold = 0.1f;
    public float testInterval = 2;
    public float rayCastDistance = 100;

    bool isTraining = false;

    TaskAwaiter<Routeguide.VoidMsg> trainingAwaiter;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(this);
            return;
        }
        I = this;
    }

    private void Start()
    {
        var handler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler());
        var channel = GrpcChannel.ForAddress(host, new GrpcChannelOptions
        {
            HttpHandler = handler,
            Credentials = ChannelCredentials.Insecure
        });
        client = new RouteGuide.RouteGuideClient(channel);

        //try
        //{
        //    client.GetAction(new Routeguide.GetActionArgs(), deadline: DateTime.UtcNow.AddSeconds(1));
        //}
        //catch (Exception)
        //{
        //    Debug.LogWarning("GetAction failed check server status");
        //    #if UNITY_EDITOR
        //        UnityEditor.EditorApplication.isPlaying = false;
        //    #else
        //        Application.Quit();
        //    #endif
        //}
    }

    public TaskAwaiter<Routeguide.Action> GetActionAsync(DdpgState state)
    {
        Routeguide.State state_ = new Routeguide.State();
        state_.Data.AddRange(state.GetData());
        var getActionCall = client.GetActionAsync(
            new Routeguide.GetActionArgs
            {
                State = state_,
                Random = false,
                Noise = true
            }
        );
        //return new Action(action_.Data[0], action_.Data[1], action_.Data[2]);
        return getActionCall.GetAwaiter();
    }

    private void Update()
    {
        if (isTraining)
        {
            if (trainingAwaiter.IsCompleted)
            {
                isTraining = false;
                Time.timeScale = GameManager.I.timeScale;
            }
            return;
        }

        timer += Time.deltaTime;
        if (frameCount > trainingInterval)
        {
            frameCount = 0;
            Time.timeScale = 0;
            isTraining = true;
            var traincall = client.TrainAsync(new Routeguide.VoidMsg());
            trainingAwaiter = traincall.GetAwaiter();
        }
    }
    private void FixedUpdate()
    {
        frameCount++;
    }

    public void RememberAsync(DdpgState state, DdpgAction action, float reward, DdpgState nextState, bool done)
    {
        client.RememberAsync(new Routeguide.DataPoint
        {
            State = new Routeguide.State
            {
                Data = { state.GetData() }
            },
            Action = new Routeguide.Action
            {
                Data = { action.GetData() }
            },
            Reward = reward,
            NextState = new Routeguide.State
            {
                Data = { nextState.GetData() }
            },
            Done = done
        });
    }
}

//    object CallRemoteFunction(string functionName, object[] parameters, bool hasreply = false)
//    {
//        return null;
//        if (!client.Connected)
//        {
//            Debug.LogWarning("python trainer not connected");
//            return null;
//        }
//        NetworkStream stream = client.GetStream();
//        var messageObj = new
//        {
//            function = functionName,
//            parameters = parameters
//        };

//        var serializer = new JsonSerializer();
//        string message;
//        using (var writer = new StringWriter())
//        {
//            serializer.Serialize(writer, messageObj);
//            message = writer.ToString();
//        }

//        byte[] data = Encoding.UTF8.GetBytes(message);
//        stream.Write(data, 0, data.Length);

//        if(hasreply)
//        {
//            byte[] responseBuffer = new byte[1024];
//            stream.ReadTimeout = 200;
//            int bytesRead = stream.Read(responseBuffer, 0, responseBuffer.Length);
//            string responseJson = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);
//            if (responseJson == null || responseJson == "")
//            {
//                return null;
//            }

//            JsonReader reader = new JsonTextReader(new StringReader(responseJson));

//            var x = serializer.Deserialize<Action?>(reader);
//            if(x == null)
//            {
//                return null;
//            }
//            Debug.Log(x);
//            return x;
//        }
//        return null;
//    }
//}
