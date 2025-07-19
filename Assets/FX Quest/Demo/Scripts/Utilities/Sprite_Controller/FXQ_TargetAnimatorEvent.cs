// FX Quest
// Version: 0.5.5
// Compatilble: Unity 5.5.1 or higher, see more info in Readme.txt file.
//
// Developer:			Gold Experience Team (https://www.assetstore.unity3d.com/en/#!/search/page=1/sortby=popularity/query=publisher:4162)
// Unity Asset Store:	https://www.assetstore.unity3d.com/en/#!/content/21073
//
// Please direct any bugs/comments/suggestions to geteamdev@gmail.com

#region Namespaces

using UnityEngine;
using System.Collections;

#endregion // Namespaces

// ######################################################################
// FXQ_TargetAnimatorEvent class
// Stores behavior information when particle played
// 
// Note this class is used in FXQ_2D_Demo.UpdateTargetAnimator() function.
// ######################################################################
public class FXQ_TargetAnimatorEvent : MonoBehaviour
{

	// ########################################
	// Variables
	// ########################################

	#region Variables

	// Type of particle
	public enum ParticleEvent
	{
		None,
		Attack,
		UI
	}

	public Animator m_TargetAnimator = null;
	public ParticleEvent m_ParticleEvent = ParticleEvent.None;

	#endregion // Variables

}
