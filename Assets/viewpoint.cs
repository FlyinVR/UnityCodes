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
	public static double gyroYRight;
	public static double gyroRRight;
	public static double gyroPRight;

	public static double gyroYLeft;
	public static double gyroRLeft;
	public static double gyroPLeft;

	public static bool shouldReset;

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


	// Use this for initialization
	void Start()
	{
		initUDP();
		up.Normalize();
		//up = new Vector3(-1.0f, 10.0f, -1.0f);
//		tip = Vector3.Cross (up, Vector3.Cross (v, up)).normalized;
		tip.Normalize();
		transform.position = new Vector3(813.0f, 1000.0f, 874.0f);
		//transform.position = new Vector3(813.0f, 330.0f, 874.0f);
		Quaternion rotation = Quaternion.LookRotation(v, up);
		transform.rotation = rotation;

	}

	// Update is called once per frame
	void Update()
	{	
		Vector3 axis1 = Vector3.Cross (up, tip);
		Vector3 axis2 = -tip;
	
		tip = Quaternion.AngleAxis(Math.Min(90.0f,Input.GetAxis("Vertical")), axis1)*tip;
		up = Quaternion.AngleAxis(Input.GetAxis("Horizontal"), axis2)*up;

		float v_up = Vector3.Dot (v, up);
		Vector3 lift = density * v_up * 0.02f * v_up * up;
		Vector3 gravity = -g * e_y;
		Vector3 friction = -fric * Vector3.Dot (v, tip) * tip;
//		print ("fric:"+friction);
//		print ("lift" + lift);
		v = v + (gravity+lift)*Time.deltaTime*C;
		transform.position += v * Time.deltaTime;
//		print ("velocity: "+v);
		Quaternion rotation = Quaternion.LookRotation(v, up);
		transform.rotation = rotation;

		float terrainHeightWhereWeAre = Terrain.activeTerrain.SampleHeight(transform.position);
		if (terrainHeightWhereWeAre > transform.position.y || GlobalVariables.shouldReset)
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
