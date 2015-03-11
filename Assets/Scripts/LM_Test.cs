using UnityEngine;
using System.Collections;
using Leap;

public class LM_Test : MonoBehaviour {

	public HandController p_handController;

	public Vector p_r_position = new Vector();
	public Vector p_r_velocity = new Vector();
	public Vector p_r_direction = new Vector();
	
	public Vector p_l_position = new Vector();
	public Vector p_l_velocity = new Vector();
	public Vector p_l_direction = new Vector();

	public LM_Album album;

	public enum HandState{None, Right, Left, Both};
	public HandState handState = HandState.None;

	public HandList hands;
	public Hand leftHand;
	public Hand rightHand;

	public float rightHandStrength = 0f;
	public float leftHandStrength = 0f;

	public bool rightHandFist = false;
	public bool leftHandFist = false;

	private Pointable pointableL;
	private Pointable pointableR;
	private Leap.Vector leapPointL;
	private Leap.Vector leapPointR;
	private Leap.Vector leftPoint;
	private Leap.Vector rightPoint;

	private float appXL;
	private float appYL;
	private float appXR;
	private float appYR;
	public float appZL = 0;
	public float appZR;
	private int appWidth;
	private int appHeight;
	private InteractionBox iBox;

	public GameObject p_debug_ui;
	public GUITexture imagePreview;
	public GUITexture imagePreviewBorder;
	public GUITexture cameraMode;

	public Texture2D photo;
	public Texture2D video;

	public GameObject PlayerManager;
	private Transform camera1;
	private Transform camera2;
	private Transform player;

	private float rotationY = 0f;

	public bool handRotate = false;
	public bool handZoom = false;

	public bool albumOpen = false;

	public bool DebugMode = false;

