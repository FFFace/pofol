﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterWeapon : MonoBehaviour
{
    private Character character;
    private float damage;
    private void Start()
    {
        character = Character.instance;
    }

    public void SetDamage(float _damage)
    {
        damage = _damage;
    }

    public void SetActiveCollider(bool active)
    {
        GetComponent<Collider>().enabled = active;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Character"))
        {
            Vector3 dir = other.transform.position - transform.position;
            dir.y = 0;
            character.Hit(damage, dir.normalized);
            GetComponent<Collider>().enabled = false;
        }
    }
}
