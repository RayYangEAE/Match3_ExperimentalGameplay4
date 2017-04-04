using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformationExtension {

	public static IEnumerator Move(this Transform t, Vector3 target, float duration){
		Vector3 diffVector = target - t.position;
		float diffLength = diffVector.magnitude;
		diffVector.Normalize ();
		float timeCount = 0;
		while (timeCount < duration) {
			float moveAmount = (Time.deltaTime * diffLength / duration);
			t.position += diffVector * moveAmount;
			timeCount += Time.deltaTime;
			yield return null;
		}
		t.position = target;
	}

}
