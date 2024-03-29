﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RopeCutterTrail : MonoBehaviour {

	#region Private Variables

	[SerializeField]
	LayerMask m_collisionMask = -1;
	[SerializeField]
	bool m_onlyShowWhenGrabTouch = false; //Need GlobalPressController to Call Grab in correct moment
	[SerializeField, PropertyBackingFieldAttribute]
	bool m_receiveEventsFromGlobalPressController = true;
	[SerializeField]
	bool m_ignoreTimeScale = true; // In Global Scale
	[SerializeField]
	float m_minDistanceToRayCastAll = 0f; // In Global Scales
	[SerializeField]
	float m_minDistanceToCut = 0f; // In Global Scale

	//Aux Variables
	float _trailTime = 0.3f;
	bool _cutting = false;
	//Used To Prevent Deformations in Trail
	float _currentDeltaTime = 0f;
	float _maxDeltaTime = 0.03f;
	Vector2 _oldPosition = Vector2.zero;

	#endregion

	#region Public Properties

	public LayerMask CollisionMask {get {return m_collisionMask;} 
		set 
		{
			if(m_collisionMask == value)
				return;
			m_collisionMask = value;
		}
	}

	public bool Cutting
	{
		get
		{
			return _cutting;
		}
		protected set
		{
			if(_cutting == value)
				return;
			_cutting = value;

			if(gameObject.GetComponent<Collider2D>() != null) 
			{
				gameObject.GetComponent<Collider2D>().enabled = _cutting;
			}
		}
	}

	public bool OnlyShowWhenGrabTouch 
	{
		get
		{
			return m_onlyShowWhenGrabTouch;
		}
		set
		{
			if(m_onlyShowWhenGrabTouch == value)
				return;
			m_onlyShowWhenGrabTouch = value;
		}
	}

	public bool IgnoreTimeScale 
	{
		get
		{
			return m_ignoreTimeScale;
		}
		set
		{
			if(m_ignoreTimeScale == value)
				return;
			m_ignoreTimeScale = value;
		}
	}

	public float MinDistanceToRayCastAll 
	{
		get
		{
			return m_minDistanceToRayCastAll;
		}
		set
		{
			if(m_minDistanceToRayCastAll == value)
				return;
			m_minDistanceToRayCastAll = value;
		}
	}

	public float MinDistanceToCut 
	{
		get
		{
			return m_minDistanceToCut;
		}
		set
		{
			if(m_minDistanceToCut == value)
				return;
			m_minDistanceToCut = value;
		}
	}

	TimedTrailRenderer _trailComponent = null;
	public TimedTrailRenderer TrailComponent 
	{
		get
		{
			if(_trailComponent == null)
				_trailComponent = GetComponent<TimedTrailRenderer>();
			return _trailComponent;
		}
	}

	Camera _cameraThatDrawThisObject = null;
	public Camera CameraThatDrawThisObject
	{
		get
		{
			if(_cameraThatDrawThisObject == null)
				_cameraThatDrawThisObject = CameraManager.GetCameraThatDrawLayer(this.gameObject.layer);
			return _cameraThatDrawThisObject;
		}
	}

	public bool ReceiveEventsFromGlobalPressController {
		get {return m_receiveEventsFromGlobalPressController;} 
		set 
		{
			if(m_receiveEventsFromGlobalPressController == value)
				return;
			m_receiveEventsFromGlobalPressController = value;
			if(Application.isPlaying)
				RegisterEvents();
		}
	}

	#endregion

	#region Unity Functions

	protected virtual void Start()
	{
		Init();
		RegisterEvents();
	}

	protected virtual void OnDestroy()
	{
		UnregisterEvents();
	}

	/*protected virtual void FixedUpdate()
	{
		if(!OnlyShowWhenGrabTouch && IsGrabbing() && FocusContainer.IsUnderFocus(this.gameObject)) // Focus Must Be Null!
			UpdateTrail(Input.GetMouseButton(0));
	}*/

	protected virtual void OnTriggerEnter2D(Collider2D p_collider)
	{
		if(p_collider != null && p_collider.gameObject != null)
		{
			TryCut(p_collider.gameObject);
		}
	}

	protected virtual void LateUpdate()
	{
		TryRayCastAll();
	}

	#endregion

	#region Event Functions

	protected virtual void HandleOnGlobalPress (bool p_isPressed)
	{
		if(p_isPressed && Input.GetMouseButton(0))
			GrabTouch();
		else if(!p_isPressed && IsTouchReferenceEqualsCurrentTouch(KiltUICamera.currentTouch))
			ResetToNonPressingState();

	}

	protected virtual void HandleOnGlobalDrag (Vector2 p_delta)
	{
		if(!OnlyShowWhenGrabTouch && IsGrabbing() && IsTouchReferenceEqualsCurrentTouch(KiltUICamera.currentTouch) && FocusContainer.IsUnderFocus(this.gameObject))
			UpdateTrail(true);
	}
	
	protected virtual void HandleOnGlobalDrop (GameObject p_dropObject)
	{
		if(!OnlyShowWhenGrabTouch && IsGrabbing() && IsTouchReferenceEqualsCurrentTouch(KiltUICamera.currentTouch) && FocusContainer.IsUnderFocus(this.gameObject))
			UpdateTrail(false);
	}

	protected virtual void OnPress(bool p_isPressed)
	{
		if(OnlyShowWhenGrabTouch)
			UpdateTrail(p_isPressed);
	}

	protected virtual void OnDrag(Vector2 p_delta)
	{
		if(OnlyShowWhenGrabTouch)
			UpdateTrail(true);
	}

	protected virtual void OnDrop(GameObject p_object)
	{
		if(OnlyShowWhenGrabTouch)
			UpdateTrail(false);
	}

	#endregion

	#region Init Functions

	public virtual void Init()
	{
		if(TrailComponent != null)
			_trailTime = TrailComponent.lifeTime;
		ResetToNonPressingState(true);
	}

	public virtual void RegisterEvents()
	{
		UnregisterEvents();
		if(ReceiveEventsFromGlobalPressController && GlobalPressController.Instance != null)
		{
			GlobalPressController.OnGlobalPress += HandleOnGlobalPress;
			GlobalPressController.OnGlobalDrag += HandleOnGlobalDrag;
			GlobalPressController.OnGlobalDrop += HandleOnGlobalDrop;
		}
		else
		{
			KiltUICamera.OnGlobalPress += HandleOnGlobalPress;
			KiltUICamera.OnGlobalDrag += HandleOnGlobalDrag;
			KiltUICamera.OnGlobalDrop += HandleOnGlobalDrop;
		}
	}

	public virtual void UnregisterEvents()
	{
		GlobalPressController.OnGlobalPress -= HandleOnGlobalPress;
		GlobalPressController.OnGlobalDrag -= HandleOnGlobalDrag;
		GlobalPressController.OnGlobalDrop -= HandleOnGlobalDrop;
		KiltUICamera.OnGlobalPress -= HandleOnGlobalPress;
		KiltUICamera.OnGlobalDrag -= HandleOnGlobalDrag;
		KiltUICamera.OnGlobalDrop -= HandleOnGlobalDrop;
	}

	#endregion

	#region Tensioned Rope Cutter Functions

	//Used to Avoid Bugs when RayCast pass between node collisors when rope is tensioned
	protected void CheckCollisionWithTensionedRopes()
	{
		List<Rope2D> v_tensionedRopes = GetTensionedCuttableRopes();
		Vector2 v_trail_point1 = _oldPosition;
		Vector2 v_trail_point2 = this.transform.position;
		
		foreach(Rope2D v_rope in v_tensionedRopes)
		{
			if(v_rope.Nodes.Count >= 2)
			{
				GameObject v_firstNode = v_rope.GetAttachedObjectA();
				GameObject v_lastNode = v_rope.GetAttachedObjectB();
				if(v_firstNode != null && v_lastNode != null)
				{
					Vector2 v_rope_point1 = v_firstNode.transform.position;
					Vector2 v_rope_point2 = v_lastNode.transform.position;
					Vector2 v_intersection = Vector2.zero;
					if(VectorHelper.LineIntersection(v_trail_point1, v_trail_point2, v_rope_point1, v_rope_point2, ref v_intersection))
					{
						GameObject v_node = GetNodeWithSmallestDistanteToPoint(v_rope, v_intersection);
						TryCut(v_node);
					}
				}
				else
					continue;
			}
		}
		
	}

	protected List<Rope2D> GetTensionedCuttableRopes()
	{
		List<Rope2D> v_tensionedRopes = new List<Rope2D>();
		foreach(Rope2D v_rope in Rope2D.AllRopesInScene)
		{
			if(v_rope != null && v_rope.IsRopeTensioned() && !v_rope.IsRopeBroken() && v_rope.UserCanCutTheRope)
				v_tensionedRopes.Add(v_rope);
		}
		return v_tensionedRopes;
	}
	
	protected GameObject GetNodeWithSmallestDistanteToPoint(Rope2D p_rope, Vector2 p_point)
	{
		GameObject v_node = null;
		float v_currentDistance = -1;
		if(p_rope != null)
		{
			foreach(GameObject p_node in p_rope.Nodes)
			{
				if(p_node != null)
				{
					if(v_currentDistance < 0)
					{
						v_node = p_node;
						v_currentDistance = Vector2.Distance(p_point, v_node.transform.position);
					}
					else
					{
						float v_thisNodeDistance = Vector2.Distance(p_point, p_node.transform.position);
						if(v_thisNodeDistance < v_currentDistance)
						{
							v_node = p_node;
							v_currentDistance = v_thisNodeDistance;
							continue;
						}
					}
				}
			}
		}
		return v_node;
	}

	#endregion

	#region Cutter Functions

	protected void TryRayCastAll(bool p_force = false)
	{
		float v_distance = Vector2.Distance(_oldPosition, this.transform.position);
		if(p_force || (v_distance >= MinDistanceToRayCastAll && Cutting))
		{
			int v_rayCastlayerMask = CollisionMask.value; //~(1 << gameObject.layer); // everything excent this layer
			RaycastHit2D[] v_hits = Physics2D.LinecastAll(_oldPosition, this.transform.position, v_rayCastlayerMask);

			foreach(RaycastHit2D v_hit in v_hits)
			{
				if(v_hit.collider != null)
				{
					TryCut(v_hit.collider.gameObject);
				}
			}
		}
		if(Cutting)
			CheckCollisionWithTensionedRopes();
	}
	
	protected virtual bool TryCut(GameObject p_object)
	{
		if(p_object != null)
		{
			Node2D v_node = p_object.GetComponent<Node2D>();
			if(v_node != null)
				return v_node.Cut();
		}
		return false;
	}

	#endregion

	#region Touch Functions

	bool _grabbing = false;
	KiltUICamera.MouseOrTouch _touchReference = null;
	//Only works when grabing touch inside HandleGlobalPress
	protected virtual void GrabTouch()
	{
		if(!_grabbing && KiltUICamera.currentTouch != null)
		{
			if(!OnlyShowWhenGrabTouch || KiltUICamera.currentTouch.current == null)
			{
				if(!OnlyShowWhenGrabTouch && !FocusContainer.IsUnderFocus(this.gameObject))
					return;
				_grabbing = true;
				_touchReference = KiltUICamera.currentTouch;
				if(KiltUICamera.currentTouch.current == null)
				{
					KiltUICamera.currentTouch.current = this.gameObject;
					KiltUICamera.currentTouch.pressed = this.gameObject;
					KiltUICamera.currentTouch.dragged = this.gameObject;
				}
				UpdateTrail(true);
			}
		}
	}

	public bool IsGrabbing()
	{
		if(_grabbing || _touchReference != null)
		{
			return true;
		}
		return false;
	}

	public bool IsTouchReferenceEqualsCurrentTouch(KiltUICamera.MouseOrTouch p_currentTouch)
	{

		if(p_currentTouch != null && _touchReference != null && 
		   ( 
		     (p_currentTouch == _touchReference) ||
			 (p_currentTouch.current == _touchReference.current) || 
			 (p_currentTouch.pressed == _touchReference.pressed) ||
			 (p_currentTouch.dragged == _touchReference.dragged)
		   )
		  )
		{
			return true;
		}
		return false;
	}


	public bool HaveTouch()
	{
		if(KiltUICamera.currentTouch != null && 
		   (KiltUICamera.currentTouch.current == this.gameObject 
		 || KiltUICamera.currentTouch.pressed == this.gameObject
		 || KiltUICamera.currentTouch.dragged == this.gameObject)
		   )
		{
			return true;
		}
		return false;
	}

	public void UpdateTrail(bool p_isPressing)
	{
		if(p_isPressing && Input.GetMouseButton(0))
		{
			_grabbing = true;
			if(CameraThatDrawThisObject != null && TrailComponent != null)
			{
				if(Cutting)
					_oldPosition = this.transform.position;
				Vector3 v_position = CameraThatDrawThisObject.ScreenToWorldPoint(Input.mousePosition); //get position, where mouse cursor is
				v_position.z = -1;

				//position cutter object to touch/click position
				TrailComponent.transform.position = v_position;
				
				//Prevent Initial Bug When Showing the Trail
				if(_currentDeltaTime > 0)
					_currentDeltaTime = Mathf.Max(_currentDeltaTime - GetDeltaTime(), 0);

				float v_distance = Vector2.Distance(_oldPosition, this.transform.position);
				if(v_distance > MinDistanceToCut)
				{
					if(!Cutting && _currentDeltaTime <= 0)
					{
						TrailComponent.lifeTime = _trailTime;
						TrailComponent.ClearPoints();
						Cutting = true;
						_oldPosition = v_position;
					}
				}
				else
					Cutting = false;
			}
		}
		else
		{
			ResetToNonPressingState();
		}
	}

	protected virtual void ResetToNonPressingState(bool p_forceDeativateColliders = false)
	{
		if(CameraThatDrawThisObject != null && TrailComponent != null)
		{
			_grabbing = false;
			_touchReference = null;
			Cutting = false;
			TrailComponent.lifeTime = 0;
			TrailComponent.ignoreTimeScale = IgnoreTimeScale;
			_currentDeltaTime = _maxDeltaTime;
			_oldPosition = this.transform.position;
			if(p_forceDeativateColliders && gameObject.GetComponent<Collider2D>() != null) 
				gameObject.GetComponent<Collider2D>().enabled = Cutting;
		}
	}

	#endregion

	#region Time Functions
	
	public float GetTime()
	{
		return IgnoreTimeScale? Time.unscaledTime : Time.time;
	}
	
	public float GetDeltaTime()
	{
		return IgnoreTimeScale? Time.unscaledDeltaTime : Time.deltaTime;
	}
	
	#endregion
}
