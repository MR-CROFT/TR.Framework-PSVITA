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
// FXQ_2D_Demo class
// Moves particles for 2D demo scene.
// ######################################################################

public class FXQ_MoveParticle : MonoBehaviour
{

	// ########################################
	// Variables
	// ########################################

	#region Variables

	public enum eMoveMethod
	{
		LeftRight,
		UpDown,
		CircularXY_Clockwise,
		CircularXY_CounterClockwise,
		CircularXZ_Clockwise,
		CircularXZ_CounterClockwise,
		CircularYZ_Clockwise,
		CircularYZ_CounterClockwise,
	}

	// Current and last move methods
	public eMoveMethod m_MoveMethod = eMoveMethod.LeftRight;
	eMoveMethod m_MoveMethodOld = eMoveMethod.LeftRight;

	// Current and last move range
	public float m_Range = 5.0f;
	float m_RangeOld = 5.0f;

	// Move speed
	public float m_Speed = 2.0f;

	// Directions
	bool m_ToggleDirectionFlag = false;

	#endregion // Variables

	// ########################################
	// MonoBehaviour Functions
	// http://docs.unity3d.com/ScriptReference/MonoBehaviour.html
	// ########################################

	#region MonoBehaviour

	// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
	// http://docs.unity3d.com/ScriptReference/MonoBehaviour.Start.html
	void Start()
	{
	}

	// Update is called every frame, if the MonoBehaviour is enabled.
	// http://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html
	void Update()
	{

		// If m_MoveMethod or m_Range was changed
		if (m_MoveMethod != m_MoveMethodOld || m_Range != m_RangeOld)
		{
			m_MoveMethodOld = m_MoveMethod;
			ResetPosition();
		}

		// Update position according to m_MoveMethod
		switch (m_MoveMethod)
		{
			// Move left-right
			case eMoveMethod.LeftRight:
				UpdateLeftRight();
				break;

			// Move up-down
			case eMoveMethod.UpDown:
				UpdateUpDown();
				break;

			// Move XY clock wise circle
			case eMoveMethod.CircularXY_Clockwise:
				UpdateCircularXY_Clockwise();
				break;

			// Move XY counter clockwise circle
			case eMoveMethod.CircularXY_CounterClockwise:
				UpdateCircularXY_CounterClockwise();
				break;

			// Move XZ clock wise circle
			case eMoveMethod.CircularXZ_Clockwise:
				UpdateCircularXZ_Clockwise();
				break;

			// Move XZ counter clockwise circle
			case eMoveMethod.CircularXZ_CounterClockwise:
				UpdateCircularXZ_CounterClockwise();
				break;

			// Move YZ clock wise circle
			case eMoveMethod.CircularYZ_Clockwise:
				UpdateCircularYZ_Clockwise();
				break;

			// Move YZ counter clock wise circle
			case eMoveMethod.CircularYZ_CounterClockwise:
				UpdateCircularYZ_CounterClockwise();
				break;
		}

	}

	#endregion // MonoBehaviour

	// ########################################
	// Update Position Functions
	// ########################################

	#region Update Positions

	// Reset the positon of this Particle object
	void ResetPosition()
	{
		switch (m_MoveMethod)
		{
			// Reset position for left-right and up-down move method
			case eMoveMethod.LeftRight:
			case eMoveMethod.UpDown:
				this.transform.localPosition = new Vector3(0, 0, 0);
				break;

			// Reset position for XY circle, XZ circle move method
			case eMoveMethod.CircularXY_Clockwise:
			case eMoveMethod.CircularXY_CounterClockwise:
			case eMoveMethod.CircularXZ_Clockwise:
			case eMoveMethod.CircularXZ_CounterClockwise:
				this.transform.localPosition = new Vector3(m_Range, 0, 0);
				break;

			// Reset position for YZ circle move method
			case eMoveMethod.CircularYZ_Clockwise:
			case eMoveMethod.CircularYZ_CounterClockwise:
				this.transform.localPosition = new Vector3(0, 0, m_Range);
				break;
		}
		m_RangeOld = m_Range;
	}

	// Move left-right
	void UpdateLeftRight()
	{
		// moving to left
		if (m_ToggleDirectionFlag == false)
		{
			this.transform.localPosition = new Vector3(this.transform.localPosition.x - (m_Speed * Time.deltaTime), 0, 0);
			if (this.transform.localPosition.x <= -m_Range)
			{
				m_ToggleDirectionFlag = true;
			}
		}
		// moving to right
		else
		{
			this.transform.localPosition = new Vector3(this.transform.localPosition.x + (m_Speed * Time.deltaTime), 0, 0);
			if (this.transform.localPosition.x >= m_Range)
			{
				m_ToggleDirectionFlag = false;
			}
		}
	}

