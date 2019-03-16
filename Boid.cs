using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour {

    public Transform Transform;
	float MaxSpeed = 0.3f;
	float MaxForce = 0.08f;
	float DesiredSeparate = 6.0f;
	float NeighborDist = 10.0f;
    Weight Weights;

	public Vector3 Acceleration;
	public Vector3 Velocity;
	public WorldData WorldData;

	WaterTank Field;

	Material[] BodyMat;

    public void SetWeights(Weight _Weights)
    {
        Weights = _Weights;
    }

	public void SetField(WaterTank _Field){
		Field = _Field;
	}

	public void SetMaxSpeed(float _speed){
		MaxSpeed = _speed;
	}

	public void SetMaxForce(float _force){
		MaxForce = _force;
	}

	public void SetDesiredSeparate(float _ds){
		DesiredSeparate = _ds;
	}

	public void SetNeighborDist(float _nd){
		NeighborDist = _nd;
	}

    //修正
	Vector3 LimitVector(Vector3 _v, float _Max){
        if(_v.magnitude > _Max){
            _v.Normalize();
            _v *= _Max;
        }

        return _v;
	}

	Vector3 PreVector;

	// Use this for initialization
	void Start () {
		Transform = this.gameObject.GetComponent<Transform>();

		this.Acceleration = new Vector3(0,0,0);
		this.Velocity = new Vector3(
			Random.Range(-2.0f, 2.0f),
            Random.Range(-2.0f, 2.0f),
            Random.Range(-2.0f, 2.0f)

        );

		PreVector = transform.position;

		StartCoroutine(RotateCoRoutine());

		BodyMat = this.gameObject.GetComponent<fish>().GetRenderer().materials;
	}
	
	// Update is called once per frame
	void Update () {
		if(!SpawnMode){
            Direction Dir = Field.DetectDirection(Transform.position);

            if (Dir != Direction.NULL){
				DeSpawn(Dir);
			}
		}
	}

	float yAngle;
	float zAngle;

	float p_yAngle;
	float p_zAngle;

	void RotateDirection(){
		Vector2 xyDim = new Vector2(
			transform.position.x - PreVector.x,
			transform.position.y - PreVector.y
		);

		zAngle = p_zAngle * 0.65f + Vector2.Angle(new Vector2(1,0), xyDim) * 0.35f;
		p_zAngle = zAngle;

		Vector2 xzDim = new Vector2(
			transform.position.x - PreVector.x,
			transform.position.z - PreVector.z
		);

		yAngle = p_yAngle * 0.65f +  Vector2.Angle(new Vector2(1,0), xzDim) * 0.35f;
		p_yAngle = yAngle;

        transform.rotation = Quaternion.Euler(0, yAngle + 180.0f, zAngle * 0.1f);

		Vector3 Rotate_v = new Vector3(yAngle, 0, zAngle);
	
		PreVector = transform.position;
	}

	IEnumerator RotateCoRoutine(){
		while(true){
			RotateDirection();
			yield return new WaitForSeconds(0.01f);
		}
	}

	public void Run(List<Boid> _Boids){
		this.Flock(_Boids);
		this.UpdatePosition();
	}

	public void ApplyForce(Vector3 _Force){
		this.Acceleration += _Force;
	}

    public void ApplyVelocity(Vector3 _Force){
        this.Velocity = _Force;
    }

	public void Flock(List<Boid> _Boids){
		var Sep = Separate(_Boids);
		var Ali = Align(_Boids);
		var Coh = Cohesion(_Boids);
        
        Sep *= Weights.Separate;
        Ali *= Weights.Align;
        Coh *= Weights.Cohesion;

		this.ApplyForce(Sep);
		this.ApplyForce(Ali);
		this.ApplyForce(Coh);
	}

	Vector3 PreVelocity;

	public void UpdatePosition(){
		this.Velocity += this.Acceleration;
		this.Velocity = LimitVector(this.Velocity, MaxSpeed);
		
		this.Velocity = 0.1f * PreVelocity + 0.9f * this.Velocity;
		
		this.Transform.position += this.Velocity;
		
		PreVelocity = this.Velocity;
		this.Acceleration *= 0;
	}

	public Vector3 Seek(Vector3 _Target){
		var Desired = _Target - this.Transform.position;
		Desired.Normalize();
		Desired *= MaxSpeed;

		var Steer = Desired - this.Velocity;
		Steer = LimitVector(Steer, MaxForce);
		return Steer;
	}

	public Vector3 Separate(List<Boid> _Boids){
		var Steer = new Vector3(0,0,0);
		int Count = 0;

		for(int i = 0; i < _Boids.Count; i++){
			var Dist = Vector3.Distance(
				this.Transform.position, _Boids[i].Transform.position
			);

			if((Dist > 0) && (Dist < DesiredSeparate)){
				var Diff = this.Transform.position - _Boids[i].Transform.position;
				Diff.Normalize();
				Diff /= Dist;
				Steer += Diff;
				Count++;
			}
		}

		if(Count > 0){
			Steer /= Count;
		}

		if(Steer.magnitude > 0){
			Steer.Normalize();
			Steer *= MaxSpeed;
			Steer -= this.Velocity;
			Steer = LimitVector(Steer, MaxForce);
			return Steer;
		}else{
			return new Vector3(0,0,0);
		}
	}

	public Vector3 Align(List<Boid> _Boids){
		var Sum = new Vector3(0,0,0);
		int Count = 0;

		for(int i = 0; i < _Boids.Count; i++){
			var Dist = Vector3.Distance(this.Transform.position, _Boids[i].Transform.position);

			if((Dist > 0) && (Dist < NeighborDist)){
				Sum += _Boids[i].Velocity;
				Count++;
			}
		}

		if(Count > 0){
			Sum /= Count;
			Sum.Normalize();
			Sum *= MaxSpeed;

			var Steer = Sum - this.Velocity;
			Steer = LimitVector(Steer, MaxForce);
			
			return Steer;
		}else{
			return new Vector3(0,0,0);
		}
	}

	public Vector3 Cohesion(List<Boid> _Boids){
		var Sum = new Vector3(0,0,0);
		int Count = 0;

		for(int i = 0; i < _Boids.Count; i++){
			var Dist = Vector3.Distance(this.Transform.position, _Boids[i].Transform.position);

			if((Dist > 0) && (Dist < NeighborDist)){
				Sum += _Boids[i].Transform.position;
				Count++;
			}
		}

		if(Count > 0){
			Sum /= Count;
			return this.Seek(Sum);
		}else{
			return new Vector3(0,0,0);
		}
	}

	//======================================
	// 魚が範囲外に出たとき
	//======================================

	bool SpawnMode = false;

	void UpdateMatAlpha(float _Value){
		for(int i = 0; i < BodyMat.Length; i++){
			Color c = BodyMat[i].color;
			c.a = _Value;
			BodyMat[i].color = c;
		}
	}

	void FadeIn(){
		Hashtable hash = new Hashtable();
		hash.Add("from", 0.0f);
		hash.Add("to", 1.0f);
		hash.Add("time", WorldData.FadeInTime);
		hash.Add("onupdate", "UpdateMatAlpha");
		hash.Add("oncompletetarget", this.gameObject);
		hash.Add("oncomplete", "SwitchSpawnMode");
		iTween.ValueTo(this.gameObject, hash);
	}

	void FadeOut(Direction _Direction){
		Hashtable hash = new Hashtable();
		hash.Add("from", 1.0f);
		hash.Add("to", 0.0f);
		hash.Add("time", WorldData.FadeOutTime);
		hash.Add("onupdate", "UpdateMatAlpha");
		hash.Add("oncompletetarget", this.gameObject);
		hash.Add("oncomplete", "ReSpawn");
        hash.Add("oncompleteparams", _Direction);
		iTween.ValueTo(this.gameObject, hash);

		SpawnMode = true; 
	}

	void DeSpawn(Direction _Direction){
		FadeOut(_Direction);
	}

	void ReSpawn(Direction _Direction){
		this.Acceleration = new Vector3(0,0,0);
		this.Velocity = new Vector3(0,0,0);
        //出現位置の決定
        //フェードイン
        //ベクトルの決定

        Vector3 Pos = Field.GetPosition();
        Vector3 AddVector = new Vector3(0, 0, 0);

        //DeSpawnした位置
        //switch (_Direction)
        //{
        //    case Direction.TOP:
        //        Transform.position = new Vector3(
        //            Random.Range(Pos.x - Field.GetWidth() / 2, Pos.x + Field.GetWidth() / 2),
        //            Pos.y - Field.GetHeight() / 2,
        //            Random.Range(Pos.z - Field.GetDepth() / 2, Pos.z + Field.GetDepth() / 2)
        //        );
        //        AddVector = new Vector3(
        //            0.0f, Random.Range(0.1f, 0.5f), 0.0f    
        //        );
        //        break;

        //    case Direction.BOTTOM:
        //        Transform.position = new Vector3(
        //            Random.Range(Pos.x - Field.GetWidth() / 2, Pos.x + Field.GetWidth() / 2),
        //            Pos.y + Field.GetHeight() / 2,
        //            Random.Range(Pos.z - Field.GetDepth() / 2, Pos.z + Field.GetDepth() / 2)
        //        );
        //        AddVector = new Vector3(
        //            0.0f, -Random.Range(0.1f, 0.5f), 0.0f
        //        );
        //        break;

        //    case Direction.LEFT:
        //        Transform.position = new Vector3(
        //            Pos.x + Field.GetWidth() / 2,
        //            Random.Range(Pos.y - Field.GetHeight() / 2, Pos.y + Field.GetHeight() / 2),
        //            Random.Range(Pos.z - Field.GetDepth() / 2, Pos.z + Field.GetDepth() / 2)
        //        );
        //        AddVector = new Vector3(
        //            Random.Range(0.1f, 0.5f), 0.0f, 0.0f
        //        );
        //        break;

        //    case Direction.RIGHT:
        //        Transform.position = new Vector3(
        //            Pos.x - Field.GetWidth() / 2,
        //            Random.Range(Pos.y - Field.GetHeight() / 2, Pos.y + Field.GetHeight() / 2),
        //            Random.Range(Pos.z - Field.GetDepth() / 2, Pos.z + Field.GetDepth() / 2)
        //        );
        //        AddVector = new Vector3(
        //            -Random.Range(0.1f, 0.5f), 0.0f, 0.0f
        //        );
        //        break;

        //    case Direction.FRONT:
        //        Transform.position = new Vector3(
        //            Random.Range(Pos.x - Field.GetWidth() / 2, Pos.x + Field.GetWidth() / 2),
        //            Random.Range(Pos.y - Field.GetHeight() / 2, Pos.y + Field.GetHeight() / 2),
        //            Pos.z - Field.GetDepth() / 2
        //        );
        //        AddVector = new Vector3(
        //            0.0f, 0.0f, Random.Range(0.1f, 0.5f)
        //        );
        //        break;

        //    case Direction.BACK:
        //        Transform.position = new Vector3(
        //            Random.Range(Pos.x - Field.GetWidth() / 2, Pos.x + Field.GetWidth() / 2),
        //            Random.Range(Pos.y - Field.GetHeight() / 2, Pos.y + Field.GetHeight() / 2),
        //            Pos.z + Field.GetDepth() / 2
        //        );
        //        AddVector = new Vector3(
        //            0.0f, 0.0f, -Random.Range(0.1f, 0.5f)
        //        );
        //        break;

        //    case Direction.NULL:
        //        Transform.position = new Vector3(
        //            Random.Range(Pos.x - Field.GetWidth() / 2, Pos.x + Field.GetWidth() / 2),
        //            Random.Range(Pos.y - Field.GetHeight() / 2, Pos.y + Field.GetHeight() / 2),
        //            Random.Range(Pos.z - Field.GetDepth() / 2, Pos.z + Field.GetDepth() / 2)
        //        );
        //        AddVector = new Vector3(
        //            Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)
        //        );
        //        break;
        //}

        
        Transform.position = new Vector3(
            Random.Range(Pos.x - Field.GetWidth() / 4, Pos.x + Field.GetWidth() / 4),
            Random.Range(Pos.y - Field.GetHeight() / 4, Pos.y + Field.GetHeight() / 4),
            Pos.z + Field.GetDepth() / 2
        );
        AddVector = new Vector3(
            0.0f, 0.0f, -Random.Range(0.1f, 0.5f)
        );
        
        FadeIn();
        ApplyForce(AddVector);

	}

	void SwitchSpawnMode(){
		SpawnMode = !SpawnMode;
	}
}
