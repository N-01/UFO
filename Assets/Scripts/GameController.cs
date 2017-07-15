using System.Collections;
using System.Collections.Generic;
using Logic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public EntityRenderer renderer;
	// Use this for initialization
	void Start ()
	{
	    var view1 = renderer.Create(new Asteroid(new FixedPointVector3(0, 0 , 0)));
	    var view2 = renderer.Create(new Asteroid(new FixedPointVector3(0, 0, 0)));

	    renderer.RecycleDelayed(view1, 3);
	    renderer.Create(new Asteroid(new FixedPointVector3(0, 0, 0)));
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
