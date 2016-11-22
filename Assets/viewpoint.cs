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
		transform.position = new Vector3(813.0f, 500.0f, 874.0f);
		//transform.position = new Vector3(813.0f, 330.0f, 874.0f);
		Quaternion rotation = Quaternion.LookRotation(v, up);
		transform.rotation = rotation;

		Input.GetAxis("Vertical");
		Input.GetAxis("Horizontal");

	}

	// Update is called once per frame
	void Update()
	{
		float v_up = Vector3.Dot (v, up);
		Vector3 lift = density * v_up * 0.02f * v_up * up;
		Vector3 gravity = -g * e_y;
		Vector3 friction = -fric * Vector3.Dot (v, tip) * tip;
//		print ("fric:"+friction);
		print ("lift" + lift);
		v = v + (gravity+lift)*Time.deltaTime*C;
		transform.position += v * Time.deltaTime;
		print ("velocity: "+v);
		Quaternion rotation = Quaternion.LookRotation(v, up);
		transform.rotation = rotation;

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
