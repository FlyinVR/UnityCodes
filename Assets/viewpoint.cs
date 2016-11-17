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

				Int32 r = 0;
				for (int i=0; i<4; i++){
					r += BitConverter.ToInt32(receiveBytes,i)<<(i*8);
				}

				Int32 y = 0;
				for (int i=4; i<8; i++){
					y += BitConverter.ToInt32(receiveBytes,i)<<(i*8);
				}

				Int32 p = 0;
				for (int i=8; i<12; i++){
					p += BitConverter.ToInt32(receiveBytes,i)<<(i*8);
				}
				print(r);
				print(y);
				print(p);
				gyror = (r-180000.0)/1000.0;
				gyroY = (y-180000.0)/1000.0;
				gyroP = (p-180000.0)/1000.0;
					

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
