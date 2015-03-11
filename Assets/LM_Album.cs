using UnityEngine;
using System.Collections;

public class LM_Album : MonoBehaviour {
	
	public float speed = 0.05F;
	private float startTime;
	private float journeyLength;
	private bool lerp = false;
	private float y = 0;
	//true = up, false = down
	public bool direction = true;

	void Start() {

	}

	void Update() {

		if (Input.GetKeyDown ("n")) {
			StartLerp();
			direction = true;
		}

		if (Input.GetKeyDown ("m")) {
			StartLerp();
			direction = false;
		}

		if (lerp) {
			if(direction)
				DoLerp (y, 0f);
			else
				DoLerp (y, -1f);
		}
	}

	public void StartLerp(){
		startTime = Time.time;
		journeyLength = Vector3.Distance(new Vector3(0f, -1f, 0f), new Vector3(0f, 0f, 0f));
		y = this.transform.position.y;
		lerp = true;
	}

	public void DoLerp(float starty, float endy){
		float distCovered = (Time.time - startTime) * speed;
		float fracJourney = distCovered / journeyLength;
		transform.position = Vector3.Lerp (new Vector3(0f, starty, 0f), new Vector3(0f, endy, 0f), fracJourney);
	}
}
