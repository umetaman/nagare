using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction{
		TOP,
		BOTTOM,
		LEFT,
		RIGHT,
		FRONT,
		BACK, 
		NULL
}

public class WaterTank : MonoBehaviour {

	public WorldData WorldData;

	float Width;
	float Height;
	float Depth;

	// Use this for initialization
	void Start () {
		Transform t = this.gameObject.GetComponent<Transform>();
		Width = t.localScale.x * WorldData.BasicScale;
		Height = t.localScale.y * WorldData.BasicScale;
		Depth = t.localScale.z * WorldData.BasicScale;

		Debug.Log("WaterTank------------");
		Debug.Log("Width: " + Width);
		Debug.Log("Height: " + Height);
		Debug.Log("Depth: " + Depth);
	}

	public float GetWidth(){
		return Width;
	}

	public float GetHeight(){
		return Height;
	}

	public float GetDepth(){
		return Depth;
	}

	public Vector3 GetPosition(){
		return this.GetComponent<Transform>().position;
	}

	public Vector3 GetScale(){
		return this.GetComponent<Transform>().localScale;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public Direction DetectDirection(Vector3 _p){
		Vector3 Pos = GetPosition();

		if(Pos.y + Height / 2 + WorldData.FieldOffSet < _p.y){
			return Direction.TOP;
		}
		else if(Pos.y - Height / 2 - WorldData.FieldOffSet > _p.y){
			return Direction.BOTTOM;
		}
		else if(Pos.x - Width / 2 - WorldData.FieldOffSet > _p.x){
			return Direction.LEFT;
		}
		else if(Pos.x + Width / 2 + WorldData.FieldOffSet < _p.x){
			return Direction.RIGHT;
		}
		else if(Pos.z - Depth / 2 - WorldData.FieldOffSet > _p.z){
			return Direction.FRONT;
		}
		else if(Pos.z + Depth / 2 + WorldData.FieldOffSet < _p.z){
			return Direction.BACK;
		}

		return Direction.NULL;
	}
}
