using UnityEngine;
using System.Collections;
<<<<<<< HEAD
using UnityEngine.SceneManagement;
=======
using System.Collections.Generic;
using System.Net;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Text;

public static class GlobalVariables
{
	public static double gyroY;
	public static double gyroR;
	public static double gyroP;

}
>>>>>>> origin/master

public class viewpoint : MonoBehaviour {
    Thread receiveThread;
    UdpClient client;
    // Use this for initialization
    void Start () {
        initUDP();
    }

<<<<<<< HEAD
	// Use this for initialization
	void Start () {
        transform.position = new Vector3(813.0f, 330.0f, 874.0f);
        Quaternion rotation = Quaternion.Euler(0.0f, -139.346f, 0.0f);
        transform.rotation = rotation;
    }
	
	// Update is called once per frame
	void Update () {
=======
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
				if (BitConverter.IsLittleEndian)
					Array.Reverse(receiveBytes);
				
				Byte[] slice1 = new List<Byte>(receiveBytes).GetRange(0, 4).ToArray();
				Int32 p = BitConverter.ToInt32(slice1,0);
//				print(p);
				Byte[] slice2 = new List<Byte>(receiveBytes).GetRange(4, 4).ToArray();
				Int32 y = BitConverter.ToInt32(slice2,0);
//				print(y);
				Byte[] slice3 = new List<Byte>(receiveBytes).GetRange(8, 4).ToArray();
				Int32 r = BitConverter.ToInt32(slice3,0);
//				print(r);

				GlobalVariables.gyroR = (r-180000.0)/1000.0;
				GlobalVariables.gyroY = (y-180000.0)/1000.0;
				GlobalVariables.gyroP = (p-180000.0)/1000.0;
					
//				string returnData = Encoding.UTF8.GetString(receiveBytes);
//				print(returnData);
//				var base64EncodedBytes = System.Convert.FromBase64String(returnData);
//				print(System.Text.Encoding.UTF8.GetString(base64EncodedBytes));
//
//                // Uses the IPEndPoint object to determine which of these two hosts responded.
//				print("This is the message you received " +
//                                             returnData.ToString());
//				print("This message was sent from " +
//                                            RemoteIpEndPoint.Address.ToString() +
//                                            " on their port number " +
//                                            RemoteIpEndPoint.Port.ToString());
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
>>>>>>> origin/master
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
        SceneManager.LoadScene(1);
    }

    public void OnApplicationQuit()
    {
        // end of application
        if (receiveThread != null) receiveThread.Abort();
        if (client != null) client.Close();
        print("Stopped");
    }
}
