﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (BoxCollider2D))] 
public class CollisionObject : MonoBehaviour {

	//Usually, I let my character controller handle the collision checks, so that it can react, but you can set this to 
	//true if you want it to happen automatically.
	public bool autoCheckForCollisionsOnMovement = false;
	private Vector3 lastPosition;

	//This is where we will store our active collisions so that we're not sending messages every frame
	Dictionary<GameObject, bool> activeCollisions = new Dictionary<GameObject, bool>();
	
	void OnStart()
	{
		// This is storing your position so that it knows when this object has moved and needs to recheck.
		// We check collisions on start so that we know if we have collisions on the first frame before it moves.
		lastPosition = transform.position;

		if ( autoCheckForCollisionsOnMovement ) 
		{
			gameObject.CollidingWithTrackedColliders();
		}
	}

	void OnEnable() {

		// Add to collision tracker list inside the Aabb static class when it's enabled/re-enabled.
		gameObject.TrackMyCollisions();

		CheckStatusOfCollisions();
	}

	void OnDisable() {

		// This will remove from collision tracker list inside the Aabb static class when the object is disabled.
		// When an object is destroyed, it disables itself first.
		Debug.LogWarning( "Untracking myself: " + gameObject.name );
		gameObject.UntrackMyCollisions();

	}
	
	public void CollisionTracker ( GameObject other )
	{
		//Check to see if this collision was already happening
		if ( !activeCollisions.ContainsKey( other ))
		{
			// Add this object to our list of active collisions if it's new.
			activeCollisions.Add ( other, true );

			// Let all the scripts do stuff based on this collision
			gameObject.SendMessage ( "AabbCollisionEnter", other, SendMessageOptions.DontRequireReceiver );

		}

	}

	void LateUpdate() {

		//Cleans up the local active collision list at the end of the frame
		CheckStatusOfCollisions();

		// This is for checking collisions automatically every frame, if you've marked that as true.
		if ( autoCheckForCollisionsOnMovement && transform.position != lastPosition ) 
		{
			lastPosition = transform.position;
			gameObject.CollidingWithTrackedColliders();
		}

	}

	private void CheckStatusOfCollisions()
	{
		// Does a check on existing collisions. If one doesn't exist, take it out of the dictionary.
		if (activeCollisions.Count > 0)
		{
			Dictionary<GameObject, bool> activeCollisionsCopy = new Dictionary<GameObject, bool>(activeCollisions);

			foreach (GameObject o in activeCollisionsCopy.Keys)
			{
				if (!gameObject.CollidingWith( o ))
				{
					if ( activeCollisions.ContainsKey( o ))
						activeCollisions.Remove( o );
				}
			}

		}

	}

	//This method goes into the other scripts that need to know when a collision has happened.
	private void AabbCollisionEnter ( GameObject other )
	{
		//This is debug line you can uncomment to test collisions.
		//Debug.LogWarning( gameObject.name + " collided with " + other.name + "." );
	}
}
