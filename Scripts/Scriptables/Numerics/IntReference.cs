using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class IntReference //Use to reference a int variable
{
	public bool useConstant;
	public int constant;
	public IntVariable var;

	public int Value
	{
		get { return useConstant ? constant : var.value; }
	}
}