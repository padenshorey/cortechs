using UnityEngine;
using System.Collections;
using Leap;

public class LM_Test : MonoBehaviour {

	public HandController p_handController;
	public Hand p_r_hand;
	public Vector p_r_position;
	public Vector p_r_velocity;
	public Vector p_r_direction;

	public Hand p_l_hand;
	public Vector p_l_position;
	public Vector p_l_velocity;
	public Vector p_l_direction;

	public GameObject p_debug_ui;
	

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		Frame frame = p_handController.GetLeapController().Frame(); // controller is a Controller object

		//right hand
		p_r_hand = frame.Hands.Rightmost;
		p_r_position = p_r_hand.PalmPosition;
		p_r_velocity = p_r_hand.PalmVelocity;
		p_r_direction = p_r_hand.Direction;

		//left hand
		p_l_hand = frame.Hands.Leftmost;
		p_l_position = p_l_hand.PalmPosition;
		p_l_velocity = p_l_hand.PalmVelocity;
		p_l_direction = p_l_hand.Direction;

		UpdateDebugUI ();
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

		int appWidth = UnityEngine.Screen.width;
		int appHeight = UnityEngine.Screen.height;
		
		InteractionBox iBox = p_handController.GetLeapController().Frame().InteractionBox;
		Pointable pointableL = p_handController.GetLeapController().Frame().Pointables.Leftmost;
		Pointable pointableR = p_handController.GetLeapController().Frame().Pointables.Rightmost;
		
		Leap.Vector leapPointL = pointableL.StabilizedTipPosition;
		Leap.Vector leapPointR = pointableR.StabilizedTipPosition;

		Leap.Vector leftPoint = differentialNormalizer (leapPointL, iBox, true, false);
		Leap.Vector rightPoint = differentialNormalizer (leapPointR, iBox, false, false);
		
		float appXR = rightPoint.x * appWidth;
		float appYR = rightPoint.y * appHeight;

		float appXL = leftPoint.x * appWidth;
		float appYL = leftPoint.y * appHeight;

		//The z-coordinate is not used

		GUITexture[] _2dhands = p_debug_ui.GetComponentsInChildren<GUITexture>();
		foreach (GUITexture gt in _2dhands) {
			if(gt.name == "RightHand"){
				gt.pixelInset = new Rect(appXR, appYR, gt.pixelInset.width, gt.pixelInset.height);
			}else if(gt.name == "LeftHand"){
				gt.pixelInset = new Rect(appXL, appYL, gt.pixelInset.width, gt.pixelInset.height);
			}
		}

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
			}
		}
	}
}
