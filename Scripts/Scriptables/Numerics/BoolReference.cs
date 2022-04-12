using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BoolReference //Use to reference a float variable
{
	public bool useConstant;
	public bool constant;
	public BoolVariable var;

	public bool Value
	{
		get { return useConstant ? constant : var.value; }
	}
}
