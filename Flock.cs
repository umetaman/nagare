using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour {

	public List<Boid> Boids = new List<Boid>();

	public void AddBoid(GameObject _b){
		Boid b = _b.GetComponent<Boid>();
		Boids.Add(b);
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		for(int i = 0; i < this.Boids.Count; i++){
			this.Boids[i].Run(this.Boids);
		}
	}

	public void ApplyForce(Vector3 _v){
		foreach(var Boid in Boids){
			Boid.ApplyForce(_v);
		}
	}

	public List<Boid> GetBoids(){
		return  this.Boids;
	}
}
