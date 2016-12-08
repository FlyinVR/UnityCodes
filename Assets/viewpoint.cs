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
	public static double gyroYRight=0;
	public static double gyroRRight=0;
	public static double gyroPRight=0;

	public static double gyroYLeft=0;
	public static double gyroRLeft=0;
	public static double gyroPLeft=0;

	public static double gyroYRight0=0;
	public static double gyroRRight0=0;
	public static double gyroPRight0=0;

	public static double gyroYLeft0=0;
	public static double gyroRLeft0=0;
	public static double gyroPLeft0=0;

	public static double fanSpeed=0;

	public static bool shouldReset=false;
}

public class viewpoint : MonoBehaviour
{
	Thread receiveRightThread;
	UdpClient clientRight;
	Thread receiveLeftThread;
	UdpClient clientLeft;

    private float g = 9.8f;
    private float density = 1.0f;
    private float fric = 0.001f;
    private float C = 1.0f;
    private Vector3 up = new Vector3(-1.0f, 10.0f, -1.2f);
    private Vector3 tip = new Vector3(-1.0f, -1.0f, -2.0f);
    //	private Vector3 tip = new Vector3(-0.0f, -1.0f, 0.0f);
    private Vector3 v = new Vector3(-2.0f, 0.0f, -2.0f);
    private Vector3 e_y = new Vector3(0.0f, 1.0f, 0.0f);

	private double flyerR = 0;
	private double flyerY = 0;
	private double flyerP = 0;


    // Use this for initialization
    void Start()
    {
        g = 9.8f;
        density = 1.0f;
        fric = 0.001f;
        C = 1.0f;
        up = new Vector3(-1.0f, 10.0f, -1.2f);
        tip = new Vector3(-1.0f, -1.0f, -2.0f);
        //	private Vector3 tip = new Vector3(-0.0f, -1.0f, 0.0f);
        v = new Vector3(-2.0f, 0.0f, -2.0f);

        initUDP();
        up.Normalize();
        //up = new Vector3(-1.0f, 10.0f, -1.0f);
        //		tip = Vector3.Cross (up, Vector3.Cross (v, up)).normalized;
        tip.Normalize();
        transform.position = new Vector3(6501.706f, 4290f, 6963.654f);
        //transform.position = new Vector3(813.0f, 330.0f, 874.0f);
        Quaternion rotation = Quaternion.LookRotation(v, up);
        transform.rotation = rotation;
    }

    // Update is called once per frame
    void Update()
    {
		Vector3 rightArm = Vector3.Cross (up, tip);
		Vector3 axis2 = -tip;

//		print ("up key"+Input.GetAxis("Vertical"));

		//-90<p,r,y<90
		//rise right arm = gyroPRight increases. ->90
		double p = (GlobalVariables.gyroPRight-GlobalVariables.gyroPRight0 
			- GlobalVariables.gyroPLeft+GlobalVariables.gyroPLeft0) / 2;

		p = Math.Min (p, 80);
		p = Math.Max (p, -80);

		double deltaUp = (p - flyerP) / 2;
		
		//hands rotate backwards = gyroRRight decreases ->90
		double r = -(GlobalVariables.gyroRRight-GlobalVariables.gyroRRight0 
			- GlobalVariables.gyroRLeft+GlobalVariables.gyroRLeft0) / 2;
		r = Math.Min (r, 80);
		r = Math.Max (r, -80);

		double deltaTip = (r - flyerR) / 2;

		//fold arms = gyroYRight decreases ->90
		double y = (-GlobalVariables.gyroYRight+GlobalVariables.gyroYRight0 
			+ GlobalVariables.gyroYLeft-GlobalVariables.gyroYLeft0) / 2;
		y = Math.Min (y, 80);
		y = Math.Max (y, -80);

		float frictionFactor = (float) (1 - Math.Sin(flyerR/180)); 


		Vector3 newtip = Quaternion.AngleAxis(Math.Min(80.0f,Input.GetAxis("Vertical")), rightArm)*tip;

		Vector3 newup = Quaternion.AngleAxis(Input.GetAxis("Horizontal"), axis2)*up;

		if (Vector3.Dot (newtip, -e_y) < 0.9 & Vector3.Dot (newup, e_y) > 0.1) {
			tip = newtip;
			up = newup;
		}

        float v_up = Vector3.Dot(v, up);
        Vector3 lift = density * v_up * 0.02f * v_up * up;
        Vector3 gravity = -g * e_y;
		Vector3 friction = -fric * Vector3.Dot(v, tip)* frictionFactor * tip;

		Vector3 newv = v + (gravity + lift + friction) * Time.deltaTime * C;
		if (Vector3.Dot(newv.normalized,e_y)<0.8){
			v = newv;
			transform.position += v * Time.deltaTime;
			print("velocity: " + v);

			GlobalVariables.fanSpeed = Math.Min (v.magnitude / 50, 1);

			Quaternion rotation = Quaternion.LookRotation(v, up);
			transform.rotation = rotation;
		}

        float terrainHeightWhereWeAre = Terrain.activeTerrain.SampleHeight(transform.position);
        if (terrainHeightWhereWeAre > transform.position.y)
		{
			GlobalVariables.shouldReset = false;
			print ("should reset");
			reset();
        }
    }

