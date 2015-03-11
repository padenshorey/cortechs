using UnityEngine;
using System.Collections;

public class CameraPreview : MonoBehaviour {

	private Camera camera;
	public Camera mainCamera;
	private float growth = 0.06f;

	public GUITexture border;
	public Texture2D defaultPhoto;
	private float borderStroke = 10;

	public LM_Test sceneController;
	public GameObject rightHandUI;
	public GameObject leftHandUI;

	public float previewScale = 2.5f;

	private bool leftHandFist = false;

	public bool tookPhoto = false;
	public bool switchedModes = false;
	public bool albumComing = false;
	public bool albumGoing = false;

	public GameObject screenFader;

	private float originalZoom = -0.2895989f;

	// Use this for initialization
	void Start () {
		camera = this.gameObject.GetComponent<Camera> ();
	}

	void LateUpdate() {
		//Camera cam = camera;
		//cam.projectionMatrix = mainCamera.projectionMatrix;
	}

	// Update is called once per frame
	void Update () {
		//DEBUG STUFF
		if (Input.GetKeyDown ("i") && camera.rect.height <= (1.0f-growth)) {
			camera.rect = new Rect(camera.rect.x, camera.rect.y - (growth/2f), camera.rect.width, camera.rect.height + growth);
			camera.fieldOfView = 60f*camera.rect.height;
			camera.aspect = (mainCamera.fieldOfView * mainCamera.aspect * camera.rect.width) / camera.fieldOfView;
		}
		
		if (Input.GetKeyDown ("k") && camera.rect.height >= growth) {
			camera.rect = new Rect(camera.rect.x, camera.rect.y + (growth/2f), camera.rect.width, camera.rect.height - growth);
			camera.fieldOfView = 60f*camera.rect.height;
			camera.aspect = (mainCamera.fieldOfView * mainCamera.aspect * camera.rect.width) / camera.fieldOfView;
		}

		if (Input.GetKeyDown ("j") && camera.rect.width >= growth) {
			camera.rect = new Rect(camera.rect.x + (growth/2f), camera.rect.y, camera.rect.width - growth, camera.rect.height);
			camera.aspect = (mainCamera.fieldOfView * mainCamera.aspect * camera.rect.width) / camera.fieldOfView;
		}
		
		if (Input.GetKeyDown ("l") && camera.rect.width <= (1.0f-growth)) {
			camera.rect = new Rect(camera.rect.x - (growth/2f), camera.rect.y, camera.rect.width + growth, camera.rect.height);
			camera.aspect = (mainCamera.fieldOfView * mainCamera.aspect * camera.rect.width) / camera.fieldOfView;
		}

		//REAL STUFF
		if ((sceneController.handState == LM_Test.HandState.Left || sceneController.handState == LM_Test.HandState.Both)
		    && sceneController.leftHandStrength < 0.1f && !sceneController.albumOpen) {
			this.GetComponent<Camera>().enabled = true;
			Rect pil = leftHandUI.GetComponent<GUITexture>().pixelInset;
			border.enabled = true;

			//handle different sections of the screen
			if(pil.x <= Screen.width/2 && pil.y <= Screen.height/2){
				//bottom left
				camera.rect = new Rect(pil.x/Screen.width,
				                       pil.y/Screen.height,
				                       (((Screen.width/2)-pil.x)*2)/Screen.width,
				                       (((Screen.height/2)-pil.y)*2)/Screen.height);
			}else if(pil.x >= Screen.width/2 && pil.y <= Screen.height/2){
				//bottom right
				camera.rect = new Rect((Screen.width-pil.x)/Screen.width,
				                       pil.y/Screen.height,
				                       (((Screen.width/2)-(Screen.width-pil.x))*2)/Screen.width,
				                       (((Screen.height/2)-pil.y)*2)/Screen.height);
			}else if(pil.x <= Screen.width/2 && pil.y >= Screen.height/2){
				//top left
				camera.rect = new Rect(pil.x/Screen.width,
				                       (Screen.height-pil.y)/Screen.height,
				                       (((Screen.width/2)-pil.x)*2)/Screen.width,
				                       (((Screen.height/2)-(Screen.height-pil.y))*2)/Screen.height);
			}else if(pil.x >= Screen.width/2 && pil.y >= Screen.height/2){
				//top right
				camera.rect = new Rect((Screen.width-pil.x)/Screen.width,
				                       (Screen.height-pil.y)/Screen.height,
				                       (((Screen.width/2)-(Screen.width-pil.x))*2)/Screen.width,
				                       (((Screen.height/2)-(Screen.height-pil.y))*2)/Screen.height);
			}

			//fix field of view and aspect ratio


			if(sceneController.handZoom){
				float potentialFOV = 60f*camera.rect.height + (sceneController.appZL*100);
				if(potentialFOV < 10.0f)
					camera.fieldOfView = 10.0f;
				else if(potentialFOV > 60f*camera.rect.height)
					camera.fieldOfView = 60f*camera.rect.height;
				else
					camera.fieldOfView = potentialFOV;
			}else{
				camera.fieldOfView = 60f*camera.rect.height;
			}

			//camera.fieldOfView = 60f*camera.rect.height;

			/*if(sceneController.appZL < 0)
				camera.fieldOfView -= 1;
			else if(sceneController.appZL > 0)
				camera.fieldOfView += 1;*/


			/*float potentialZoom = originalZoom + sceneController.appZL;
			if(potentialZoom < originalZoom)
				this.gameObject.transform.position = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y, originalZoom);
			else if(potentialZoom > 50f)
				this.gameObject.transform.position = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y, 50.0f);
			else
				this.gameObject.transform.position = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y, originalZoom + sceneController.appZL);*/
			
			camera.aspect = (mainCamera.fieldOfView * mainCamera.aspect * camera.rect.width) / camera.fieldOfView;
		}else{
			this.GetComponent<Camera>().enabled = false;
			border.enabled = false;
		}

		//handle taking the photo
		if (sceneController.handState == LM_Test.HandState.Both) {
			//only take a picture if the camera is on
			if(camera.enabled){
				if(!sceneController.leftHandFist){
					if(sceneController.rightHandFist){
						if(!tookPhoto)
							TakePhoto();
					}else{
						tookPhoto = false;
					}
				}
			}else{
				//left hand in fist
				//handles mode switching
				if(sceneController.rightHandFist && !sceneController.albumOpen){
					if(!switchedModes)
						SwitchModes();
				}else{
					switchedModes = false;
				}

				//bring up album
				if(sceneController.p_r_velocity.y > 500f && !albumComing){
					albumComing = true;
					sceneController.albumOpen = true;
					albumGoing = false;
					sceneController.album.StartLerp();
					sceneController.album.direction = true;
				}else if(sceneController.p_r_velocity.y < -500f && !albumGoing){
					albumComing = false;
					sceneController.albumOpen = false;
					albumGoing = true;
					sceneController.album.StartLerp();
					sceneController.album.direction = false;
				}
			}
		}

		border.pixelInset = new Rect(camera.rect.x*Screen.width - (borderStroke/2), camera.rect.y*Screen.height - (borderStroke/2), camera.rect.width*Screen.width + borderStroke, camera.rect.height*Screen.height + borderStroke);
	}

