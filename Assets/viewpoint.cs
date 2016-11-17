using UnityEngine;
using System.Collections;
using System.Net;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Text;

public static class GlobalVariables
{
    public static float gyroY;
    public static float gyroR;
    public static float gyroP;

}

public class viewpoint : MonoBehaviour {
    Thread receiveThread;
    UdpClient client;
    // Use this for initialization
    void Start () {
        initUDP();
    }

    private void initUDP()
    {
        receiveThread = new Thread(
            new ThreadStart(ReceiveData));

        receiveThread.IsBackground = true;
        receiveThread.Start();

        print("Start UDP Client");
    }

    private void ReceiveData()
    {
        int somePort = 6390;

        try
        {

            client = new UdpClient(somePort);
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            //client.Client.Blocking = false;
            while (Thread.CurrentThread.IsAlive)
            {
                // do stuff
                Byte[] receiveBytes = client.Receive(ref RemoteIpEndPoint);
                string returnData = Encoding.ASCII.GetString(receiveBytes);

                // Uses the IPEndPoint object to determine which of these two hosts responded.
                Console.WriteLine("This is the message you received " +
                                             returnData.ToString());
                Console.WriteLine("This message was sent from " +
                                            RemoteIpEndPoint.Address.ToString() +
                                            " on their port number " +
                                            RemoteIpEndPoint.Port.ToString());
                Thread.Sleep(10);
            }

        }
        catch (Exception e)
        {
            print(e);
        }
    }
    // Update is called once per frame
    void Update () {
        transform.position += transform.forward * Time.deltaTime * 50.0f;

        transform.Rotate(-Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"), 0.0f);

        float terrainHeightWhereWeAre = Terrain.activeTerrain.SampleHeight(transform.position);
        if (terrainHeightWhereWeAre > transform.position.y)
        {
            reset();
        }
	}

    void OnCollisionEnter(Collision col)
    {
        reset();
    }

    void reset()
    {
        transform.position = new Vector3(813.0f, 330.0f, 874.0f);
        Quaternion rotation = Quaternion.Euler(0.0f, -139.346f, 0.0f);
        transform.rotation = rotation;
    }

    public void OnApplicationQuit()
    {
        // end of application
        if (receiveThread != null) receiveThread.Abort();
        if (client != null) client.Close();
        print("Stopped");
    }
}
