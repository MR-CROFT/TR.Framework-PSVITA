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
using UnityEngine.UI;

#endregion // Namespaces

// ######################################################################
// FXQ_2D_Demo class
// This class handles user key inputs, play and stop all particle effects for 2D demo scene.
// 
// Up/Down Buttons to switch Particle type
// Left/Right Buttons to switch Particle.
// ######################################################################

public class FXQ_2D_Demo : MonoBehaviour
{

	// ########################################
	// Variables
	// ########################################

	#region Variables

	[System.Serializable]           // Embed this class with sub properties in the inspector. http://docs.unity3d.com/ScriptReference/Serializable.html
	public class FXQ_ParticleType
	{
		public Transform m_Particles;
		public Button m_Buttons;
	}

	// Elements
	public FXQ_ParticleType[] m_ParticleTypeList;

	// Index of current element
	int m_ParticleType = 0;
	int m_ParticleTypeOld = -1;
	int m_ParticleTypeChildCount = 0;

	// Index of current particle
	int m_ParticleIndex = 0;
	int m_ParticleIndexOld = -1;

	// Current and Old ParticleSystem
	ParticleSystem m_Particle = null;
	ParticleSystem m_ParticleOld = null;

	// Particle details
	string m_ParticleTypeName = "";
	string m_ParticleName = "";

	// Canvas
	public Canvas m_2DDemo_UI = null;

	// SelectDemo
	public Button m_SelectDemo_Button = null;
	public GameObject m_SelectDemo_Window = null;

	// Options
	public GameObject m_Options_Window = null;
	public Toggle m_Options_Toggle_ShowSprite = null;
	public Toggle m_Options_Toggle_DarkScreen = null;

	// Help
	public Button m_Help_Button = null;
	public GameObject m_Help_Window = null;

	// Particle Selection
	public GameObject m_ParticleSelection_Window = null;

	// Particle Details
	public Text m_ParticleDetails_Text_Order = null;
	public Button m_ParticleDetails_Button_ParticleName = null;
	public Text m_ParticleDetails_Text_Name = null;

	// HowTo
	public GameObject m_HowTo = null;

	// Target Animator Events
	public FXQ_TargetAnimatorEvent m_TargetAnimatorEvent_Current = null;
	public FXQ_TargetAnimatorEvent m_TargetAnimatorEvent_Old = null;

	// Toggles
	public GameObject[] m_SpriteToToggle = null;
	public GameObject[] m_DarkScreenToToggle = null;

	// Screen resolutions
	int m_DefWidth;
	int m_DefHeight;

	#endregion // Variables

	// ########################################
	// MonoBehaviour Functions
	// http://docs.unity3d.com/ScriptReference/MonoBehaviour.html
	// ########################################

	#region MonoBehaviour

	// Awake is called when the script instance is being loaded.
	// http://docs.unity3d.com/ScriptReference/MonoBehaviour.Awake.html
	void Awake()
	{
		// Set GUIAnimSystemFREE.Instance.m_AutoAnimation to false, 
		// this will let you control all GUI Animator elements in the scene via scripts.
		if (enabled)
		{
			GUIAnimSystemFREE.Instance.m_GUISpeed = 4.0f;
			GUIAnimSystemFREE.Instance.m_AutoAnimation = false;
		}
	}

	// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
	// http://docs.unity3d.com/ScriptReference/MonoBehaviour.Start.html
	void Start()
	{

		// Store current screen resolution
		m_DefWidth = Screen.width;
		m_DefHeight = Screen.height;

		// Check if there is any particle in prefab list
		if (m_ParticleTypeList.Length > 0)
		{
			// reset indices of element and particle
			m_ParticleType = 0;
			m_ParticleTypeOld = -1;
			m_ParticleIndex = 0;
			m_ParticleIndexOld = -1;
		}

		// Play UI move-in animations
		StartCoroutine(ShowUIs());
	}

	// Update is called every frame, if the MonoBehaviour is enabled.
	// http://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html
	void Update()
	{
		// User released Up arrow key
		if (Input.GetKeyUp(KeyCode.UpArrow))
		{
			NextParticleType();
		}
		// User released Down arrow key
		else if (Input.GetKeyUp(KeyCode.DownArrow))
		{
			PreviousParticleType();
		}
		// User released Left arrow key
		else if (Input.GetKeyUp(KeyCode.LeftArrow))
		{
			PreviousParticle();
		}
		// User released Right arrow key
		else if (Input.GetKeyUp(KeyCode.RightArrow))
		{
			NextParticle();
		}
		// User released Enter key
		else if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
		{
			// Show particle
			ShowParticle();
		}
	}