	private void SwitchModes(){
		if (sceneController.cameraMode.texture == sceneController.photo) {
			sceneController.cameraMode.texture = sceneController.video;
		}else{
			sceneController.cameraMode.texture = sceneController.photo;
		}

		switchedModes = true;
	}

	private void TakePhoto(){
		Debug.Log ("*SNAP*");
		tookPhoto = true;

		StartCoroutine(CaptureImage());
	}

	private IEnumerator CaptureImage(){

		yield return new WaitForEndOfFrame();

		Texture2D tex = new Texture2D((int)Mathf.Floor(camera.rect.width*Screen.width), (int)Mathf.Floor(camera.rect.height*Screen.height));
		tex.ReadPixels(new Rect((int)Mathf.Floor(camera.rect.x*Screen.width), (int)Mathf.Floor(camera.rect.y*Screen.height), (int)Mathf.Floor(camera.rect.width*Screen.width), (int)Mathf.Floor(camera.rect.height*Screen.height)), 0, 0);
		tex.Apply();

		sceneController.imagePreviewBorder.pixelInset = new Rect (sceneController.imagePreview.pixelInset.x - 5, sceneController.imagePreview.pixelInset.y - 5, camera.rect.width*Screen.width/previewScale + 10, camera.rect.height*Screen.height/previewScale + 10);
		sceneController.imagePreview.pixelInset = new Rect (sceneController.imagePreview.pixelInset.x, sceneController.imagePreview.pixelInset.y, camera.rect.width*Screen.width/previewScale, camera.rect.height*Screen.height/previewScale);
		sceneController.imagePreview.texture = tex;

		GUITexture[] photosInAlbum = sceneController.album.GetComponentsInChildren<GUITexture>();
		foreach (GUITexture gt in photosInAlbum) {
			if(gt.texture == defaultPhoto){
				gt.texture = tex;
				break;
			}
		}

		Instantiate (screenFader);
	}

}