	// Move up-down
	void UpdateUpDown()
	{
		// moving up
		if (m_ToggleDirectionFlag == false)
		{
			this.transform.localPosition = new Vector3(0, this.transform.localPosition.y + (m_Speed * Time.deltaTime), 0);
			if (this.transform.localPosition.y >= m_Range)
			{
				m_ToggleDirectionFlag = true;
			}
		}
		// moving down
		else
		{
			this.transform.localPosition = new Vector3(0, this.transform.localPosition.y - (m_Speed * Time.deltaTime), 0);
			if (this.transform.localPosition.y <= -m_Range)
			{
				m_ToggleDirectionFlag = false;
			}
		}
	}

	// Move XY clock wise circle
	void UpdateCircularXY_Clockwise()
	{
		float centerX = 0;
		float centerY = 0;
		float point2x = this.transform.localPosition.x;
		float point2y = this.transform.localPosition.y;
		float newX = centerX + ((point2x - centerX) * Mathf.Cos(m_Speed / 360.0f) - (point2y - centerY) * Mathf.Sin(m_Speed / 360.0f));
		float newY = centerY + ((point2x - centerX) * Mathf.Sin(m_Speed / 360.0f) + (point2y - centerY) * Mathf.Cos(m_Speed / 360.0f));

		this.transform.localPosition = new Vector3(newX, newY, 0);
	}

	// Move XY counter clockwise circle
	void UpdateCircularXY_CounterClockwise()
	{
		float centerX = 0;
		float centerY = 0;
		float point2x = this.transform.localPosition.x;
		float point2y = this.transform.localPosition.y;
		float newX = centerX + ((point2x - centerX) * Mathf.Cos(-m_Speed / 360.0f) - (point2y - centerY) * Mathf.Sin(-m_Speed / 360.0f));
		float newY = centerY + ((point2x - centerX) * Mathf.Sin(-m_Speed / 360.0f) + (point2y - centerY) * Mathf.Cos(-m_Speed / 360.0f));

		this.transform.localPosition = new Vector3(newX, newY, 0);
	}

	// Move XZ clock wise circle
	void UpdateCircularXZ_Clockwise()
	{
		float centerX = 0;
		float centerZ = 0;
		float point2x = this.transform.localPosition.x;
		float point2z = this.transform.localPosition.z;
		float newX = centerX + ((point2x - centerX) * Mathf.Cos(m_Speed / 360.0f) - (point2z - centerZ) * Mathf.Sin(m_Speed / 360.0f));
		float newZ = centerZ + ((point2x - centerX) * Mathf.Sin(m_Speed / 360.0f) + (point2z - centerZ) * Mathf.Cos(m_Speed / 360.0f));

		this.transform.localPosition = new Vector3(newX, 0, newZ);
	}

	// Move XZ counter clockwise circle
	void UpdateCircularXZ_CounterClockwise()
	{
		float centerX = 0;
		float centerZ = 0;
		float point2x = this.transform.localPosition.x;
		float point2z = this.transform.localPosition.z;
		float newX = centerX + ((point2x - centerX) * Mathf.Cos(-m_Speed / 360.0f) - (point2z - centerZ) * Mathf.Sin(-m_Speed / 360.0f));
		float newZ = centerZ + ((point2x - centerX) * Mathf.Sin(-m_Speed / 360.0f) + (point2z - centerZ) * Mathf.Cos(-m_Speed / 360.0f));

		this.transform.localPosition = new Vector3(newX, 0, newZ);
	}

	// Move YZ clock wise circle
	void UpdateCircularYZ_Clockwise()
	{
		float centerY = 0;
		float centerZ = 0;
		float point2y = this.transform.localPosition.y;
		float point2z = this.transform.localPosition.z;
		float newY = centerY + ((point2y - centerY) * Mathf.Cos(m_Speed / 360.0f) - (point2z - centerZ) * Mathf.Sin(m_Speed / 360.0f));
		float newZ = centerZ + ((point2y - centerY) * Mathf.Sin(m_Speed / 360.0f) + (point2z - centerZ) * Mathf.Cos(m_Speed / 360.0f));

		this.transform.localPosition = new Vector3(0, newY, newZ);
	}

	// Move YZ counter clock wise circle
	void UpdateCircularYZ_CounterClockwise()
	{
		float centerY = 0;
		float centerZ = 0;
		float point2y = this.transform.localPosition.y;
		float point2z = this.transform.localPosition.z;
		float newY = centerY + ((point2y - centerY) * Mathf.Cos(-m_Speed / 360.0f) - (point2z - centerZ) * Mathf.Sin(-m_Speed / 360.0f));
		float newZ = centerZ + ((point2y - centerY) * Mathf.Sin(-m_Speed / 360.0f) + (point2z - centerZ) * Mathf.Cos(-m_Speed / 360.0f));

		this.transform.localPosition = new Vector3(0, newY, newZ);
	}

	#endregion // Update Positions
}