	#endregion // MonoBehaviour

	// ########################################
	// Show UI functions
	// ########################################

	#region Show UI

	// Play UI move-in animations
	IEnumerator ShowUIs()
	{

		// Disable all buttons of m_2DDemo_UI
		// http://docs.unity3d.com/Manual/script-GraphicRaycaster.html
		GUIAnimSystemFREE.Instance.SetGraphicRaycasterEnable(m_2DDemo_UI, false);

		yield return new WaitForSeconds(0.25f);

		GUIAnimSystemFREE.Instance.MoveIn(m_Options_Window.transform, true);

		yield return new WaitForSeconds(0.25f);

		GUIAnimSystemFREE.Instance.MoveIn(m_ParticleDetails_Button_ParticleName.transform, true);

		yield return new WaitForSeconds(0.5f);

		GUIAnimSystemFREE.Instance.MoveIn(m_ParticleSelection_Window.transform, true);
		GUIAnimSystemFREE.Instance.MoveIn(m_HowTo.transform, true);

		yield return new WaitForSeconds(0.25f);

		GUIAnimSystemFREE.Instance.MoveIn(m_SelectDemo_Button.transform, true);
		GUIAnimSystemFREE.Instance.MoveIn(m_Help_Button.transform, true);

		// Enable all buttons of m_2DDemo_UI
		// http://docs.unity3d.com/Manual/script-GraphicRaycaster.html
		GUIAnimSystemFREE.Instance.SetGraphicRaycasterEnable(m_2DDemo_UI, true);

		ShowParticle();
		SetParticleType(0);

		UpdateToggleSpriteAndDarkScreen();
	}

	#endregion // Show UI

	// ########################################
	// Particle functions
	// ########################################

	#region Particle

	// Remove old Particle and do Create new Particle GameObject
	void ShowParticle()
	{
		// Keep m_ParticleType between 0 to m_ParticleTypeList.Length-1
		if (m_ParticleType >= m_ParticleTypeList.Length)
		{
			m_ParticleType = 0;
		}
		else if (m_ParticleType < 0)
		{
			m_ParticleType = m_ParticleTypeList.Length - 1;
		}

		int index = 0;
		if (m_ParticleType != m_ParticleTypeOld)
		{
			// Disable all m_ParticleTypeList[m_ParticleTypeOld]
			if (m_ParticleTypeOld >= 0)
			{
				index = 0;
				foreach (Transform child in m_ParticleTypeList[m_ParticleTypeOld].m_Particles)
				{
					ParticleSystem pParticleSystem = child.gameObject.GetComponent<ParticleSystem>();
					if (pParticleSystem != null)
					{
						pParticleSystem.Stop();
						pParticleSystem.gameObject.SetActive(false);
					}

					index++;
				}
			}

			// Disable all m_ParticleTypeList[m_ParticleType]
			if (m_ParticleType >= 0)
			{
				index = 0;
				foreach (Transform child in m_ParticleTypeList[m_ParticleType].m_Particles)
				{
					ParticleSystem pParticleSystem = child.gameObject.GetComponent<ParticleSystem>();
					if (pParticleSystem != null)
					{
						pParticleSystem.Stop();
						pParticleSystem.gameObject.SetActive(false);
					}

					index++;
				}
			}

			if (m_ParticleTypeOld >= 0)
			{
				m_ParticleTypeList[m_ParticleTypeOld].m_Particles.gameObject.SetActive(false);
			}
			if (m_ParticleType >= 0)
			{
				m_ParticleTypeList[m_ParticleType].m_Particles.gameObject.SetActive(true);
			}

			m_ParticleTypeName = m_ParticleTypeList[m_ParticleType].m_Particles.name;
			m_ParticleTypeChildCount = m_ParticleTypeList[m_ParticleType].m_Particles.childCount;
		}

		// Keep m_ParticleIndex between 0 to m_ParticleTypeChildCount-1
		if (m_ParticleIndex >= m_ParticleTypeChildCount)
		{
			m_ParticleIndex = 0;
		}
		else if (m_ParticleIndex < 0)
		{
			m_ParticleIndex = m_ParticleTypeChildCount - 1;
		}

		// Play ParticleSystem
		if (m_ParticleIndex != m_ParticleIndexOld || m_ParticleType != m_ParticleTypeOld)
		{
			// Disable Old particle
			if (m_Particle != null)
			{
				m_Particle.Stop();
				m_Particle.gameObject.SetActive(false);
			}

			index = 0;
			foreach (Transform child in m_ParticleTypeList[m_ParticleType].m_Particles)
			{
				if (index == m_ParticleIndex)
				{
					m_ParticleOld = m_Particle;

					// Keep old TargetAnimatorEvent
					if (m_ParticleOld != null)
						m_TargetAnimatorEvent_Old = m_ParticleOld.gameObject.GetComponent<FXQ_TargetAnimatorEvent>();

					// Play current paritcle
					m_Particle = child.gameObject.GetComponent<ParticleSystem>();
					if (m_Particle != null)
					{
						m_Particle.gameObject.SetActive(true);
						m_Particle.Play();

						m_ParticleName = m_Particle.name;
						m_ParticleDetails_Text_Name.text = " (" + (m_ParticleIndex + 1) + " / " + m_ParticleTypeChildCount + ")  " + m_ParticleName;
						m_ParticleDetails_Text_Order.text = m_ParticleTypeName;

						// Keep new TargetAnimatorEvent
						m_TargetAnimatorEvent_Current = m_Particle.gameObject.GetComponent<FXQ_TargetAnimatorEvent>();

						UpdateTargetAnimator();
					}
					break;
				}

				index++;
			}
		}
	}

