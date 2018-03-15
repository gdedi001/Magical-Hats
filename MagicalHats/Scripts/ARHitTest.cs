using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

public class ARHitTest : MonoBehaviour {
	public Camera ARCamera; //the Virtual Camera used for AR
	public GameObject hitPrefab; //prefab we place on a hit test
	public GameObject Effect;

	private List<GameObject> spawnedObjects = new List<GameObject>(); //array used to keep track of spawned objects

	/// <summary>
	/// Function that is called on 
	/// NOTE: HIT TESTS DON'T WORK IN ARKIT REMOTE
	/// </summary>
	public void SpawnHitObject() {
		ARPoint point = new ARPoint { 
			x = 0.5f, //do a hit test at the center of the screen
			y = 0.5f
		};

		// prioritize result types
		ARHitTestResultType[] resultTypes = {
			//ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent, // if you want to use bounded planes
			//ARHitTestResultType.ARHitTestResultTypeExistingPlane, // if you want to use infinite planes 
			ARHitTestResultType.ARHitTestResultTypeFeaturePoint // if you want to hit test on feature points
		}; 

		foreach (ARHitTestResultType resultType in resultTypes) {
			if (HitTestWithResultType(point, resultType)) {
				return;
			}
		}
	}

	bool HitTestWithResultType (ARPoint point, ARHitTestResultType resultTypes) {
		List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface().HitTest(point, resultTypes);
		if (hitResults.Count > 0) {
			foreach (var hitResult in hitResults) {
				Vector3 hitPosition = UnityARMatrixOps.GetPosition(hitResult.worldTransform);
				Quaternion rotation = UnityARMatrixOps.GetRotation(hitResult.worldTransform);
				spawnedObjects.Add(Instantiate(hitPrefab, hitPosition, rotation));
				Instantiate (Effect, hitPosition, rotation); // spawn particle system
				return true;
			}
		}
		return false;
	}

	// Fixed Update is called once per frame
	void FixedUpdate () {
		if (Input.GetMouseButtonDown(0)) { //this works with touch as well as with a mouse
			RemoveObject (Input.mousePosition);
		}
	}

	public void RemoveObject(Vector2 point) {
		//TODO: Raycast from the screen point into the virtual world and see if we hit anything
		//if we do, then check to see if it is part of the spawnedObjects array
		//if so, then delete the object we raycast hit
		RaycastHit hit;
		if (Physics.Raycast(ARCamera.ScreenPointToRay(point), out hit)) {
			GameObject item = hit.collider.transform.parent.gameObject; // parent is what is stored in our area
			if (spawnedObjects.Remove (item)) {
				Destroy (item);
			}
		}

	}
		
	/// <summary>
	/// NOTE: A Function To Be Called When the Shuffle Button is pressed
	/// </summary>
	public void Shuffle(){
		StartCoroutine( ShuffleTime ( Random.Range(5, 10)) );
	}
		
	/// <summary>
	/// NOTE: A Co-routine that shuffles 
	/// </summary>
	IEnumerator ShuffleTime(int numSuffles) {
		//TODO:
		//iterate numShuffles times
		//pick two hats randomly from spawnedObject and call the Co-routine Swap with their Transforms
		GameObject hatA;
		GameObject hatB;
		for (int i = 0; i < numSuffles; i++) {
			hatA = spawnedObjects[Random.Range(0, spawnedObjects.Count)];
			hatB = spawnedObjects [Random.Range (0, spawnedObjects.Count)];
			yield return StartCoroutine(Swap (hatA.transform, hatB.transform, 1.0f));
		}
	}

	IEnumerator Swap(Transform item1, Transform item2, float duration){
		//Lerp the position of item1 and item2 so that they switch places
		//the transition should take "duration" amount of time
		//Optional: try making sure the hats do not collide with each other
		item1.position = Vector3.Lerp(item1.position, item2.position, Time.deltaTime * duration);
		item2.position = Vector3.Lerp(item2.position, item1.position, Time.deltaTime * duration);

		yield return null; //placeholder to make sure this compiles
	}
}