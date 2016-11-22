using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

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

public class viewpoint : MonoBehaviour
{
    Thread receiveThread;
    UdpClient client;

    public float g = 9.8f;
    public float c_d = 0.5f;
    public float c_l = 1.0f;
    public Vector3 up = new Vector3(-1.0f, 10.0f, -1.0f);
    public Vector3 v = new Vector3(-1.0f, -0.10f, -1.0f);
    public Vector3 e_xz;
    public Vector3 e_y = new Vector3(0.0f, 1.0f, 0.0f);
    public float c = 1.0f;


    // Use this for initialization
    void Start()
    {
        initUDP();
        transform.position = new Vector3(813.0f, 500.0f, 874.0f);
        //transform.position = new Vector3(813.0f, 330.0f, 874.0f);
        // Quaternion rotation = Quaternion.Euler(0.0f, -139.346f, 0.0f);
        Quaternion rotation = Quaternion.LookRotation(v, up);
        transform.rotation = rotation;

        e_xz = (v-Vector3.Dot(v,e_y)*e_y).normalized;
        print(v);
    }

    // Update is called once per frame
    void Update()
    {

        UpdatePosition();
        //transform.Rotate(-Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"), 0.0f);

        UpdateV();

        float terrainHeightWhereWeAre = Terrain.activeTerrain.SampleHeight(transform.position);
        if (terrainHeightWhereWeAre > transform.position.y)
        {
            reset();
        }
    }

    void UpdatePosition()
    {
        transform.position += v * Time.deltaTime;
        Quaternion rotation = Quaternion.LookRotation(v, up);
        print(Quaternion.Equals(transform.rotation, rotation));
        transform.rotation = rotation;
        print(transform.position);
        print(v.ToString("F8"));
    }

    void UpdateV()
    {
        float s_d = Vector3.Dot(v, e_y);
        float s_l = Vector3.Dot(v, e_xz);
        float k = GetK();
    
        float s_q = (float)Math.Sqrt(Math.Pow(s_d, 2) + Math.Pow(s_l, 2));
        float s_l_old = s_l;
        s_l -= (k * s_q * (c_l * s_d - c_d * s_l)) * c * Time.deltaTime;
        s_d -= (g - k * s_q * (c_l * s_l_old + c_d * s_d)) * c * Time.deltaTime;
        
        //Vector3 v_old = v;
        v = s_d * e_y + s_l * e_xz;
        //up = (v - v_old).normalized;
        //print("up:");
        //print(up);
    }

    float GetK()
    {
        return 0.024f * Vector3.Cross(v.normalized, e_y).magnitude;
    }

    void OnCollisionEnter(Collision col)
    {
        reset();
    }

    void reset()
    {
        if (receiveThread != null) receiveThread.Abort();
        if (client != null) client.Close();
        SceneManager.LoadScene(1);
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
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(receiveBytes);

                Byte[] slice1 = new List<Byte>(receiveBytes).GetRange(0, 4).ToArray();
                Int32 p = BitConverter.ToInt32(slice1, 0);
                //				print(p);
                Byte[] slice2 = new List<Byte>(receiveBytes).GetRange(4, 4).ToArray();
                Int32 y = BitConverter.ToInt32(slice2, 0);
                //				print(y);
                Byte[] slice3 = new List<Byte>(receiveBytes).GetRange(8, 4).ToArray();
                Int32 r = BitConverter.ToInt32(slice3, 0);
                //				print(r);

                GlobalVariables.gyroR = (r - 180000.0) / 1000.0;
                GlobalVariables.gyroY = (y - 180000.0) / 1000.0;
                GlobalVariables.gyroP = (p - 180000.0) / 1000.0;

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

    public void OnApplicationQuit()
    {
        // end of application
        if (receiveThread != null) receiveThread.Abort();
        if (client != null) client.Close();
        // print("Stopped");
    }
}