    void OnCollisionEnter(Collision col)
    {
        reset();
    }

    void reset()
    {
		if (receiveRightThread != null) receiveRightThread.Abort();
		if (clientRight != null) clientRight.Close();
		if (receiveLeftThread != null) receiveLeftThread.Abort();
		if (clientLeft != null) clientLeft.Close();
		SceneManager.LoadScene(1);
    }


    private void initUDP()
    {
		receiveRightThread = new Thread(
			new ThreadStart(ReceiveRightData));

		receiveRightThread.IsBackground = true;
		receiveRightThread.Start();

		receiveLeftThread = new Thread(
			new ThreadStart(ReceiveLeftData));

		receiveLeftThread.IsBackground = true;
		receiveLeftThread.Start();

        print("Start UDP Client");
    }

	private void ReceiveRightData()
	{
		int portRight = 6390;

		try
		{

			clientRight = new UdpClient(portRight);
			IPEndPoint RemoteIpEndPointRight = new IPEndPoint(IPAddress.Any, 0);
			//client.Client.Blocking = false;
			while (Thread.CurrentThread.IsAlive)
			{
				// do stuff
				Byte[] receiveBytes = clientRight.Receive(ref RemoteIpEndPointRight);
				if (BitConverter.IsLittleEndian)
					Array.Reverse(receiveBytes);

				Byte[] slice1 = new List<Byte>(receiveBytes).GetRange(0, 4).ToArray();
				UInt32 p = BitConverter.ToUInt32(slice1, 0);
				//				print(p);
				Byte[] slice2 = new List<Byte>(receiveBytes).GetRange(4, 4).ToArray();
				UInt32 y = BitConverter.ToUInt32(slice2, 0);
				//								print(y);
				Byte[] slice3 = new List<Byte>(receiveBytes).GetRange(8, 4).ToArray();
				UInt32 r = BitConverter.ToUInt32(slice3, 0);
				//								print(r);

				if (r<UInt32.MaxValue && y<UInt32.MaxValue && p<UInt32.MaxValue){
					GlobalVariables.gyroRRight = (r - 180000.0) / 1000.0;
					GlobalVariables.gyroYRight = (y - 180000.0) / 1000.0;
					GlobalVariables.gyroPRight = (p - 180000.0) / 1000.0;
				}else{
					print("reset");
					GlobalVariables.shouldReset = true;
				}


				Thread.Sleep(10);
			}

		}
		catch (Exception e)
		{
			print(e);
		}
	}
	private void ReceiveLeftData()
	{
		int portLeft = 6391;

		try
		{

			clientLeft = new UdpClient(portLeft);
			IPEndPoint RemoteIpEndPointLeft = new IPEndPoint(IPAddress.Any, 0);
			//client.Client.Blocking = false;
			while (Thread.CurrentThread.IsAlive)
			{
				// do stuff
				Byte[] receiveBytes = clientLeft.Receive(ref RemoteIpEndPointLeft);
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

				GlobalVariables.gyroRLeft = (r - 180000.0) / 1000.0;
				GlobalVariables.gyroYLeft = (y - 180000.0) / 1000.0;
				GlobalVariables.gyroPLeft = (p - 180000.0) / 1000.0;

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
		if (receiveRightThread != null) receiveRightThread.Abort();
		if (clientRight != null) clientRight.Close();
		if (receiveLeftThread != null) receiveLeftThread.Abort();
		if (clientLeft != null) clientLeft.Close();
		// print("Stopped");
	}
}
