using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using UnityEngine;

public class CollisionSound : MonoBehaviour
{
    private GameObject[] gameObjects;

    private GameObject player;

    private readonly HashSet<string> collisionTracker = new HashSet<string>();

    public static StringBuilder collisions = new StringBuilder();

    public static int counter;

    private const float MAX_DISTANCE = 2.5f;

    private const float MIN_DISTANCE = 1.2f;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("PlayerController");

        var components = GetComponentsInChildren<AudioSource>();
        gameObjects = Array.ConvertAll(components, c => c.gameObject);

        foreach (var component in components)
        {
            component.spatialBlend = 1f;
            component.minDistance = 1.2f;
            component.maxDistance = 2.5f;
            component.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!SimpleCapsuleWithStickMovement.isMeasuring)
        {
            return;
        }

        var p = player.transform.position;

        foreach (var gameObject in gameObjects)
        {
            var collider = gameObject.GetComponents<Collider>().FirstOrDefault(c => !c.isTrigger);

            if (collider == null)
            {
                continue;
            }

            var distance = Vector3.Distance(collider.ClosestPoint(p), p);

            if (distance > MAX_DISTANCE && collisionTracker.Contains(gameObject.name))
            {
                collisionTracker.Remove(gameObject.name);
            }
            else if (distance <= MIN_DISTANCE && collisionTracker.Add(gameObject.name))
            {
                collisions.AppendLine($"Collision with {gameObject.name}");
                counter++;
            }
        }
    }
}
