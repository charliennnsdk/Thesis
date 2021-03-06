﻿using UnityEngine;
using System.Collections;
using Parse;

public class RobotPanel : MonoBehaviour {

    public dfLabel UsernameLabel, LevelLabel, HealthLabel, MaxSpeedLabel;
    public dfSprite Avatar;
    public SelectWeaponScript SelectWeapon;

    Player Player
    {
        get
        {
            return ParseUser.CurrentUser as Player;
        }
    }

	// Use this for initialization
	void Start () {
        
        UsernameLabel.Text = Player.Username;
        LevelLabel.Text = Player.Level + "";
        Avatar.SpriteName = string.Format("spell-{0}", (3 + Player.Level));
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void ShowSelectWeapon(WeaponType type) 
    {
        SelectWeapon.ShowSelectWeapons(type);
    }
}
