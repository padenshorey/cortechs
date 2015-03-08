using UnityEngine;
using System.Collections;

public class CameraPreview : MonoBehaviour {

	private Camera camera;
	public Camera mainCamera;
	private float growth = 0.06f;

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
		if (Input.GetKeyDown ("i")) {
			camera.rect = new Rect(camera.rect.x, camera.rect.y - (growth/2f), camera.rect.width, camera.rect.height + growth);
			camera.fieldOfView = 60f*camera.rect.height;
		}
		
		if (Input.GetKeyDown ("k")) {
			camera.rect = new Rect(camera.rect.x, camera.rect.y + (growth/2f), camera.rect.width, camera.rect.height - growth);
			camera.fieldOfView = 60f*camera.rect.height;
		}

		if (Input.GetKeyDown ("j")) {
			camera.rect = new Rect(camera.rect.x + (growth/2f), camera.rect.y, camera.rect.width - growth, camera.rect.height);
		}
		
		if (Input.GetKeyDown ("l")) {
			camera.rect = new Rect(camera.rect.x - (growth/2f), camera.rect.y, camera.rect.width + growth, camera.rect.height);
		}
	}

}
