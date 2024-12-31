using BepInEx;
using System.Reflection;
using UnityEngine;

[BepInPlugin("CiarenceW.Saiga9", "Saiga-9", "1.0.0")]
public class Saiga9Loader : BaseUnityPlugin
{
	public void Awake()
	{
		var bepinAttribute = this.GetType().GetCustomAttribute<BepInPlugin>();
		Logger.LogInfo($"{bepinAttribute.GUID} version {bepinAttribute.Version} loaded!");
	}
}
