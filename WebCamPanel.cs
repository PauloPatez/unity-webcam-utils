/*
Add this to a GameObject with a Renderer component to display camera image
*/

using UnityEngine;
using System.Collections;
using Utilities;

public class WebCamPanel : MonoBehaviour {
	WebCamTexture webcam;
	public string frontCamName;
	public string backCamName;
	public bool isBackCam;

    void Start(){
		frontCamName = WebcamUtils.GetFrontCamName();
		backCamName = WebcamUtils.GetBackCamName();
		Debug.Log(frontCamName);
		Debug.Log(backCamName);
		if (backCamName != null){
			webcam = new WebCamTexture(backCamName);
			webcam.Play();
			gameObject.GetComponent<Renderer>().material.mainTexture = webcam;
			Apply();
			isBackCam = true;
		}else{
			webcam = new WebCamTexture(frontCamName);
			webcam.Play();
			gameObject.GetComponent<Renderer>().material.mainTexture = webcam;
			Apply();
		}
		Apply();
	}
	public void Apply(){
		WebcamUtils.SetOrientation (webcam, gameObject);
		WebcamUtils.SetRatio (WebcamUtils.GetRatio (webcam), gameObject);
		WebcamUtils.SetCameraSize (Camera.main, gameObject.GetComponent<Renderer>());
	}
	public void Play(){
		webcam.Play();
		transform.GetComponent<Renderer>().material.mainTexture = webcam;
	}
	public void Freeze(){
		webcam.Pause();
		var camTex = new Texture2D(webcam.width, webcam.height);
		camTex.SetPixels(webcam.GetPixels());
		camTex.Apply();
		webcam.Stop();
		transform.GetComponent<Renderer>().material.mainTexture = camTex;
	}
	public void SwitchCamera(){
		if(frontCamName != null && backCamName != null){
			if(isBackCam){
				webcam.Stop();
				webcam = new WebCamTexture(frontCamName);
				webcam.Play();
				transform.GetComponent<Renderer>().sharedMaterial.mainTexture = webcam;
				Apply();
				isBackCam = false;
			}else{
				webcam.Stop();
				webcam = new WebCamTexture(backCamName);
				webcam.Play();
				transform.GetComponent<Renderer>().sharedMaterial.mainTexture = webcam;
				Apply();
				isBackCam = true;
			}
			Debug.Log(isBackCam);
		}else{Debug.Log("There's no other camera");}
		Apply();
	}
}
