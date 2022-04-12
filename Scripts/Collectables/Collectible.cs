using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
	[SerializeField] bool shouldDestroy; //If true, Destroy. Else set active to false. Depends on if they respawn
	[SerializeField] bool shouldRotate;
	
    [SerializeField] private IntVariable[] targetCounters; //Local, Overall


	void Update()
	{
		if(shouldRotate)
		{
			transform.Rotate(90 * Time.deltaTime, 0 , 0);
		}
	}

	//needs: Mesh, box collider or sphere collider
	void OnTriggerEnter(Collider coll)
	{
		if(coll.gameObject.tag == "Player")
		{
			//Add score to appropriate counter for individual level;
            //targetCounters[0].value += 1;
			//Add score to appropriate counter for overall tracker
            //targetCounters[1].value += 1;

			if(shouldDestroy)
			{
				Destroy(gameObject);
			}
			else
			{
				gameObject.SetActive(false);
			}
		}
	}
}
