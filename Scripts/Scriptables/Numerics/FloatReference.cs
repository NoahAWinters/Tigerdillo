using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FloatReference //Use to reference a float variable
{
	public bool useConstant;
	public float constant;
	public FloatVariable var;

	public float Value
	{
		get { return useConstant ? constant : var.value; }
	}
}