	// Show/Hide Sprites and play hurt animation of adamantine 
	void UpdateTargetAnimator()
	{
		if (m_TargetAnimatorEvent_Old != null)
		{
			if (m_TargetAnimatorEvent_Old != m_TargetAnimatorEvent_Current)
			{
				m_TargetAnimatorEvent_Old.m_TargetAnimator.gameObject.SetActive(false);
			}
		}

		if (m_TargetAnimatorEvent_Current == null)
			return;

		if (m_TargetAnimatorEvent_Current.m_TargetAnimator.gameObject.activeSelf == false && m_Options_Toggle_ShowSprite.isOn == true)
		{
			m_TargetAnimatorEvent_Current.m_TargetAnimator.gameObject.SetActive(true);
		}

		if (m_TargetAnimatorEvent_Current.m_ParticleEvent == FXQ_TargetAnimatorEvent.ParticleEvent.None)
		{

		}
		else if (m_TargetAnimatorEvent_Current.m_ParticleEvent == FXQ_TargetAnimatorEvent.ParticleEvent.Attack)
		{
			if (m_TargetAnimatorEvent_Current.m_TargetAnimator.gameObject != null)
			{
				if (m_TargetAnimatorEvent_Current.m_TargetAnimator.gameObject.activeSelf == true)
				{
					m_TargetAnimatorEvent_Current.m_TargetAnimator.gameObject.SendMessage("UnderAttack", SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		else if (m_TargetAnimatorEvent_Current.m_ParticleEvent == FXQ_TargetAnimatorEvent.ParticleEvent.UI)
		{
		}
	}

	// Show/Hide Sprite and DarkScreen
	void UpdateToggleSpriteAndDarkScreen()
	{
		// Toggle show/hide sprites in m_SpriteToToggle
		if (m_Options_Toggle_ShowSprite.isOn == true)
		{
			foreach (GameObject child in m_SpriteToToggle)
			{
				if (child == m_TargetAnimatorEvent_Current.m_TargetAnimator)
				{
					child.SetActive(m_Options_Toggle_ShowSprite.isOn);
				}
				else
				{
					child.SetActive(false);
				}
			}
			UpdateTargetAnimator();
		}
		else
		{
			foreach (GameObject child in m_SpriteToToggle)
			{
				child.SetActive(false);
			}
		}

		// Toggle show/hide sprites in m_DarkScreenToToggle
		foreach (GameObject child in m_DarkScreenToToggle)
		{
			if (child.activeSelf != m_Options_Toggle_DarkScreen.isOn)
				child.SetActive(m_Options_Toggle_DarkScreen.isOn);
		}
	}

	// Set current particle type to a given type
	void SetParticleType(int ParticleType)
	{
		m_ParticleTypeOld = m_ParticleType;
		m_ParticleType = ParticleType;

		m_ParticleIndexOld = m_ParticleIndex;
		m_ParticleIndex = 0;

		UpdateButtonParticleType();

		// Show particle
		ShowParticle();
	}

	// Switch particle type to previous and play its first particle
	void NextParticleType()
	{
		m_ParticleTypeOld = m_ParticleType;
		m_ParticleType++;
		m_ParticleIndexOld = m_ParticleIndex;
		m_ParticleIndex = 0;
		if (m_ParticleType >= m_ParticleTypeList.Length)
			m_ParticleType = 0;

		UpdateButtonParticleType();

		// Show particle
		ShowParticle();
	}

	// Switch particle type to next and play its first particle
	void PreviousParticleType()
	{
		m_ParticleTypeOld = m_ParticleType;
		m_ParticleType--;
		m_ParticleIndexOld = m_ParticleIndex;
		m_ParticleIndex = 0;
		if (m_ParticleType < 0)
			m_ParticleType = m_ParticleTypeList.Length - 1;

		UpdateButtonParticleType();

		// Show particle
		ShowParticle();
	}

	// Show next particle
	void NextParticle()
	{
		m_ParticleIndexOld = m_ParticleIndex;
		m_ParticleIndex++;

		// Show particle
		ShowParticle();
	}

	// Show previous particle
	void PreviousParticle()
	{
		m_ParticleIndexOld = m_ParticleIndex;
		m_ParticleIndex--;

		// Show particle
		ShowParticle();
	}

	// Re-scale the Particle type buttons
	void UpdateButtonParticleType()
	{
		for (int i = 0; i < m_ParticleTypeList.Length; i++)
		{
			if (i == m_ParticleType)
			{
				if (m_ParticleTypeList[i].m_Buttons.interactable == true)
				{
					m_ParticleTypeList[i].m_Buttons.interactable = false;
					GUIAnimFREE pGUIAnimFREE = m_ParticleTypeList[i].m_Buttons.gameObject.GetComponent<GUIAnimFREE>();
					if (pGUIAnimFREE != null)
					{
						pGUIAnimFREE.m_ScaleOut.Enable = true;
						pGUIAnimFREE.m_ScaleOut.Time = 1.5f;
						pGUIAnimFREE.m_ScaleOut.ScaleEnd = new Vector3(1.25f, 1.25f, 1.25f);
						pGUIAnimFREE.MoveOut();
					}
				}
			}
			else
			{
				if (m_ParticleTypeList[i].m_Buttons.interactable == false)
				{
					m_ParticleTypeList[i].m_Buttons.interactable = true;
					GUIAnimFREE pGUIAnimFREE = m_ParticleTypeList[i].m_Buttons.gameObject.GetComponent<GUIAnimFREE>();
					if (pGUIAnimFREE != null)
					{
						pGUIAnimFREE.m_ScaleIn.Enable = true;
						pGUIAnimFREE.m_ScaleIn.Time = 1.5f;
						pGUIAnimFREE.m_ScaleIn.ScaleBegin = new Vector3(1.25f, 1.25f, 1.25f);
						pGUIAnimFREE.MoveIn();
					}
				}
			}
		}
	}

	#endregion // Particle

	// ########################################
	// UI Responder functions
	// ########################################

	#region UI Responder

	public void Button_SelectDemo()
	{
		GUIAnimSystemFREE.Instance.MoveOut(m_SelectDemo_Button.transform, true);
		GUIAnimSystemFREE.Instance.MoveIn(m_SelectDemo_Window.transform, true);

		FXQ_SoundController.Instance.Play_SoundBack();
	}

	public void Button_SelectDemo_Minimize()
	{
		GUIAnimSystemFREE.Instance.MoveIn(m_SelectDemo_Button.transform, true);
		GUIAnimSystemFREE.Instance.MoveOut(m_SelectDemo_Window.transform, true);

		FXQ_SoundController.Instance.Play_SoundBack();
	}

	public void Button_SelectDemo_2D()
	{
	}

	public void Button_SelectDemo_3D()
	{
		GUIAnimSystemFREE.Instance.LoadLevel("FX Quest 3D Demo (960x600px)", 1.0f);

		FXQ_SoundController.Instance.Play_SoundPress();
	}

	// Toggle full screen
	public void Toggle_FullScreen()
	{
		Screen.fullScreen = !Screen.fullScreen;
		if (!Screen.fullScreen)
		{
			Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
		}
		else
		{
			Screen.SetResolution(m_DefWidth, m_DefHeight, false);
		}
	}

	public void Toggle_ShowSprite()
	{
		UpdateToggleSpriteAndDarkScreen();

		FXQ_SoundController.Instance.Play_SoundClick();
	}

	public void Toggle_DarkScreen()
	{
		UpdateToggleSpriteAndDarkScreen();

		FXQ_SoundController.Instance.Play_SoundClick();
	}

	public void Button_Help()
	{
		GUIAnimSystemFREE.Instance.MoveOut(m_Help_Button.transform, true);
		GUIAnimSystemFREE.Instance.MoveIn(m_Help_Window.transform, true);

		FXQ_SoundController.Instance.Play_SoundBack();
	}

	public void Button_Help_Minimize()
	{
		GUIAnimSystemFREE.Instance.MoveIn(m_Help_Button.transform, true);
		GUIAnimSystemFREE.Instance.MoveOut(m_Help_Window.transform, true);

		FXQ_SoundController.Instance.Play_SoundBack();
	}

	public void Button_Help_Support()
	{
		// http://docs.unity3d.com/ScriptReference/Application.OpenURL.html
		Application.OpenURL("mailto:geteamdev@gmail.com");

		FXQ_SoundController.Instance.Play_SoundClick();
	}

	public void Button_Help_Tutorials()
	{
		// http://docs.unity3d.com/ScriptReference/Application.ExternalEval.html
		//Application.ExternalEval("window.open('https://www.youtube.com/watch?v=TWpKPCGYEyI','FX Quest 0.5.0')");

        // http://docs.unity3d.com/ScriptReference/Application.OpenURL.html
        Application.OpenURL("https://www.youtube.com/watch?v=TWpKPCGYEyI");

        FXQ_SoundController.Instance.Play_SoundClick();
	}

	public void Button_Help_Products()
	{
		// http://docs.unity3d.com/ScriptReference/Application.ExternalEval.html
		//Application.ExternalEval("window.open('https://www.ge-team.com/en/products/','GOLD EXPERIENCE TEAM')");

        // http://docs.unity3d.com/ScriptReference/Application.OpenURL.html
        Application.OpenURL("https://www.ge-team.com/en/products/");

        FXQ_SoundController.Instance.Play_SoundClick();
	}

	public void Button_PlayParticle()
	{
		// Show particle
		ShowParticle();

		FXQ_SoundController.Instance.Play_SoundClick();
	}

	public void Button_PreviousParticle()
	{
		PreviousParticle();

		FXQ_SoundController.Instance.Play_SoundClick();
	}

	public void Button_NextParticle()
	{
		NextParticle();

		FXQ_SoundController.Instance.Play_SoundClick();
	}

	public void Button_ParticleType_Abilities()
	{
		SetParticleType(0);

		FXQ_SoundController.Instance.Play_SoundClick();
	}

	public void Button_ParticleType_Explosion()
	{
		SetParticleType(1);

		FXQ_SoundController.Instance.Play_SoundClick();
	}

	public void Button_ParticleType_Fight()
	{
		SetParticleType(2);

		FXQ_SoundController.Instance.Play_SoundClick();
	}

	public void Button_ParticleType_Magic()
	{
		SetParticleType(3);

		FXQ_SoundController.Instance.Play_SoundClick();
	}

	public void Button_ParticleType_Misc()
	{
		SetParticleType(4);

		FXQ_SoundController.Instance.Play_SoundClick();
	}

	public void Button_ParticleType_Prop()
	{
		SetParticleType(5);

		FXQ_SoundController.Instance.Play_SoundClick();
	}

	public void Button_ParticleType_UI_Front()
	{
		SetParticleType(6);

		FXQ_SoundController.Instance.Play_SoundClick();
	}

	public void Button_ParticleType_UI_Back()
	{
		SetParticleType(7);

		FXQ_SoundController.Instance.Play_SoundClick();
	}

	#endregion // UI Responder
}