	// Use this for initialization
	void Start () {
		player = PlayerManager.transform;
		Camera[] cameras = PlayerManager.GetComponentsInChildren<Camera> ();
		foreach (Camera t in cameras) {
			if(t.gameObject.name == "Main Camera")
				camera1 = t.transform;
			else if(t.gameObject.name == "Camera")
				camera2 = t.transform;
		}

		if(!DebugMode){
			Transform[] uiElements = p_debug_ui.GetComponentsInChildren<Transform>();
			foreach (Transform go in uiElements) {
				if(go.gameObject.tag == "Debug")
					go.gameObject.SetActive(false);
			}
		}

		GUITexture[] guitextures = p_debug_ui.GetComponentsInChildren<GUITexture> ();
		foreach (GUITexture gt in guitextures) {
			if(gt.gameObject.name == "PreviewImage"){
				imagePreview = gt;
			}else if(gt.gameObject.name == "PreviewImageBorder"){
				imagePreviewBorder = gt;
			}else if(gt.gameObject.name == "CameraMode"){
				cameraMode = gt;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		Frame frame = p_handController.GetLeapController().Frame(); // controller is a Controller object

		hands = frame.Hands;

		UpdateHandInfo();
		UpdateCameras ();

		if (hands.Count == 1) {
			if(hands[0].IsLeft){
				//left hand only
				handState = HandState.Left;
			}else{
				//right hand only
				handState = HandState.Right;
			}
		} else if (hands.Count == 2) {
			//both hands
			handState = HandState.Both;
		}else{
			//no hands
			handState = HandState.None;
		}

		UpdateDebugUI ();
	}

	private void UpdateCameras(){

		float sensitivityX = 8.0f;
		float sensitivityY = 8.0f;

		if ((handState == HandState.Right || handState == HandState.Both) && rightHandStrength < 0.1f && handRotate) {
			player.Rotate(0, ((appXR - (appWidth/2))/appWidth) * sensitivityX, 0);

			/*rotationY += ((appYR - (appHeight/2))/appHeight) * sensitivityY;
			rotationY = Mathf.Clamp (rotationY, -60f, 60f);

			Debug.Log("RotationY: "  + rotationY);
			
			camera1.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
			camera2.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);*/
		}
	}

	private void UpdateHandInfo(){

		appWidth = UnityEngine.Screen.width;
		appHeight = UnityEngine.Screen.height;
		
		iBox = p_handController.GetLeapController().Frame().InteractionBox;

		foreach(Hand hand in hands){
			if (hand.IsLeft) {
				leftHand = hand;
				p_l_position = hand.PalmPosition;
				p_l_velocity = hand.PalmVelocity;
				p_l_direction = hand.Direction;
				leftHandStrength = hand.GrabStrength;

				if(leftHandStrength > 0.5f)
					leftHandFist = true;
				else
					leftHandFist = false;

				pointableL = p_handController.GetLeapController().Frame().Pointables.Leftmost;
				leapPointL = pointableL.StabilizedTipPosition;
				leftPoint = differentialNormalizer (leapPointL, iBox, true, true);
				appZL = leftPoint.z;
				appXL = leftPoint.x * appWidth;
				appYL = leftPoint.y * appHeight;
			} else {
				rightHand = hand;
				p_r_position = hand.PalmPosition;
				p_r_velocity = hand.PalmVelocity;
				p_r_direction = hand.Direction;
				rightHandStrength = hand.GrabStrength;

				if(rightHandStrength > 0.5f)
					rightHandFist = true;
				else
					rightHandFist = false;

				pointableR = p_handController.GetLeapController().Frame().Pointables.Rightmost;
				leapPointR = pointableR.StabilizedTipPosition;
				rightPoint = differentialNormalizer (leapPointR, iBox, false, true);
				appZR = rightPoint.z;
				appXR = rightPoint.x * appWidth;
				appYR = rightPoint.y * appHeight;
			}
		}

		GUITexture[] _2dhands = p_debug_ui.GetComponentsInChildren<GUITexture>();
		foreach (GUITexture gt in _2dhands) {
			if(gt.name == "RightHand" && (handState == HandState.Both || handState == HandState.Right)){
				gt.enabled = true;
				gt.pixelInset = new Rect(appXR, appYR, gt.pixelInset.width, gt.pixelInset.height);
			}else if(gt.name == "LeftHand" && (handState == HandState.Both || handState == HandState.Left)){
				gt.enabled = true;
				gt.pixelInset = new Rect(appXL, appYL, gt.pixelInset.width, gt.pixelInset.height);
			}else if(gt.name == "CameraMode" && (handState == HandState.Both || handState == HandState.Left)){
				gt.enabled = true;
				gt.pixelInset = new Rect(UnityEngine.Screen.width/2 - gt.pixelInset.width/2, UnityEngine.Screen.height - gt.pixelInset.height - 20, gt.pixelInset.width, gt.pixelInset.height);
			}else if(gt.name != "PreviewImage" && gt.name != "PreviewImageBorder"){
				gt.enabled = false;
			}
		}
	}

	Leap.Vector differentialNormalizer(Leap.Vector leapPoint,
	                                   InteractionBox iBox,
	                                   bool isLeft,
	                                   bool clamp)
	{
		Leap.Vector normalized = iBox.NormalizePoint(leapPoint, false);
		float offset = isLeft ? 0.25f : -0.25f;
		normalized.x += offset;
		
		//clamp after offsetting
		normalized.x = (clamp && normalized.x < 0) ? 0 : normalized.x;
		normalized.x = (clamp && normalized.x > 1) ? 1 : normalized.x;
		normalized.y = (clamp && normalized.y < 0) ? 0 : normalized.y;
		normalized.y = (clamp && normalized.y > 1) ? 1 : normalized.y;
		
		return normalized;
	}
	
	//Update the values in the Debug UI
	private void UpdateDebugUI(){

		GUIText[] _text_fields = p_debug_ui.GetComponentsInChildren<GUIText>();
		foreach(GUIText gt in _text_fields){
			if(gt.name == "Right_Position"){
				p_r_position = new Vector(Mathf.Ceil(p_r_position.x), Mathf.Ceil(p_r_position.y), Mathf.Ceil(p_r_position.z));
				gt.text = "Right Position: " + p_r_position;
			}else if(gt.name == "Right_Direction"){
				p_r_direction = new Vector(Mathf.Ceil(p_r_direction.x), Mathf.Ceil(p_r_direction.y), Mathf.Ceil(p_r_direction.z));
				gt.text = "Right Direction: " + p_r_direction;
			}else if(gt.name == "Right_Velocity"){
				p_r_velocity = new Vector(Mathf.Ceil(p_r_velocity.x), Mathf.Ceil(p_r_velocity.y), Mathf.Ceil(p_r_velocity.z));
				gt.text = "Right Velocity: " + p_r_velocity;
			}else if(gt.name == "Left_Position"){
				p_l_position = new Vector(Mathf.Ceil(p_l_position.x), Mathf.Ceil(p_l_position.y), Mathf.Ceil(p_l_position.z));
				gt.text = "Left Position: " + p_l_position;
			}else if(gt.name == "Left_Direction"){
				p_l_direction = new Vector(Mathf.Ceil(p_l_direction.x), Mathf.Ceil(p_l_direction.y), Mathf.Ceil(p_l_direction.z));
				gt.text = "Left Direction: " + p_l_direction;
			}else if(gt.name == "Left_Velocity"){
				p_l_velocity = new Vector(Mathf.Ceil(p_l_velocity.x), Mathf.Ceil(p_l_velocity.y), Mathf.Ceil(p_l_velocity.z));
				gt.text = "Left Velocity: " + p_l_velocity;
			}else if(gt.name == "Hands"){
				gt.text = "Hands: " + handState;
			}
		}

		GUITexture[] _textures = p_debug_ui.GetComponentsInChildren<GUITexture>();
		foreach (GUITexture gt in _textures) {
			if(gt.name == "PreviewImage"){
				gt.pixelInset = new Rect (UnityEngine.Screen.width - gt.pixelInset.width - 40, UnityEngine.Screen.height/2 - (gt.pixelInset.height/2), gt.pixelInset.width, gt.pixelInset.height);
			}else if(gt.name == "PreviewImageBorder"){
				gt.pixelInset = new Rect (UnityEngine.Screen.width - gt.pixelInset.width - 40 + 5, UnityEngine.Screen.height/2 - (gt.pixelInset.height/2), gt.pixelInset.width, gt.pixelInset.height);
			}
		}
	}
}
