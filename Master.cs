using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Master : MonoBehaviour, OnSwingHandler, OnWaveHandler {

	public GameObject FlockPrefab;
	List<Flock> Flocks = new List<Flock>();

	public WorldData WorldData;
	public WaterTank Field;

    public Transform[] BubbleObj = new Transform[2];

    public LeapControllerManager LeapControl;
    public LoadAnimation StandByAnimation;

	// Use this for initialization
	void Start () {

        //フレームレート
        Application.targetFrameRate = 60;

        //マウスカーソルの非表示
        Cursor.visible = false;

        //魚の生成
        for (int k = 0; k < 3; k++)
        {

            for (int j = 0; j < WorldData.Fish.Count; j++)
            {

                GameObject Flock = Instantiate(FlockPrefab);

                for (int i = 0; i < WorldData.FishNum; i++)
                {

                    GameObject Boid = Instantiate(WorldData.Fish[j].Prefab);
                    Boid.transform.SetParent(Flock.transform);

                    Boid b = Boid.GetComponent<Boid>();
                    b.SetMaxSpeed(WorldData.Fish[j].MaxSpeed);
                    b.SetWeights(WorldData.Fish[j].Weights);
                    b.SetDesiredSeparate(WorldData.Fish[j].DesiredSeparate);
                    b.SetNeighborDist(WorldData.Fish[j].NeighborDist);
                    b.SetField(Field);

                    fish Fish = Boid.GetComponent<fish>();
                    Fish.SetfinSpeed(Random.Range(0.5f, 1.0f));
                    Fish.SetCycleOffset(Random.Range(0.1f, 1.0f));
                    Fish.SetMaterialFin(WorldData.Fish[j].FinColor);
                    Fish.SetMaterialBody(WorldData.Fish[j].BodyColor);

                    Transform FishBody = Boid.GetComponent<Transform>();
                    FishBody.localScale = new Vector3(1, 1, 1) * Random.Range(3.0f, 7.0f);

                    Vector3 fp = Field.GetPosition();

                    FishBody.localPosition = new Vector3(0, 0, 0);

                    Flock.GetComponent<Flock>().AddBoid(Boid);

                }

                Flocks.Add(Flock.GetComponent<Flock>());

            }
        }

        IEnumerator StandByCoRoutine = WaitLeapHandCoRoutine();

        StartCoroutine(StandByCoRoutine);
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

	}

	public List<Flock> GetFlocks(){
		return Flocks;
	}

    public float DistLimit = 30.000f;

    struct BoidDist
    {
        public Boid Boid;
        public float Distance;
        public Vector3 DirectionToHand;

        public void SetBoidDist(Boid _Boid, float _Distance, Vector3 _Direction)
        {
            Boid = _Boid;
            Distance = _Distance;
            DirectionToHand = _Direction;
        }
    }


    //指定した距離内にある魚のリストを返す, 距離順
    List<BoidDist> CalcurateBoidDistance(Vector3 _Point)
    {
        var BoidList = new List<BoidDist>();

        foreach(var Flock in Flocks)
        {
            foreach(var Boid in Flock.GetBoids())
            {
                float Dist = Vector3.Distance(Boid.transform.position, _Point);
                if(Dist < DistLimit)
                {
                    BoidDist b = new BoidDist();
                    Vector3 Dir = _Point - Boid.transform.position;
                    Dir.Normalize();
                    b.SetBoidDist(Boid, Dist, Dir);
                    BoidList.Add(b);
                }
            }
        }

        //距離が短い順に並べ替え
        var tmp =  new BoidDist();

        for(int i = 0; i < BoidList.Count; i++)
        {
            for(int j = i + 1; j < BoidList.Count; j++)
            {
                if(BoidList[i].Distance > BoidList[j].Distance)
                {
                    tmp = BoidList[i];
                    BoidList[i] = BoidList[j];
                    BoidList[j] = tmp;
                }
            }
        }

        return BoidList;
    }

    IEnumerator InduceCoRoutine(List<BoidDist> _BoidList, Vector3 _Direction)
    {
        _Direction.Normalize();

        foreach(var _Boid in _BoidList)
        {
            Boid b = _Boid.Boid;
            float Dist = _Boid.Distance;

            float Scale = Mathf.Lerp(5.0000f, 0.0001f, Dist / DistLimit);
            b.ApplyForce(Scale * _Direction);
            
            yield return new WaitForSeconds(0.002f);
        }

    }

    IEnumerator DissolveCoRoutine(List<BoidDist> _BoidList)
    {
        foreach(var _Boid in _BoidList)
        {
            Boid b = _Boid.Boid;
            float Dist = _Boid.Distance;
            Vector3 Dir = _Boid.DirectionToHand;

            float Scale = Mathf.Lerp(5.0000f, 0.0001f, Dist / DistLimit);
            b.ApplyVelocity(Scale * Dir * -5.0f);

            yield return new WaitForSeconds(0.001f);
        }
    }

    public void InduceInOrder(Vector3 _HandPosition, Vector3 _Direction)
    {
        var Boids = CalcurateBoidDistance(_HandPosition);
        StartCoroutine(InduceCoRoutine(Boids, _Direction));
    }

    void DissolveInOrder(Vector3 _HandPosition)
    {
        var Boids = CalcurateBoidDistance(_HandPosition);
        StartCoroutine(DissolveCoRoutine(Boids));
    }

    public void OnSwing(int id, Vector3 beginPoint, Vector3 currentPoint, Vector3 velocity)
    {
        Vector3 v = new Vector3(-velocity.x, velocity.y * 0.2f, -velocity.z);
        InduceInOrder(BubbleObj[id].transform.position, v);
        Debug.Log("Induce");
        Debug.Log(velocity);
    }

    public void OnWave(int id, Vector3 currentPoint)
    {
        DissolveInOrder(BubbleObj[id].transform.position);
        Debug.Log("Dissolve");
        Debug.Log(currentPoint);
    }

    //LeapMotionを待ち続ける
    IEnumerator WaitLeapHandCoRoutine()
    {
        while (true)
        {
            var LeapHand = LeapControl.GetHandList();

            bool InteractLeap = false;

            for (int i = 0; i < LeapHand.Count; i++)
            {
                if (LeapHand[i] != null && !InteractLeap)
                {
                    InteractLeap = true;
                }
            }

            if (InteractLeap)
            {
                StandByAnimation.EndAnimation();
                //yield return new WaitForSeconds(2.0f);
            }
            else
            {
                if (!StandByAnimation.GetisLoop())
                {
                    StandByAnimation.StartAnimation();
                }
                //yield return new WaitForSeconds(5.0f);
            }

            yield return new WaitForSeconds(2.0f);
        }
    }
}
