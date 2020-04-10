using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthIndicator : MonoBehaviour {

    public Transform healthIndicator;

    float originalScaleX;
    Enemy enemy;

	void Start () {
        originalScaleX = healthIndicator.localScale.x;
        enemy = GetComponent<Enemy>();
	}
	
	void Update () {
        float scaleX = enemy.health * originalScaleX / enemy.maxHealth;
        healthIndicator.localScale = new Vector2(scaleX, healthIndicator.localScale.y);
	}
}
